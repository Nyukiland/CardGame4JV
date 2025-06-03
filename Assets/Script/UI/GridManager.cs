using UnityEngine;

namespace CardGame.UI
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private RectTransform _gridContainer;
        [Header("Tiles")]
        [SerializeField] private TileUI _tilePrefab;
        [SerializeField] private Vector2 _tileSize;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private Color _baseColor;
        [SerializeField] private Color _offsetColor;

        private TileUI[,] _grid;

        private void Start()
        {
            _grid = new TileUI[_width, _height];
            
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            float totalWidth = _width * _tileSize.x + (_width - 1) * _spacing.x;
            float totalHeight = _height * _tileSize.y + (_height - 1) * _spacing.y;
            _gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    TileUI instantiatedTile = Instantiate(_tilePrefab, _gridContainer);
                    _grid[x,y] = instantiatedTile;
                    instantiatedTile.name = $"Tile_{x}_{y}";

                    float offsetX = -totalWidth / 2f + _tileSize.x / 2f;
                    float offsetY = totalHeight / 2f - _tileSize.y / 2f;
                    float posX = offsetX + x * (_tileSize.x + _spacing.x);
                    float posY = offsetY - y * (_tileSize.y + _spacing.y);
                    instantiatedTile.TileRectTransform.anchoredPosition = new Vector2(posX, posY);
                    instantiatedTile.TileRectTransform.sizeDelta = _tileSize;

                    bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    instantiatedTile.Image.color = isOffset ? _offsetColor : _baseColor;
                }
            }
        }

        public TileUI GetTile(int x, int y)
        {
            // À terme on renverra plutôt la data que l'UI
            return _grid[x, y];
        }

        public void SetTile(TileUI tileUI, int x, int y)
        {
            // À terme on attribuera plutôt la data que l'UI
            _grid[x, y] = tileUI;
        }
    }
}