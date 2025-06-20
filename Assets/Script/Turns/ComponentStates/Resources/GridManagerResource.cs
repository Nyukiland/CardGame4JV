using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
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

        public override void Init(Controller owner)
        {
            _grid = new TileVisu[_width, _height];

            GenerateGrid();
        }

        public override void OnEnable()
        {
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
            if (x + 1 <= _width - 1) _grid[x + 1, y].gameObject.SetActive(true);
            if (x - 1 >= 0) _grid[x - 1, y].gameObject.SetActive(true);
            if (y + 1 <= _height - 1 ) _grid[x, y + 1].gameObject.SetActive(true);
            if (y - 1 >= 0) _grid[x, y - 1].gameObject.SetActive(true);
        }

		public int GetPlacementConnectionCount(TileData tileData, Vector2Int pos)
		{
			int connections = 0;

			ZoneData[] myZones = tileData.Zones;

			(GridManagerResource grid, TileVisu neighbor, TileData neighborData) = (this, null, null);

			// Nord vs sud
			neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y + 1));
			if (neighbor != null && (neighborData = neighbor.TileData) != null)
			{
				if (myZones[0].environment != neighborData.Zones[2].environment)
					return 0;
				connections++;
			}

			// Est vs Ouest
			neighbor = grid.GetTile(new Vector2Int(pos.x + 1, pos.y));
			if (neighbor != null && (neighborData = neighbor.TileData) != null)
			{
				if (myZones[1].environment != neighborData.Zones[3].environment)
					return 0;
				connections++;
			}

			// Sud vs Nord
			neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y - 1));
			if (neighbor != null && (neighborData = neighbor.TileData) != null)
			{
				if (myZones[2].environment != neighborData.Zones[0].environment)
					return 0;
				connections++;
			}

			// Ouest vs est
			neighbor = grid.GetTile(new Vector2Int(pos.x - 1, pos.y));
			if (neighbor != null && (neighborData = neighbor.TileData) != null)
			{
				if (myZones[3].environment != neighborData.Zones[1].environment)
					return 0;
				connections++;
			}

			//Debug.Log($"Placement valide avec {connections}");
			return connections;
		}

		public override void OnDisable()
        {
            if (Storage.CheckInstance()) Storage.Instance.Delete(this);
        }
    }
}