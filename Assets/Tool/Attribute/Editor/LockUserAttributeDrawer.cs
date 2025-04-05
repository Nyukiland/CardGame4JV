using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LockUserAttribute))]
public class LockUserAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = LockUserUtility.CheckCanEdit();
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}