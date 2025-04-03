using UnityEditor;
using UnityEngine;

public class LockUserWindow : EditorWindow
{
	[MenuItem("Tools/LockUser")]
	public static void ShowWindow()
	{
	    LockUserWindow window = GetWindow<LockUserWindow>();
	    window.titleContent = new GUIContent("LockUser");
	    window.Show();
	}
	
	private void OnGUI()
	{
		GUILayout.Space(10);

		GUIStyle style = new();
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		style.fontSize = 18;
		style.fontStyle = FontStyle.Bold;

		GUILayout.Label("Lock User", style);

		GUILayout.Space(10);
	}
}