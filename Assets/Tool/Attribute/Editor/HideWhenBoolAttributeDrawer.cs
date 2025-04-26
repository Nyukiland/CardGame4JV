using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideWhenBoolAttribute))]
public class HideWhenBoolAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var parameters = (HideWhenBoolAttribute)attribute;
		var controllingPropertyPath = $"{GetParentPath(property)}{parameters.PropertyString}";
		var controllingProperty = property.serializedObject.FindProperty(controllingPropertyPath);

		bool isVisible = controllingProperty is { propertyType: SerializedPropertyType.Boolean }
						  && controllingProperty.boolValue == parameters.PropertyBool;

		if (isVisible) EditorGUI.PropertyField(position, property, label, true);
		
	}

	private string GetParentPath(SerializedProperty property)
	{
		var pathParts = property.propertyPath.Split('.');
		return pathParts.Length > 1 ? string.Join(".", pathParts.Take(pathParts.Length - 1)) + "." : string.Empty;
	}
}
