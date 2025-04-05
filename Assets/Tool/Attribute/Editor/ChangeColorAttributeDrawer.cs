using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChangeColorAttribute))]
public class ChangeColorAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ChangeColorAttribute param = (ChangeColorAttribute)attribute;

		Color prevColor = GUI.color;
		Color prevBackgroundColor = GUI.backgroundColor;

		GUI.backgroundColor = param.PropertyColor;
		GUI.color = param.PropertyColor;
		EditorGUI.PropertyField(position, property, label);

		if (param.PropertyResetColor)
		{
			GUI.backgroundColor = prevBackgroundColor;
			GUI.color = prevColor;
		}
	}
}