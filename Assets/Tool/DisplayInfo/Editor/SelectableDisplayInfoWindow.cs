using UnityEditor;
using UnityEngine;

public class SelectableDisplayInfoWindow : EditorWindow
{
	private string _toDisplay;

	[MenuItem("Tools/Selectable Display Info")]
	public static void ShowWindow()
	{
	    SelectableDisplayInfoWindow window = GetWindow<SelectableDisplayInfoWindow>();
	    window.titleContent = new GUIContent("Selectable Display Info");
	    window.Show();
	}

	private void OnEnable()
	{
		Selection.selectionChanged += UpdateSelection;
		EditorApplication.update += UpdateEditor;
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= UpdateSelection;
		EditorApplication.update -= UpdateEditor;
	}

	private void UpdateEditor()
	{
		Repaint();
	}

	private void OnGUI()
	{
		GUILayout.Space(10);

		GUIStyle style = new();
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		style.fontSize = 18;
		style.fontStyle = FontStyle.Bold;

		GUILayout.Label("Selectable Display Info", style);

		GUILayout.Space(20);

		GUILayout.Box(_toDisplay, GUILayout.ExpandWidth(true));
	}

	void UpdateSelection()
	{
		_toDisplay = "";

		foreach (var gameObject in Selection.gameObjects)
		{
			ISelectableInfo[] infos = gameObject.GetComponentsInChildren<ISelectableInfo>();
			if (infos.Length <= 0) continue;

			_toDisplay += $"--- {gameObject.name} ---";

			foreach(ISelectableInfo info in infos)
			{
				_toDisplay += "\n" + info.GetInfo();
			}

			_toDisplay += "\n";
			_toDisplay += "\n";
		}

		if (_toDisplay == "")
		{
			_toDisplay = "------------ No Selected GameObject ------------";
		}
	}
}