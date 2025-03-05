using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChangeColorAttribute))]
public class ChangeColorAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ChangeColorAttribute param = (ChangeColorAttribute)attribute;

		Color prevColor = GUI.backgroundColor;

		GUI.backgroundColor = param.PropertyColor;
		EditorGUI.PropertyField(position, property, label);

		if (param.PropertyResetColor) GUI.backgroundColor = prevColor;
	}
}