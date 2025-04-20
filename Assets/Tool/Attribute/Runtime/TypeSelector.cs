using System;
using UnityEngine;

public class TypeSelector : PropertyAttribute
{
	public Type Type;

	public TypeSelector(Type type)
	{
		Type = type;
	}
}