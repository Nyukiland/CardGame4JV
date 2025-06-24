using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.Rendering;
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

            Camera.main.transform.position = new Vector3(_width / 2, _height / 2, Camera.main.transform.position.z);
        }

        public TileVisu GetTile(int x, int y)
        {
            if (x > _width - 1 || x < 0 || y > _height - 1 || y < 0) return null;
            return _grid[x, y];
        }

        public TileVisu GetTile(Vector2Int arrayCoordinates)
        {
            if (arrayCoordinates.x > _width - 1 || arrayCoordinates.x < 0 || arrayCoordinates.y > _height - 1 || arrayCoordinates.y < 0) return null;
            return _grid[arrayCoordinates.x, arrayCoordinates.y];
        }

        public bool SetTile(TileData tile, int x, int y)
        {
            if (x > _width - 1 || x < 0 || y > _height - 1 || y < 0) return false;

            TileVisu tileVisu = _grid[x, y];
            //if (tileVisu != null) return false; TODO > Ca BUG

            // On va passer les données de la tuile, désactiver le collider et rendre le GameObject visible
            tileVisu.UpdateTile(tile);
            tileVisu.gameObject.SetActive(true);
            ActivateSurroundingTiles(x, y);
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

            if (x + 1 <= _width - 1) ActivateTile(x+1, y);
            if (x - 1 >= 0) ActivateTile(x - 1, y);
			if (y + 1 <= _height - 1 ) ActivateTile(x, y +1);
			if (y - 1 >= 0) ActivateTile(x, y - 1);

			void ActivateTile(int x, int y)
			{
				_grid[x, y].gameObject.SetActive(true);

				if (!SurroundingTilePos.Contains(new(x, y)))
					SurroundingTilePos.Add(new(x, y));
			}
        }

        public int GetPlacementConnectionCount(TileData tileData, Vector2Int pos)
        {
            int connections = 0;

            ZoneData[] myZones = tileData.Zones;

            // Nord vs Sud
            TileVisu northNeighbor = GetTile(new Vector2Int(pos.x, pos.y + 1));
            if (northNeighbor != null)
            {
                TileData northData = northNeighbor.TileData;
                if (northData != null)
                {
                    if (myZones[0].environment != northData.Zones[2].environment)
                        return 0;

                    connections++;
                }
            }

            // Est vs Ouest
            TileVisu eastNeighbor = GetTile(new Vector2Int(pos.x + 1, pos.y));
            if (eastNeighbor != null)
            {
                TileData eastData = eastNeighbor.TileData;
                if (eastData != null)
                {
                    if (myZones[1].environment != eastData.Zones[3].environment)
                        return 0;

                    connections++;
                }
            }

            // Sud vs Nord
            TileVisu southNeighbor = GetTile(new Vector2Int(pos.x, pos.y - 1));
            if (southNeighbor != null)
            {
                TileData southData = southNeighbor.TileData;
                if (southData != null)
                {
                    if (myZones[2].environment != southData.Zones[0].environment)
                        return 0;

                    connections++;
                }
            }

            // Ouest vs Est
            TileVisu westNeighbor = GetTile(new Vector2Int(pos.x - 1, pos.y));
            if (westNeighbor != null)
            {
                TileData westData = westNeighbor.TileData;
                if (westData != null)
                {
                    if (myZones[3].environment != westData.Zones[1].environment)
                        return 0;

                    connections++;
                }
            }

            return connections;
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