using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SubclassSelectorAttribute : PropertyAttribute
{
	public Type BaseType { get; }

	public SubclassSelectorAttribute(Type baseType)
	{
		BaseType = baseType;
	}
}