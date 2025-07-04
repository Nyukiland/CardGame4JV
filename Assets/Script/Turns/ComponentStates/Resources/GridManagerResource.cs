using CardGame.Card;
using CardGame.Net;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using DG.Tweening;
using System.Collections.Generic;
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

		// tile bonus
		private List<Vector2Int> BonusTilePositions = new();
		private List<TileData> BonusTilePool = new();

		public override void LateInit()
		{
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();
			drawPile.OnTilesLoaded += GenerateGrid;
		}

		public override void OnEnable()
		{
			Storage.Instance.Register(this);
		}

		private void GenerateGrid()
		{
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>(); // On se desabonne
			drawPile.OnTilesLoaded -= GenerateGrid;

			_grid = new TileVisu[_width, _height];

			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					GameObject instantiatedTile = GameObject.Instantiate(_tilePrefab, _gridContainer.transform);
					instantiatedTile.GetComponent<BoxCollider>().enabled = false;
					_grid[x, y] = instantiatedTile.GetComponent<TileVisu>();

					instantiatedTile.transform.position = new Vector2(x, y);
					instantiatedTile.SetActive(false);
				}
			}

			// Set StartingTile in the midle of the grid :
			TileData tileData = new();
			tileData.InitTile(_startingTileSettings);
			SetTile(tileData, _width / 2, _height / 2);
			//tileData.DetermineTileRegions(_width / 2, _height / 2);

			Camera.main.transform.position = new Vector3(_width / 2, (_height / 2) - 5, Camera.main.transform.position.z);
		}

		public void GenerateBonusTiles()
		{
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			for (int range = 2; range < 4; range++) // On call a range = 2 et = 3 pour les deux carrés
			{
				BonusTilePool.Clear(); // On recupere la liste des tiles bonus prévues pour ce carré
				BonusTilePool = drawPile.GetBonusTileFromPoolIndex(range - 1); // Cringe, car en index on a 1 et 2 pour premier et 2e carré

				BonusTilePositions.Clear();
				GetBonusTilePositionList(range); // On recup toutes les positions possibles pour ce carré

				for (int i = 0; i < 3; i++) // On pose 3 bonus tiles
					PlaceBonusTile();
			}
		}

		private void GetBonusTilePositionList(int range)
		{
			Vector2Int center = new Vector2Int(_width / 2, _height / 2); // Centre du carré 

			for (int x = -range; x <= range; x++) // Calcul sur un centre 0,0
			{
				for (int y = -range; y <= range; y++)
				{
					if (Mathf.Abs(x) == range || Mathf.Abs(y) == range) // Un carré 5x5 par ex, la bordure c'est toutes les valeurs ou X vaut 2 ou -2, avec y entre -2 et 2, ou l'inverse
					{
						BonusTilePositions.Add(center + new Vector2Int(x, y)); // On ajoute la nouvelle position, mais ramenée au centre du carré
					}
				}
			}
		}

		private void PlaceBonusTile()
		{
			Vector2Int value = BonusTilePositions[UnityEngine.Random.Range(0, BonusTilePositions.Count)];
			TileVisu tempTile = GetTile(value.x, value.y);
			BonusTilePositions.Remove(value); // On retire la valeur, si ca reussi on retire, si ca fail on retire donc duh

			if (tempTile != null && tempTile.TileData == null)
			{
				for (int x = -2; x < 3; x++)
				{
					for (int y = -2 + Mathf.Abs(x); y < 3 - Mathf.Abs(x); y++)
					{
						TileVisu tile = GetTile(value.x + x, value.y + y);

						if (tile != null && tile.TileData != null) // && !(x == 0 && y ==0) si on veut autoriser a être proche du centre
						{
							//Si ca fail
							PlaceBonusTile(); //on retry recursivement
							return;
						}
					}
				}

				//Si ca fonctionne
				TileData tileData = BonusTilePool[UnityEngine.Random.Range(0, BonusTilePool.Count)];
				BonusTilePool.Remove(tileData); // On retire le tiledata de la liste temp, pour pas le placer en double

				SetTile(tileData, value.x, value.y);
			}
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
			tileVisu.IsLinked = (tile.TileSettings.PoolIndex == 0) ? true : false; // Si tile normale true, si tile bonus false
			ActivateSurroundingTiles(x, y);
			SetNeighborBonusTileLinked(new(x, y));
			PlayTileEffect(tileVisu);
			DetermineTileRegions(x, y);
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

			if (!GetTile(x, y).IsLinked) return; // Le cas des tiles bonus lors de la generation de la grille

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

			Sequence seq = DOTween.Sequence();

			seq.Append(visu.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
			seq.Join(visu.transform.DOShakeRotation(0.5f, 50, 10, 80, true));

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
						if (myZones[myZone].environment != ENVIRONEMENT_TYPE.None &&
							data.Zones[otherZone].environment != ENVIRONEMENT_TYPE.None && 
							myZones[myZone].environment != data.Zones[otherZone].environment)
							return 0;


						return 1;
					}
				}

				return null;
			}
		}

		public int CheckNeighborTileLinked(Vector2Int pos) // Ca sert surtout dans le cas des tiles bonus, on l'utilise surtout pour check si > 0;
		{
			TileVisu tile = null;
			int total = 0;

			tile = GetTile(pos.x - 1, pos.y); // a gauche
			if (tile != null && tile.TileData != null && tile.IsLinked)
				total++;

			tile = GetTile(pos.x + 1, pos.y); // a droite
			if (tile != null && tile.TileData != null && tile.IsLinked)
				total++;

			tile = GetTile(pos.x, pos.y - 1); // au dessous
			if (tile != null && tile.TileData != null && tile.IsLinked)
				total++;

			tile = GetTile(pos.x, pos.y + 1); // au dessus
			if (tile != null && tile.TileData != null && tile.IsLinked)
				total++;

			return total;
		}

		public void SetNeighborBonusTileLinked(Vector2Int pos) 
		{
			TileVisu tile = null;
			TileVisu currentTile = GetTile(pos.x, pos.y);

			tile = GetTile(pos.x - 1, pos.y); // a gauche
			if (tile != null && !tile.IsLinked) // On check si c'est une tile bonus jamais connectée ni owned, du coup.
			{
				tile.IsLinked = true;
				tile.TileData.OwnerPlayerIndex = currentTile.TileData.OwnerPlayerIndex;
				tile.UpdateTile(tile.TileData);
				ActivateSurroundingTiles(pos.x - 1, pos.y);
			}

			tile = GetTile(pos.x + 1, pos.y); // a droite
			if (tile != null && !tile.IsLinked)
			{
				tile.IsLinked = true;
				tile.TileData.OwnerPlayerIndex = currentTile.TileData.OwnerPlayerIndex;
				tile.UpdateTile(tile.TileData);
				ActivateSurroundingTiles(pos.x + 1, pos.y);
			}

			tile = GetTile(pos.x, pos.y - 1); // au dessous
			if (tile != null && !tile.IsLinked)
			{
				tile.IsLinked = true;
				tile.TileData.OwnerPlayerIndex = currentTile.TileData.OwnerPlayerIndex;
				tile.UpdateTile(tile.TileData);
				ActivateSurroundingTiles(pos.x, pos.y - 1);
			}

			tile = GetTile(pos.x, pos.y + 1); // au dessus
			if (tile != null && !tile.IsLinked)
			{
				tile.IsLinked = true;
				tile.TileData.OwnerPlayerIndex = currentTile.TileData.OwnerPlayerIndex;
				tile.UpdateTile(tile.TileData);
				ActivateSurroundingTiles(pos.x, pos.y + 1);
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

		//For the net (not recommended outside of specific net use)
		public DataToSendList GetListOfPlacedTile()
		{
			DataToSendList list = new();

			foreach (TileVisu tile in _grid)
			{
				if (tile.TileData == null) continue;

				Vector2Int pos = new((int)tile.transform.position.x, (int)tile.transform.position.y);
				DataToSend data = new (tile.TileData, pos);
				list.DataList.Add(data);
			}

			return list;
		}

		private TileData DetermineNeighborTile(int DirectionIndex, int x, int y)
		{
			return DirectionIndex switch
			{
				0 => GetTile(x, y + 1).TileData,
				1 => GetTile(x + 1, y).TileData,
				2 => GetTile(x, y - 1).TileData,
				3 => GetTile(x - 1, y).TileData,
				_ => null,
			};
		}

		private int DetermineCorrespondingZoneIndex(int ZoneIndex)
		{
			return ZoneIndex switch
			{
				0 => 2,
				1 => 3,
				2 => 0,
				3 => 1,
				_ => -1
			};
		}

		public void DetermineTileRegions(int x, int y)
		{
			//Debug.Log("----------------------------- TILE : " + x + " - " + y + "-----------------------------");
			TileVisu tileObject = GetTile(x, y);
			TileData tile = tileObject.TileData;

			// Etape 1 : Check pour chaque zone, s'il y a une tuile à côté.
			for (int i = 0; i <= 3; i++)
			{
				//Debug.Log("Direction : " + i);
				TileData neighborTile = DetermineNeighborTile(i, x, y);

				// si une tuile jouxte la zone :
				if (neighborTile != null)
				{
					//Debug.Log("Il y a une tuile à côté");
					int correspondingZoneIndex = DetermineCorrespondingZoneIndex(i);

					// Si oui, copie cette région comme étant celle de la zone
					tile.Zones[i].Region = neighborTile.Zones[correspondingZoneIndex].Region;
					tile.Zones[i].Region.Tiles.Add(tileObject);
					// baisse le compteur de la zone en question de 1
					tile.Zones[i].Region.OpeningCount--;
					if (tile.Zones[i].Region.OpeningCount == 0)
					{
						//Debug.Log("La région dans la direction " + i + " est fermée");
						//Debug.Log("Il y a " + tile.Zones[i].Region.Tiles.Count + " dans la zone");
					}
				}
				else
				{
					//Debug.Log("Il n'y a pas une tuile à côté > on créé une région");
					// Sinon Crée une nouvelle zone. 
					tile.Zones[i].Region = new Region(1);
				}
				tile.Zones[i].Region.Tiles.Add(tileObject);
			}

			// Etape 2 : check pour chaque zone si elle est connectée à une autre zone de la tuile
			for (int i = 0; i <= 3; i++)
			{
				//Debug.Log("Direction I : " + i);
				if (tile.Zones[i].isOpen == false) continue;
				for (int j = i + 1; j <= 3; j++)
				{
					//Debug.Log("Direction I : " + i + " + J : " + j);

					if (tile.Zones[i].environment != tile.Zones[j].environment) continue;
					if (tile.Zones[i].Region == tile.Zones[j].Region) continue;
					// On merge :
					tile.Zones[i].Region.Merge(tile.Zones[j].Region);
				}
			}
		}
	}
}