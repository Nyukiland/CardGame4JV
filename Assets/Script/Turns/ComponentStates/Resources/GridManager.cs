using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using UnityEngine;

namespace CardGame.Turns
{
    public class GridManager : Resource
    {
        [Header("Grid")]
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private GameObject _gridContainer;
        [Header("Tiles")]
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private Vector2 _tileSize;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private Color _baseColor;
        [SerializeField] private Color _offsetColor;

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
            float totalWidth = _width * _tileSize.x + (_width - 1) * _spacing.x;
            float totalHeight = _height * _tileSize.y + (_height - 1) * _spacing.y;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    GameObject instantiatedTile = GameObject.Instantiate(_tilePrefab, _gridContainer.transform);
                    _grid[x, y] = instantiatedTile.GetComponent<TileVisu>();
                    instantiatedTile.name = $"Tile_{x}_{y}";

                    float offsetX = -totalWidth / 2f + _tileSize.x / 2f;
                    float offsetY = totalHeight / 2f - _tileSize.y / 2f;
                    float posX = offsetX + x * (_tileSize.x + _spacing.x);
                    float posY = offsetY - y * (_tileSize.y + _spacing.y);
                    instantiatedTile.transform.position = new Vector2(posX, posY);
                    instantiatedTile.SetActive(true);
                }
            }
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
            tileVisu.gameObject.GetComponent<BoxCollider>().enabled = false;
            tileVisu.gameObject.SetActive(true);
            return true;
        }


        public override void OnDisable()
        {
            if (Storage.CheckInstance()) Storage.Instance.Delete(this);
        }
    }


}