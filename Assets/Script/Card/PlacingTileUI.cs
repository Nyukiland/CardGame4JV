using CardGame.Card;
using CardGame.UI;
using UnityEngine;

public class PlacingTileUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform _posToFollow;

	[SerializeField]
	private CanvasGroup _canvasController;

	[SerializeField]
	private TileVisualSorter _fourSided;

	[SerializeField]
	private TileVisualSorter _diagonal;

	[SerializeField]
	private TileVisualSorter _diagonalDouble;

	[SerializeField]
	private TileVisualSorter _oneSided;

	[SerializeField]
	private TileVisualSorter _bridge;

	private TileVisualSorter _properTile;
	private Camera _camera;

	private void Start()
	{
		_fourSided.gameObject.SetActive(false);
		_diagonal.gameObject.SetActive(false);
		_diagonalDouble.gameObject.SetActive(false);
		_oneSided.gameObject.SetActive(false);
		_bridge.gameObject.SetActive(false);
		_canvasController.alpha = 0;

		_camera = Camera.main;
	}

	public void SetVisual(TileVisu tile)
	{

		if (_properTile != null)
			_properTile.gameObject.SetActive(false);

		_canvasController.alpha = 0;

		if (tile == null)
		{
			_properTile = null;
			return;
		}

		_canvasController.alpha = 1;
		TileSettings tileSettings = tile.TileData.TileSettings;

		switch (tileSettings.tilePreset)
		{
			case (TilePreset.FourDifferentClosed):
				_properTile = _fourSided;
				break;
			case (TilePreset.ThreeSame):
				_properTile = _oneSided;
				break;
			case (TilePreset.DiagonalOpenHalf):
				_properTile = _diagonal;
				break;
			case (TilePreset.DiagonalOpenFull):
				_properTile = _diagonalDouble;
				break;
			case (TilePreset.Path):
				_properTile = _bridge;
				break;
		}

		_properTile.gameObject.SetActive(true);

		_properTile.VisuNorth.material = tile.GetMaterialForType(tileSettings.NorthZone.environment);
		_properTile.VisuSouth.material = tile.GetMaterialForType(tileSettings.SouthZone.environment);
		_properTile.VisuEast.material = tile.GetMaterialForType(tileSettings.EastZone.environment);
		_properTile.VisuWest.material = tile.GetMaterialForType(tileSettings.WestZone.environment);

		_properTile.transform.eulerAngles = new Vector3(0, 0, 90 * tile.TileData.TileRotationCount);
	}

	private void Update()
	{
		if (_properTile == null)
			return;

		_properTile.gameObject.SetActive(true);

		Vector3[] worldCorners = new Vector3[4];
		_posToFollow.GetWorldCorners(worldCorners);
		Vector3 center = Vector3.zero;

		foreach (Vector3 pos in worldCorners)
			center += pos;

		center /= 4;

		center += _camera.transform.forward;
		_properTile.transform.position = center;
	}
}