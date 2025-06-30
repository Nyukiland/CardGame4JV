using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.Card;
using CardGame.UI;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class GridManagerResource : Resource
	{
		[Header("Grid")]
		[SerializeField] private int _width;
		[SerializeField] private int _height;
		[SerializeField] private GameObject _gridContainer;
		[Header("Tiles")]
		[SerializeField] private GameObject _tilePrefab;
		[SerializeField] private TileSettings _startingTileSettings;

		public int Width => _width;
		public int Height => _height;

		private TileVisu[,] _grid;

		public List<Vector2Int> SurroundingTilePos { get; private set; } = new();

		public override void Init(Controller owner)
		{
			_grid = new TileVisu[_width, _height];

			GenerateGrid();

			Storage.Instance.Register(this);
		}

		private void GenerateGrid()
		{
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					GameObject instantiatedTile = GameObject.Instantiate(_tilePrefab, _gridContainer.transform);
					instantiatedTile.GetComponent<BoxCollider>().enabled = false;
					_grid[x, y] = instantiatedTile.GetComponent<TileVisu>();
					instantiatedTile.name = $"Tile_{x}_{y}";

					instantiatedTile.transform.position = new Vector2(x, y);
					instantiatedTile.SetActive(false);
				}
			}

			// Set StartingTile in the midle of the grid :
			TileData tileData = new();
			tileData.InitTile(_startingTileSettings);
			SetTile(tileData, _width / 2, _height / 2);

			Camera.main.transform.position = new Vector3(_width / 2, (_height / 2) - 5.5f, Camera.main.transform.position.z);
		}

		public TileVisu GetTile(int x, int y)
		{
			if (x > _width - 1 || x < 0 || y > _height - 1 || y < 0) return null;
			return _grid[x, y];
		}

		public TileVisu GetTile(Vector2Int arrayCoordinates)
		{
			return GetTile(arrayCoordinates.x, arrayCoordinates.y);
		}

		public bool SetTile(TileData tile, int x, int y)
		{
			if (x > _width - 1 || x < 0 || y > _height - 1 || y < 0) return false;

			TileVisu tileVisu = _grid[x, y];
			//if (tileVisu != null) return false; TODO > Ca BUG

			// On va passer les données de la tuile, désactiver le collider et rendre le GameObject visible
			tileVisu.UpdateTile(tile);
			tileVisu.GetComponent<BoxCollider>().enabled = true;
			tileVisu.SetTilePosOnGrid(new(x, y));
			tileVisu.SetTileLayerGrid(true);
			tileVisu.gameObject.SetActive(true);
			ActivateSurroundingTiles(x, y);
			PlayTileEffect(tileVisu);
			return true;
		}

		public bool SetTile(TileData tile, Vector2Int arrayCoordinates)
		{
			return SetTile(tile, arrayCoordinates.x, arrayCoordinates.y);
		}

		private void ActivateSurroundingTiles(int x, int y)
		{
			if (SurroundingTilePos.Contains(new(x, y)))
				SurroundingTilePos.Remove(new(x, y));

			if (x + 1 <= _width - 1) ActivateTile(x + 1, y);
			if (x - 1 >= 0) ActivateTile(x - 1, y);
			if (y + 1 <= _height - 1) ActivateTile(x, y + 1);
			if (y - 1 >= 0) ActivateTile(x, y - 1);

			void ActivateTile(int x, int y)
			{
				_grid[x, y].gameObject.SetActive(true);

				if (!SurroundingTilePos.Contains(new(x, y)) && GetTile(x, y).TileData == null)
				{
					SurroundingTilePos.Add(new(x, y));
					PlaySurroundingTileEffect(_grid[x, y]);
				}
			}
		}

		public void PlayTileEffect(TileVisu visu)
		{
			visu.transform.localScale = Vector3.zero;
			visu.transform.rotation = Quaternion.identity;

			Sequence seq = DOTween.Sequence();

			seq.Append(visu.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
			seq.Join(visu.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));

			seq.Play();
		}

		public void PlaySurroundingTileEffect(TileVisu visu)
		{
			visu.transform.localScale = Vector3.zero;

			Sequence seq = DOTween.Sequence();

			seq.Append(visu.transform.DORotate(new Vector3(0, 360, 0), 0.3f, RotateMode.FastBeyond360).SetEase(Ease.OutBack));
			seq.Join(visu.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

			seq.Play();
		}

		public int GetPlacementConnectionCount(TileData tileData, Vector2Int pos)
		{
			int connections = 0;

			ZoneData[] myZones = tileData.Zones;

			// Nord vs Sud
			int? result = CheckNeighborValidity(pos.x, pos.y + 1, 0, 2);
			if (result == 0) return 0;
			else if (result == 1) connections++;

			// Est vs Ouest
			result = CheckNeighborValidity(pos.x + 1, pos.y, 1, 3);
			if (result == 0) return 0;
			else if (result == 1) connections++;

			// Sud vs Nord
			result = CheckNeighborValidity(pos.x, pos.y - 1, 2, 0);
			if (result == 0) return 0;
			else if (result == 1) connections++;

			// Ouest vs Est
			result = CheckNeighborValidity(pos.x - 1, pos.y, 3, 1);
			if (result == 0) return 0;
			else if (result == 1) connections++;

			return connections;

			int? CheckNeighborValidity(int x, int y, int myZone, int otherZone)
			{
				TileVisu neighbor = GetTile(new Vector2Int(x, y));
				if (neighbor != null)
				{
					TileData data = neighbor.TileData;
					if (data != null)
					{
						if (myZones[myZone].environment != data.Zones[otherZone].environment)
							return 0;

						return 1;
					}
				}

				return null;
			}
		}

		public int GetPlacementConnectionCount(Vector2Int pos)
		{
			return GetPlacementConnectionCount(_grid[pos.x, pos.y].TileData, pos);
		}

		public override void OnDisable()
		{
			if (Storage.CheckInstance()) Storage.Instance.Delete(this);
		}
	}
}