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
                    instantiatedTile.SetActive(true);
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
            return SetTile(tileVisu, tile);
        }

        public bool SetTile(TileData tile, Vector2Int arrayCoordinates)
        {
            if (arrayCoordinates.x > _width - 1 || arrayCoordinates.x < 0 || arrayCoordinates.y > _height - 1 || arrayCoordinates.y < 0) return false;
            TileVisu tileVisu = _grid[arrayCoordinates.x, arrayCoordinates.y];
            return SetTile(tileVisu, tile);

        }

        private bool SetTile(TileVisu tileVisu, TileData tile)
        {
            // On va passer les données de la tuile, désactiver le collider et rendre le GameObject visible
            tileVisu.UpdateTile(tile);
            tileVisu.gameObject.SetActive(true);
            return true;
        }


        public override void OnDisable()
        {
            if (Storage.CheckInstance()) Storage.Instance.Delete(this);
        }
    }


}