using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyWhenBoolAttribute))]
public class ReadOnlyWhenBoolAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var parameters = (ReadOnlyWhenBoolAttribute)attribute;
		var controllingPropertyPath = $"{GetParentPath(property)}{parameters.PropertyString}";
		var controllingProperty = property.serializedObject.FindProperty(controllingPropertyPath);

		bool isReadOnly = controllingProperty is { propertyType: SerializedPropertyType.Boolean }
						  && controllingProperty.boolValue == parameters.PropertyBool;

		using (new EditorGUI.DisabledScope(isReadOnly))
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}

	private string GetParentPath(SerializedProperty property)
	{
		var pathParts = property.propertyPath.Split('.');
		return pathParts.Length > 1 ? string.Join(".", pathParts.Take(pathParts.Length - 1)) + "." : string.Empty;
	}
}