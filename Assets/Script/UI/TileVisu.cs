using CardGame.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CardGame.UI
{
	public class TileVisu : MonoBehaviour, ISelectableInfo
	{
		[SerializeField]
		private string _layerGrid;
		[SerializeField]
		private string _layerHand;

		[Space(10)]

		[SerializeField]
		private MeshRenderer _visuValidity;
		[SerializeField]
		private MeshRenderer _visuNorth;
		[SerializeField]
		private MeshRenderer _visuSouth;
		[SerializeField]
		private MeshRenderer _visuEast;
		[SerializeField]
		private MeshRenderer _visuWest;
		[SerializeField]
		private MeshRenderer _visuCenter;

		[Space(10)]

		//Enviro
		[SerializeField] private Material _neutral;
		[SerializeField] private Material _grass;
		[SerializeField] private Material _water;
		[SerializeField] private Material _fields;
		[SerializeField] private Material _terrain;
		[SerializeField] private Material _greyBase;
		[SerializeField] private Material _validTile;

		[Space(10)]

		[SerializeField] private TMPro.TextMeshPro _ownerTextMeshPro;

		[SerializeField] private List<GameObject> _meshesPresetList = new();
		private GameObject _currentVisualMesh;
		[SerializeField] private GameObject _baseVisualMesh; // La tile grise de base, 
		[SerializeField] private GameObject _flagPrefab; // La tile grise de base, 

		public TileData TileData { get; set; }

		public Vector2 PositionOnGrid { get; private set; }

		public bool IsLinked = true; // Sert pour la preview des placements possibles

		[Space(10)]

		[Header("Tile Rotation speed")]
		[SerializeField] private float _backMovement = 0.1f;
		[SerializeField] private float _rotationMovement = 0.2f;
		[SerializeField] private float _backMovementAgain =0.1f;


		private Sequence _rotationSequence;

		private void Start()
		{
			UpdateTile(TileData);
			_ownerTextMeshPro.text = "";
		}

		private void OnDestroy()
		{
			if (TileData != null) TileData.OnTileRotated -= SetMeshRotationOnSet;
		}

		public void UpdateTile(TileData data)
		{
			TileData = data;
			UpdateVisu();
		}

		public void ChangeParent(Transform parent)
		{
			gameObject.transform.SetParent(parent);
		}

		private void UpdateVisu()
		{

			List<ZoneData> zones = new();

			if (TileData == null) return;

			_ownerTextMeshPro.text = TileData.OwnerPlayerIndex >= 0 ? $"P{TileData.OwnerPlayerIndex}" : "";

			SetTileMesh(TileData.TileSettings.tilePreset); // Y a une sécu dedans pour eviter de tout reconfig
		}

		public void ChangeValidityVisual(bool isValid)
		{
			_baseVisualMesh.GetComponentInChildren<MeshRenderer>().material = isValid ? _validTile : _greyBase;
		}

		public void ResetValidityVisual()
		{
			_baseVisualMesh.GetComponentInChildren<MeshRenderer>().material = _greyBase;
		}

		private Material GetMaterialForType(ENVIRONEMENT_TYPE type)
		{
			switch (type)
			{
				case ENVIRONEMENT_TYPE.Neutral:
					return _neutral;
				case ENVIRONEMENT_TYPE.Grass:
					return _grass;
				case ENVIRONEMENT_TYPE.Fields:
					return _fields;
				case ENVIRONEMENT_TYPE.Water:
					return _water;
				case ENVIRONEMENT_TYPE.Terrain:
					return _terrain;
			}

			return null;
		}

		public void SetTileLayerGrid(bool isOnGrid)
		{
			if (isOnGrid) gameObject.layer = LayerMask.NameToLayer(_layerGrid);
			else gameObject.layer = LayerMask.NameToLayer(_layerHand);
		}

		public void SetTilePosOnGrid(Vector2 pos) => PositionOnGrid = pos;

		/// <summary> Sets the mesh, spawns the flag, sub to rotate </summary>
		public void SetTileMesh(TilePreset preset)
		{
			if (_currentVisualMesh != null)
				return;

			_baseVisualMesh.SetActive(false); // Le mesh gris de base

			TileData.OnTileRotated += SetMeshRotationOnSet; // Pas besoin de securiser, on peut arriver ici qu'une fois grace au return

			GameObject visualPrefab = _meshesPresetList[(int)preset]; // Recup le bon prefab
			_currentVisualMesh = Instantiate(visualPrefab, transform); // Spawn en enfant

			var renderers = _currentVisualMesh.GetComponentsInChildren<MeshRenderer>(); // On recup les renderer, pour set materials
			ZoneData[] zones = TileData.GetUnrotatedZones();

			switch (preset) // Ici je me base sur les 5 presets de tiles : ils ont tous un nbr de mesh different, je dois donc adapter au cas par cas
			{
				case TilePreset.FourDifferentClosed: // 4 tiles dif
					for (int i = 0; i < 4 && i < renderers.Length; i++)
						renderers[i].material = GetMaterialForType(zones[i].environment); // Classic
					break;

				case TilePreset.ThreeSame:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North
					renderers[1].material = GetMaterialForType(zones[1].environment); // East + South + West
					break;

				case TilePreset.DiagonalOpenFull:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North + Est
					renderers[1].material = GetMaterialForType(zones[2].environment); // South + West
					break;

				case TilePreset.DiagonalOpenHalf:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North
					renderers[1].material = GetMaterialForType(zones[2].environment); // East
					renderers[2].material = GetMaterialForType(zones[3].environment); // South + West
					break;

				case TilePreset.Path:
					renderers[0].material = GetMaterialForType(zones[0].environment); // North + South
					renderers[1].material = GetMaterialForType(zones[1].environment); // East
					renderers[2].material = GetMaterialForType(zones[3].environment); // West
					break;
			}

			SetMeshRotationOnSet(); // On met le visual dans la même rota que la tile

			if (TileData.HasFlag) // On spawn le flag if needed
			{
				GameObject flag = Instantiate(_flagPrefab, transform);
			}
		}

		private void SetMeshRotationOnSet()
		{
			if (!_currentVisualMesh) return;

			Transform visual = _currentVisualMesh.transform;

			float targetRotation = (TileData.TileRotationCount % 4) * -90f;

			Sequence seq = DOTween.Sequence();

			seq.Append(visual.DOLocalRotate(new Vector3(0, 0, ((TileData.TileRotationCount -1 + 4) % 4) * -90f + 10f), 0.1f)); // recul (position avant la rota)
			seq.Append(visual.DOLocalRotate(new Vector3(0, 0, targetRotation - 10f), 0.2f).SetEase(Ease.InOutQuad)); // Rota vers la new rotation + 10
			seq.Append(visual.DOLocalRotate(new Vector3(0, 0, targetRotation), 0.1f).SetEase(Ease.OutQuad)); // Rota to real location

			seq.Play();
		}
		public string GetInfo()
		{
			string text = $"[{nameof(TileVisu)}] : {PositionOnGrid.x} - {PositionOnGrid.y}\n " +
				$"Flag : {TileData.HasFlag}\n \n";

			if (TileData == null) return text;

			text += $"Direction 0 (Nord) :\n Region {TileData.Zones[0].Region.GetHashCode()} de type {TileData.Zones[0].environment} \n " +
					$"Nombre d'Ouverture :  {TileData.Zones[0].Region.OpeningCount} \n" +
					$"Nombre de Tuiles :  {TileData.Zones[0].Region.Tiles.Count} \n \n" +

					$"Direction 1 (Est) :\n Region {TileData.Zones[1].Region.GetHashCode()} de type {TileData.Zones[1].environment} \n" +
										$"Nombre d'Ouverture :  {TileData.Zones[1].Region.OpeningCount} \n" +
					$"Nombre de Tuiles :  {TileData.Zones[1].Region.Tiles.Count} \n \n" +

					$"Direction 2 (Sud) :\n Region {TileData.Zones[2].Region.GetHashCode()} de type {TileData.Zones[2].environment} \n" +
					$"Nombre d'Ouverture :  {TileData.Zones[2].Region.OpeningCount} \n" +
					$"Nombre de Tuiles :  {TileData.Zones[2].Region.Tiles.Count} \n \n" +

					$"Direction 3 (Ouest) :\n Region {TileData.Zones[3].Region.GetHashCode()} de type {TileData.Zones[3].environment} \n" +
					$"Nombre d'Ouverture :  {TileData.Zones[3].Region.OpeningCount} \n" +
					$"Nombre de Tuiles :  {TileData.Zones[3].Region.Tiles.Count}  \n";

			return text;
		}
	}
}