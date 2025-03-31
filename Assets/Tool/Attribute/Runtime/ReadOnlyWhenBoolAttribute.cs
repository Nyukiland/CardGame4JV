using UnityEngine;

public class ReadOnlyWhenBoolAttribute : PropertyAttribute
{
	public string PropertyString { get; private set; }
	public bool PropertyBool { get; private set; }

	public ReadOnlyWhenBoolAttribute(string varName, bool readonlyWhen = true)
	{
		PropertyString = varName;
		PropertyBool = readonlyWhen;
	}
}