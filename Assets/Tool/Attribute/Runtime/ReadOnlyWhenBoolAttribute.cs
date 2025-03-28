using UnityEditor;
using UnityEngine;

public class ReadOnlyWhenBoolAttribute : PropertyAttribute
{
	public string PropertyString { get; private set; }
	public bool PropertyBool { get; private set; }

	public ReadOnlyWhenBoolAttribute(string varName, bool whenTrue = true)
	{
		PropertyString = varName;
		PropertyBool = whenTrue;
	}
}