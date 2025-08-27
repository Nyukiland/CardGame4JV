using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

public class MeshOverlapAlphaFeature : ScriptableRendererFeature
{
	[System.Serializable]
	public class Settings
	{
		public List<MeshRenderer> TargetMeshes = new();
		public Shader AccumulationShader;
		public string TextureName = "_OverlapCountTex";
	}

	public Settings settings = new();

	class AccumulationPass : ScriptableRenderPass
	{
		private Settings _settings;
		private Material _accumulationMat;
		private List<MeshRenderer> _meshes = new();

		public AccumulationPass(Settings settings)
		{
			_settings = settings;
			if (settings.AccumulationShader != null)
				_accumulationMat = CoreUtils.CreateEngineMaterial(settings.AccumulationShader);
		}

		public void SetMeshes(List<MeshRenderer> list)
		{
			_meshes = list;
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
			RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;

			desc.depthBufferBits = 0;
			desc.msaaSamples = 1;
			desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm; // single-channel

			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			TextureDesc depthDesc = renderGraph.GetTextureDesc(resourceData.activeDepthTexture);

			TextureHandle accumulationTex = renderGraph.CreateTexture(
				new TextureDesc(depthDesc.width, depthDesc.height)
				{
					colorFormat = GraphicsFormat.R8_UNorm,
					depthBufferBits = DepthBits.None,
					msaaSamples = depthDesc.msaaSamples,
					name = _settings.TextureName,
					clearBuffer = true,
					clearColor = Color.clear
				});


			using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
				"Mesh Overlap Accumulation", out PassData passData))
			{
				builder.SetRenderAttachment(accumulationTex, 0);

				builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);

				builder.AllowPassCulling(false);

				builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
				{
					if (_accumulationMat == null || _meshes == null || _meshes.Count == 0)
						return;

					foreach (MeshRenderer meshRenderer in _meshes)
					{
						if (meshRenderer == null) continue;
						MeshFilter mf = meshRenderer.GetComponent<MeshFilter>();
						if (mf != null && mf.sharedMesh != null)
						{
							ctx.cmd.DrawMesh(
								mf.sharedMesh,
								meshRenderer.transform.localToWorldMatrix,
								_accumulationMat);
						}
					}
				});
			}

			using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
				"Expose OverlapTex", out PassData passData))
			{
				builder.UseTexture(accumulationTex);
				builder.AllowPassCulling(false);
				builder.AllowGlobalStateModification(true);

				builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
				{
					ctx.cmd.SetGlobalTexture(_settings.TextureName, accumulationTex);
				});
			}
		}

		private class PassData { }
	}

	private AccumulationPass _accumulationPass;

	public override void Create()
	{
		_accumulationPass = new(settings)
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingTransparents
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		_accumulationPass.SetMeshes(settings.TargetMeshes);
		renderer.EnqueuePass(_accumulationPass);
	}
}