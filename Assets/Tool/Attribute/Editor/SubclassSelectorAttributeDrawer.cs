using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		if (property.propertyType != SerializedPropertyType.ManagedReference)
		{
			EditorGUI.LabelField(position, label.text, "Use with [SerializeReference]");
			EditorGUI.EndProperty();
			return;
		}

		var attr = (SubclassSelectorAttribute)attribute;
		var baseType = attr.BaseType;
		var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
			.ToArray();

		string[] options = derivedTypes.Select(t => t.FullName).ToArray();
		int currentIndex = Array.FindIndex(derivedTypes, t => t == property.managedReferenceValue?.GetType());

		Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		int selected = EditorGUI.Popup(dropdownRect, label.text, currentIndex, options);

		if (selected != currentIndex && selected >= 0)
		{
			var newInstance = Activator.CreateInstance(derivedTypes[selected]);
			property.managedReferenceValue = newInstance;
			property.isExpanded = true;
		}

		if (property.isExpanded && property.managedReferenceValue != null)
		{
			EditorGUI.indentLevel++;
			SerializedProperty iterator = property.Copy();
			var end = iterator.GetEndProperty();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
			{
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.PropertyField(propRect, iterator, true);
				enterChildren = false;
			}
			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded && property.managedReferenceValue != null)
		{
			SerializedProperty iterator = property.Copy();
			var end = iterator.GetEndProperty();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
			{
				height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
				enterChildren = false;
			}
		}

		return height;
	}
}