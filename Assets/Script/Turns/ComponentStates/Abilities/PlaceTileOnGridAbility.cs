using CardGame.StateMachine;
using CardGame.Card;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class PlaceTileOnGridAbility : Ability
	{
		[SerializeField]
		private DrawPile _drawPile;

		[SerializeField, Min(0)]
		private float _maxTimeTurnFirst = 30;

		[SerializeField, Min(0)]
		private float _maxTimeTurnEnd = 180;

		private Plane _planeForCast = new(Vector3.forward, new Vector3(0, 0, -0.15f));

		private MoveTileAbility _moveTile;
		private ZoneHolderResource _zoneHolder;
		private GridManagerResource _gridManager;
		private CreateHandAbility _createHandAbility;
		private SendInfoAbility _sender;
		private ScoringAbility _scoring;
		private NetworkResource _network;
		private SoundResource _sound;

		private GameManager _gameManager;

		public float Timer { get; private set; } = 0;

		public event System.Action OnCardReleased; //Pour la preview d'ou on peut poser la tileObject de maniere valide

		public TileVisu TempPlacedTile { get; set; } = null;
		public Vector2Int TempPos;

		public float MaxTimeTurn { private set; get; }
		public bool TilePlaced { get; private set; }

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_moveTile = owner.GetStateComponent<MoveTileAbility>();
			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
			_createHandAbility = owner.GetStateComponent<CreateHandAbility>();
			_sender = owner.GetStateComponent<SendInfoAbility>();
			_scoring = owner.GetStateComponent<ScoringAbility>();
			_network = owner.GetStateComponent<NetworkResource>();
			_sound = owner.GetStateComponent<SoundResource>();

			_gameManager = GameManager.Instance;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			TilePlaced = false;
			MaxTimeTurn = Mathf.Lerp(_maxTimeTurnFirst, _maxTimeTurnEnd, (float)_gameManager.LocalPlayerTurn / (float)_gameManager.MaxTurn);
			Timer = 0;

			TempPlacedTile = null;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			ReleaseTile(new(10000, 10000));

			int nextTurn = GameManager.Instance.LocalPlayerTurn + 1;
			bool willBeFlagTurn = (nextTurn % 3 == 0);

			foreach (GameObject tileObj in _zoneHolder.TileInHand)
			{
				if (tileObj == null) continue;

				TileVisu tileVisu = tileObj.GetComponent<TileVisu>();
				if (tileVisu != null)
				{
					tileVisu.ShowFlagPreviewVisual(willBeFlagTurn);
				}
			}
		}



		public void ReleaseTile(Vector2 position)
		{
			if (_moveTile.CurrentTile == null)
				return;

			_moveTile.StandardRelease();
			TileVisu tempTile = _moveTile.CurrentTile;
			_moveTile.CurrentTile = null;
			tempTile.ResetValidityVisual();

			OnCardReleased?.Invoke();

			if (_zoneHolder.IsInHand(position))
			{
				tempTile.SetWrongRotationFeedbackActive(false);
				_zoneHolder.GiveTileToHand(tempTile.gameObject);
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay(position);
			_planeForCast.Raycast(ray, out float dist);
			Vector2Int pos = Vector2Int.RoundToInt(ray.GetPoint(dist));

			TileVisu targetTile = _gridManager.GetTile(pos);

			if (targetTile != null && targetTile.TileData == null)
			{
				int neighborCount = _gridManager.CheckNeighborTileLinked(pos);

				if (neighborCount == 0) // Si pas de connection valide, ou que si mais pas de voisin valide (cas d'une tileObject bonus isol�e)
				{
					_zoneHolder.GiveTileToHand(tempTile.gameObject);
					return;
				}

				TempPlacedTile = tempTile;
				TempPos = pos;

				int connectionCount = _gridManager.GetPlacementConnectionCount(TempPlacedTile.TileData, TempPos);
				TempPlacedTile.SetWrongRotationFeedbackActive(connectionCount == 0);

			}
			else
			{
				_zoneHolder.GiveTileToHand(tempTile.gameObject);
			}
		}

		private void SoloDrawCard()
		{
			int tileId = _drawPile.GetTileIDFromDrawPile();
			if (tileId == -1) return;

			TileSettings tileSettings = null;
			foreach (TileSettings setting in _drawPile.AllTileSettings)
			{
				if (setting.IdCode == tileId)
				{
					tileSettings = setting;
					break;
				}
			}

			_createHandAbility.CreateTile(tileSettings);
		}

		public void CallEndTurn()
		{
			if (TempPlacedTile == null) return;

			int connectionCount = _gridManager.GetPlacementConnectionCount(TempPlacedTile.TileData, TempPos);

			if (connectionCount == 0) return;

			TempPlacedTile.TileData.OwnerPlayerIndex = GameManager.Instance.PlayerIndex; // On donne l'index du joueur a la tileObject
			TempPlacedTile.TileData.HasFlag = GameManager.Instance.FlagTurn; // Check si flag turn

			_sound.PlayTilePlaced();
			_gridManager.SetTile(TempPlacedTile.TileData, TempPos);
			_sender.SendInfoTilePlaced(TempPlacedTile.TileData, TempPos);
			_scoring.SetScoringPos(TempPos);

			if (!_network.IsNetActive())
			{
				for (int i = 0; i < connectionCount; i++)
				{
					SoloDrawCard();
				}
			}

			TilePlaced = true;

			GameObject.Destroy(TempPlacedTile.gameObject);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (Timer == -1f)
				return;

			if (Timer > MaxTimeTurn 
				|| UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame) //Debug feature
			{
				Timer = -1f;

				if (TempPlacedTile == null)
				{
					AutoPlace();
				}
				else if (_gridManager.GetPlacementConnectionCount(TempPlacedTile.TileData, TempPos) == 0)
				{
					// verify there is no correct rotation for the placed tile :
					for(int i = 0; i <= 3; i++)
					{
						TempPlacedTile.TileData.RotateTile();
						// if a correct rotation is found, end turn 
						if(_gridManager.GetPlacementConnectionCount(TempPlacedTile.TileData, TempPos) != 0)
						{
                            CallEndTurn();

                            return;
                        }
						// else, rotate tile again
						continue;
                    }
					// if no correct rotation is found : 
					_zoneHolder.GiveTileToHand(TempPlacedTile.gameObject);
					AutoPlace();
				}

				CallEndTurn();

				return;
			}

			Timer += deltaTime;
		}

		private void AutoPlace()
		{
			//find card placement
			//fun triple loop
			Vector2Int tilePlaced = new(-100, -100);
			TileVisu tileVisu = null;
			foreach (GameObject tileObject in _zoneHolder.TileInHand)
			{
				TileVisu tile = tileObject.GetComponent<TileVisu>();

				foreach (Vector2Int pos in _gridManager.SurroundingTilePos)
				{
					for (int i = 0; i < 4; i++)
					{
						if (_gridManager.GetPlacementConnectionCount(tile.TileData, pos) != 0)
						{
							tileVisu = tile;
							tilePlaced = pos;
							break;
						}
						else
						{
							tile.TileData.RotateTile();
						}
					}

					if (tilePlaced != new Vector2Int(-100, -100)) break;
				}

				if (tilePlaced != new Vector2Int(-100, -100)) break;
			}

			TempPlacedTile = tileVisu;
			TempPos = tilePlaced;

			_zoneHolder.RemoveTileFromHand(TempPlacedTile.gameObject);
		}
	}
}