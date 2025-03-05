using UnityEngine;

public class ChangeColorAttribute : PropertyAttribute
{
	public bool PropertyResetColor { get; private set; }
	public Color PropertyColor { get; private set; }

	public ChangeColorAttribute(float r, float g, float b, float a = 1, bool resetAfter = true)
	{
		PropertyColor = new Color(r, g, b, a);
		PropertyResetColor = resetAfter;
	}
}
