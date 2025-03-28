using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyWhenBoolAttribute))]
public class ReadOnlyWhenBoolAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ReadOnlyWhenBoolAttribute param = (ReadOnlyWhenBoolAttribute)attribute;

		//serializedObject
	}
}