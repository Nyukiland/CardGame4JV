using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(TypeSelector))]
public class TypeSelectorDrawer : PropertyDrawer
{
	private static readonly Dictionary<Type, AdvancedDropdownState> _states = new();
	private static readonly Dictionary<Type, TypeDropdown> _dropdowns = new();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Type baseType = ((TypeSelector)attribute).Type;
		string currentTypeName = property.stringValue;

		if (!_states.TryGetValue(baseType, out var state))
		{
			state = new AdvancedDropdownState();
			_states[baseType] = state;
		}

		if (!_dropdowns.TryGetValue(baseType, out var dropdown))
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
				.ToList();

			dropdown = new TypeDropdown(types, state);
			dropdown.OnTypeSelected += selectedType =>
			{
				property.stringValue = selectedType?.FullName ?? string.Empty;
				property.serializedObject.ApplyModifiedProperties();
			};

			_dropdowns[baseType] = dropdown;
		}

		// Draw button with current selected type name
		string buttonText = string.IsNullOrEmpty(currentTypeName)
			? "(None)"
			: ObjectNames.NicifyVariableName(currentTypeName.Split('.').Last());

		float widthSplit = position.width / 4;

		EditorGUI.LabelField(new Rect(position.position.x, position.y, widthSplit, position.height), label.text);

		Rect dropDownRect = new (position.position.x + widthSplit, position.y, widthSplit * 3, position.height);
		if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(buttonText), FocusType.Keyboard))
		{
			dropdown.Show(dropDownRect);
		}
	}

	public class TypeDropdown : AdvancedDropdown
	{
		private readonly List<Type> _types;
		public event Action<Type> OnTypeSelected;

		public TypeDropdown(List<Type> types, AdvancedDropdownState state) : base(state)
		{
			_types = types;
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var root = new AdvancedDropdownItem("Select Type");

			foreach (var type in _types.OrderBy(t => t.FullName))
			{
				root.AddChild(new TypeDropdownItem(type));
			}

			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (item is TypeDropdownItem typeItem)
			{
				OnTypeSelected?.Invoke(typeItem.Type);
			}
		}
	}

	public class TypeDropdownItem : AdvancedDropdownItem
	{
		public Type Type { get; }

		public TypeDropdownItem(Type type) : base(ObjectNames.NicifyVariableName(type.FullName))
		{
			Type = type;
		}
	}
}