using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

//based on Mackysoft's work
//code : https://github.com/mackysoft/Unity-SerializeReferenceExtensions/blob/main/Assets/MackySoft/MackySoft.SerializeReferenceExtensions/Editor/SubclassSelectorDrawer.cs

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorAttributeDrawer : PropertyDrawer
{
	private static readonly Dictionary<Type, Type[]> DerivedTypesCache = new();

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
		Type baseType = attr.BaseType;

		// Get or cache derived types
		if (!DerivedTypesCache.TryGetValue(baseType, out Type[] derivedTypes))
		{
			derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
				.ToArray();

			DerivedTypesCache[baseType] = derivedTypes;
		}

		// Draw foldout
		Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
		position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		// Draw type selector popup
		int currentIndex = Array.FindIndex(derivedTypes, t => t == property.managedReferenceValue?.GetType());
		string[] options = derivedTypes.Select(t => t.FullName).ToArray();

		Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		int selected = EditorGUI.Popup(popupRect, "Type", currentIndex, options);
		position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		if (selected != currentIndex && selected >= 0)
		{
			var newInstance = Activator.CreateInstance(derivedTypes[selected]);
			property.managedReferenceValue = newInstance;
			property.isExpanded = true;
		}

		// Draw child properties
		if (property.isExpanded && property.managedReferenceValue != null)
		{
			EditorGUI.indentLevel++;
			SerializedProperty iterator = property.Copy();
			SerializedProperty end = iterator.GetEndProperty();

			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
			{
				float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
				Rect childRect = new Rect(position.x, position.y, position.width, childHeight);
				EditorGUI.PropertyField(childRect, iterator, true);
				position.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
				enterChildren = false;
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;

		if (property.isExpanded && property.managedReferenceValue != null)
		{
			SerializedProperty iterator = property.Copy();
			SerializedProperty end = iterator.GetEndProperty();

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