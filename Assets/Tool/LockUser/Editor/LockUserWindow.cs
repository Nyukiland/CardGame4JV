using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

public class LockUserWindow : EditorWindow
{
	public bool AllowAccess;

	private List<string> _listUser = new();
	private ReorderableList _reorderableList;

	[MenuItem("Tools/LockUser")]
	public static void ShowWindow()
	{
	    LockUserWindow window = GetWindow<LockUserWindow>();
	    window.titleContent = new GUIContent("LockUser");
	    window.Show();
	}

	private void OnEnable()
	{
		_listUser = LockUserUtility.ReadLockJsonFile();
		SetListVisu();
	}

	private void OnGUI()
	{
		if (AllowAccess)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.red;
			GUI.Box(new Rect(0, 0, position.width, position.height), "");
			GUI.backgroundColor = prevColor;
		}

		string currentUser = System.Environment.UserName;

		GUILayout.Space(10);

		GUIStyle style = new();
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		style.fontSize = 18;
		style.fontStyle = FontStyle.Bold;

		GUILayout.Label("Lock User", style);

		GUILayout.Space(20);

		GUILayout.BeginHorizontal();

		GUILayout.Label("Current User: ", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		GUILayout.Box(currentUser);
		GUI.enabled = !_listUser.Contains(currentUser);
		if (GUILayout.Button("Add User"))
		{
			_listUser.Add(currentUser);
			LockUserUtility.SaveLockJsonFile(_listUser);
		}
		GUI.enabled = true;

		GUILayout.EndHorizontal();

		GUILayout.Space(20);

		_reorderableList.DoLayoutList();

		GUILayout.FlexibleSpace();

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(10));

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		AllowAccess = GUILayout.Toggle(AllowAccess, "Allow Access");
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
	}

	void SetListVisu()
	{
		_reorderableList = new ReorderableList(_listUser, typeof(string), true, true, false, false);

		_reorderableList.drawHeaderCallback = (Rect rect) =>
		{
			rect = new Rect(rect.x + 10, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(rect, "Locked Users", EditorStyles.boldLabel);
		};

		_reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			Rect textFieldRect = new Rect(rect.x, rect.y + 2, rect.width - 60, EditorGUIUtility.singleLineHeight);
			Rect buttonRect = new Rect(rect.x + rect.width - 75, rect.y + 2, 75, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(textFieldRect, _listUser[index]);

			if (GUI.Button(buttonRect, "Remove"))
			{
				_listUser.RemoveAt(index);
				LockUserUtility.SaveLockJsonFile(_listUser);
				SetListVisu();
			}
		};
	}
}