using System.Collections.Generic;
using CardGame.Card;
using UnityEngine;

namespace CardGame.UI
{
	public class TileContainer : MonoBehaviour
	{
		[SerializeField, Min(0)] private int _maxTiles;

		[SerializeField]
		private GameObject _tileUIPrefab;

		public int MaxTiles => _maxTiles;

		private List<TileVisu> _tiles = new();
		[SerializeField] private List<TileSettings> _tempTileSettings;
		public RectTransform RectTransform { get; set; }

		private void Awake()
		{
			//RectTransform = GetComponent<RectTransform>();

			//foreach (TileSettings tileSettings in _tempTileSettings)
			//{
			//	_tiles.Add(CreateTileUI(tileSettings));
			//}
		}

		public TileVisu CreateTileUI(TileSettings settings)
		{
			//if (_tileUIPrefab == null)
			//{
			//	Debug.LogWarning($"[{nameof(TileContainer)}] cardUIPrefab is null, card will not be created");
			//	return null;
			//}

			//TileVisu tileUI = Instantiate(_tileUIPrefab, transform).GetComponent<TileVisu>();
			//tileUI.InitTile(settings);

			//return tileUI;

			return null;
		}

		public bool ContainsTile(Vector3 position, out TileVisu tile)
		{
			tile = null;

			//foreach (TileVisu currentTileUI in _tiles)
			//{
			//	if (currentTileUI == null)
			//		continue;

			//	if (!RectTransformUtility.RectangleContainsScreenPoint(currentTileUI.TileRectTransform, position))
			//		continue;

			//	tile = currentTileUI;
			//	return true;
			//}
			return false;
		}

		public int RemoveTile(TileVisu tile)
		{
			//int index = _tiles.IndexOf(tile);
			//_tiles.Remove(tile);
			//return index;

			return -1;
		}

		public void AddTile(TileVisu tile, int index)
		{
			//_tiles.Insert(index, tile);
		}

		public bool GetMouseBetweenIndexes(Vector2 mousePosition, Canvas mainCanvas, out int listIndex)
		{
			listIndex = 0;

			//if (_tiles.Count >= _maxTiles)
			//	return false;

			//if (_tiles.Count <= 0)
			//	return true;

			//RectTransformUtility.ScreenPointToLocalPointInRectangle(
			//	mainCanvas.transform as RectTransform, mousePosition, mainCanvas.worldCamera, out Vector2 localPoint);

			//List<float> positionsX = new();
			//foreach (TileVisu tileUI in _tiles)
			//{
			//	positionsX.Add(tileUI.TileRectTransform.localPosition.x);
			//}

			//// Left of all tiles
			//if (localPoint.x < positionsX[0])
			//{
			//	listIndex = 0;
			//	return true;
			//}

			//// Right of all tiles
			//if (localPoint.x > positionsX[^1])
			//{
			//	listIndex = positionsX.Count;
			//	return true;
			//}

			//// Between two tiles
			//for (int i = 0; i < positionsX.Count - 1; i++)
			//{
			//	if (!(localPoint.x >= positionsX[i]) || !(localPoint.x <= positionsX[i + 1]))
			//		continue;

			//	listIndex = i + 1;
			//	return true;
			//}

			return false;
		}
	}
}