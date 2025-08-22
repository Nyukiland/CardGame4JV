using UnityEngine;

public class TileVisualSorter : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer _visuNorth;
	[SerializeField]
	private MeshRenderer _visuSouth;
	[SerializeField]
	private MeshRenderer _visuEast;
	[SerializeField]
	private MeshRenderer _visuWest;

	public MeshRenderer VisuNorth => _visuNorth;
	public MeshRenderer VisuSouth => _visuSouth;
	public MeshRenderer VisuEast => _visuEast;
	public MeshRenderer VisuWest => _visuWest;
}