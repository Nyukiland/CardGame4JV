using UnityEditor;
using UnityEngine;
using System.IO;

namespace ScriptTemplateTool
{
	public class ScriptTemplateContextMenu : Editor
	{
		[MenuItem("Assets/Create/Script/MonoBehaviour Script", false, 55)]
		public static void CreateMonoBehaviourScript()
		{
			CreateScriptFromTemplate("MonoBehaviourTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Empty Script", false, 55)]
		public static void CreateEmptyScript()
		{
			CreateScriptFromTemplate("EmptyScriptTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Editor Window Script", false, 55)]
		public static void CreateEditorWindowScript()
		{
			CreateScriptFromTemplate("EditorWindowTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Editor Script", false, 55)]
		public static void CreateEditorScript()
		{
			CreateScriptFromTemplate("EditorTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Scriptable Object Script", false, 55)]
		public static void CreateScriptableScript()
		{
			CreateScriptFromTemplate("ScriptableObjectTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Singleton", false, 55)]
		public static void CreateSingleton()
		{
			CreateScriptFromTemplate("SingletonTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/State", false, 55)]
		public static void CreateState()
		{
			CreateScriptFromTemplate("StateTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/CombinedState", false, 55)]
		public static void CreateCombinedState()
		{
			CreateScriptFromTemplate("CombinedStateTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Ability", false, 55)]
		public static void CreateAbility()
		{
			CreateScriptFromTemplate("AbilityTemplate.txt");
		}

		[MenuItem("Assets/Create/Script/Resource", false, 55)]
		public static void CreateResource()
		{
			CreateScriptFromTemplate("ResourceTemplate.txt");
		}

		private static void CreateScriptFromTemplate(string templateName)
		{
			string[] g = AssetDatabase.FindAssets($"t:Script {nameof(ScriptTemplateContextMenu)}");
			string filePath = AssetDatabase.GUIDToAssetPath(g[0]);
			int lastSlash = filePath.LastIndexOf('/');
			int secondLastSlash = filePath.LastIndexOf('/', lastSlash - 1);
			string trimmedPath = filePath.Substring(0, secondLastSlash);
			string templatePath = trimmedPath + "/" + templateName;

			if (!File.Exists(templatePath))
			{
				Debug.LogError($"Template file not found: {templatePath}");
				return;
			}

			string selectedPath = GetSelectedPath();
			string defaultFileName = "NewScript.cs";

			if (string.IsNullOrEmpty(selectedPath))
			{
				Debug.LogError("SelectedPath not Found");
				return;
			}

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance<DoCreateScriptAsset>(),
				Path.Combine(selectedPath, defaultFileName),
				null,
				File.ReadAllText(templatePath)
			);
		}

		private static string GetSelectedPath()
		{
			string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (string.IsNullOrEmpty(selectedPath))
			{
				return "";
			}

			if (AssetDatabase.IsValidFolder(selectedPath))
			{
				return selectedPath;
			}

			return Path.GetDirectoryName(selectedPath);
		}

		private class DoCreateScriptAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
		{
			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				string scriptName = Path.GetFileNameWithoutExtension(pathName);
				string scriptContent = resourceFile.Replace("#SCRIPTNAME#", scriptName);

				File.WriteAllText(pathName, scriptContent);
				AssetDatabase.Refresh();

				Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
				ProjectWindowUtil.ShowCreatedAsset(asset);
			}
		}
	}
}