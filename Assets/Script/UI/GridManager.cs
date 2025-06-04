using CardGame.Utility;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

namespace CardGame.UI
{
    public class GridManager : MonoBehaviour
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

        private GameObject[,] _grid;

        private void Start()
        {
            _grid = new GameObject[_width, _height];
            
            GenerateGrid();
        }
        private void OnEnable()
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
                    GameObject instantiatedTile = Instantiate(_tilePrefab, _gridContainer.transform);
                    _grid[x, y] = instantiatedTile;
                    instantiatedTile.name = $"Tile_{x}_{y}";

                    float offsetX = -totalWidth / 2f + _tileSize.x / 2f;
                    float offsetY = totalHeight / 2f - _tileSize.y / 2f;
                    float posX = offsetX + x * (_tileSize.x + _spacing.x);
                    float posY = offsetY - y * (_tileSize.y + _spacing.y);
                    instantiatedTile.transform.position = new Vector2(posX, posY);
                    instantiatedTile.SetActive(false);
                }
            }
        }

        public GameObject GetTile(int x, int y)
        {
            return _grid[x, y];
        }

        public GameObject GetTile(Vector2Int arrayCoordinates)
        {
            return _grid[arrayCoordinates.x, arrayCoordinates.y];
        }

        public void SetTile(GameObject tile, int x, int y)
        {
            // On va passer le 
            _grid[x, y] = tile;
        }

        public void SetTile(GameObject tile, Vector2Int arrayCoordinates)
        {
            // On va passer le 
            _grid[arrayCoordinates.x, arrayCoordinates.y] = tile;
        }


        private void OnDisable()
        {
            Storage.Instance.Delete(this);
        }
    }

    
}