using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagPickerAttribute))]
public class TagPickerAttributeDrawer : PropertyDrawer
{
	private static readonly Dictionary<string, string[]> _storage = new();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		string[] tags = GetTags();
		float buttonSize = position.height + 10f;

		Rect fieldRect = new(position) { xMax = position.xMax - buttonSize - EditorGUIUtility.standardVerticalSpacing };
		Rect buttonRect = new(position) { x = fieldRect.xMax + EditorGUIUtility.standardVerticalSpacing, width = buttonSize };

		property.stringValue = DrawTagPicker(fieldRect, label.text, property.stringValue, tags);
		if (GUI.Button(buttonRect, "r")) _storage.Remove("Tag");
	}

	private string DrawTagPicker(Rect position, string label, string text, string[] tags)
	{
		text = EditorGUI.TextField(new Rect(position) { xMax = position.xMax - position.height - EditorGUIUtility.standardVerticalSpacing }, label, text);

		GUI.enabled = tags.Length > 0;
		int index = Array.IndexOf(tags, text);
		index = EditorGUI.Popup(new Rect(position) { x = position.xMax - position.height, width = position.height }, index, tags);
		GUI.enabled = true;

		return index >= 0 ? tags[index] : text;
	}

	private string[] GetTags()
	{
		if (!_storage.TryGetValue("Tag", out string[] tags))
		{
			tags = InternalEditorUtility.tags;
			_storage["Tag"] = tags;
		}
		return tags;
	}
}