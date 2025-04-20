using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeSelector))]
public class TypeSelectorDrawer : PropertyDrawer
{
	private static readonly Dictionary<Type, string[]> _typeCache = new();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var baseType = ((TypeSelector)attribute).Type;

		if (!_typeCache.TryGetValue(baseType, out var subclassNames))
		{
			subclassNames = FetchSubclassNames(baseType);
			_typeCache[baseType] = subclassNames;
		}

		int currentIndex = Array.IndexOf(subclassNames, property.stringValue);
		currentIndex = Mathf.Clamp(currentIndex, 0, subclassNames.Length - 1);

		int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, subclassNames);
		property.stringValue = subclassNames[selectedIndex];
	}

	private static string[] FetchSubclassNames(Type baseType)
	{
		return AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
			.Select(type => type.FullName)
			.ToArray();
	}
}