using CardGame.StateMachine;
using CardGame.Card;
using CardGame.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGame.Turns
{
	public class PlaceTileOnGridAbility : Ability
	{
		[SerializeField]
		private DrawPile _drawPile;

		[SerializeField, Min(0)]
		private float _maxTimeTurn = 30;
		public float MaxTimeTurn => _maxTimeTurn;

		private Plane _planeForCast = new(Vector3.forward, new Vector3(0, 0, -0.15f));

		private MoveTileAbility _moveTile;
		private ZoneHolderResource _zoneHolder;
		private GridManagerResource _gridManager;
		private CreateHandAbility _createHandAbility;
		private SendInfoAbility _sender;
		private ScoringAbility _scoring;
		private NetworkResource _network;
		private SoundResource _sound;

		public float Timer { get; private set; } = 0;

		public event System.Action OnCardReleased; //Pour la preview d'ou on peut poser la tileObject de maniere valide

		public TileVisu TempPlacedTile { get; set; } = null;
		private Vector2Int _tempPos;

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
		}

		public override void OnEnable()
		{
			base.OnEnable();
			TilePlaced = false;
			Timer = 0;

			TempPlacedTile = null;
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
				_zoneHolder.GiveTileToHand(tempTile.gameObject);
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay(position);
			_planeForCast.Raycast(ray, out float dist);
			Vector2Int pos = Vector2Int.FloorToInt(ray.GetPoint(dist));

			TileVisu targetTile = _gridManager.GetTile(pos);

			if (targetTile != null && targetTile.TileData == null)
			{
				int neighborCount = _gridManager.CheckNeighborTileLinked(pos);
				int connectionCount = _gridManager.GetPlacementConnectionCount(tempTile.TileData, pos);

				if (connectionCount == 0 || neighborCount == 0) // Si pas de connection valide, ou que si mais pas de voisin valide (cas d'une tileObject bonus isol�e)
				{
					_zoneHolder.GiveTileToHand(tempTile.gameObject);
					return;
				}

				TempPlacedTile = tempTile;
				_tempPos = pos;
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

			int connectionCount = _gridManager.GetPlacementConnectionCount(TempPlacedTile.TileData, _tempPos);
			
			if (connectionCount == 0) return;

			TempPlacedTile.TileData.OwnerPlayerIndex = GameManager.Instance.PlayerIndex; // On donne l'index du joueur a la tileObject
			TempPlacedTile.TileData.HasFlag = GameManager.Instance.FlagTurn; // Check si flag turn

			_sound.PlayTilePlaced();
			_gridManager.SetTile(TempPlacedTile.TileData, _tempPos);
			_sender.SendInfoTilePlaced(TempPlacedTile.TileData, _tempPos);
			_scoring.SetScoringPos(_tempPos);

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

			if (Timer > _maxTimeTurn)
			{
				Timer = -1f;

				if (TempPlacedTile == null) AutoPlace();
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
			_tempPos = tilePlaced;

			_zoneHolder.RemoveTileFromHand(TempPlacedTile.gameObject);
		}
	}
}