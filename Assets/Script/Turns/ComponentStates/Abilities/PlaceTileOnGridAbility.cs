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

		private MoveTileAbility _moveTile;
		private ZoneHolderResource _zoneHolder;
		private GridManagerResource _gridManager;
		private CreateHandAbility _createHandAbility;
		private SendInfoAbility _sender;

		public bool TilePlaced
		{
			get;
			private set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_moveTile = owner.GetStateComponent<MoveTileAbility>();
			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
			_createHandAbility = owner.GetStateComponent<CreateHandAbility>();
			_sender = owner.GetStateComponent<SendInfoAbility>();
		}

		public override void OnEnable()
		{
			base.OnEnable();
			TilePlaced = false;
		}

		public void ReleaseTile(Vector2 position)
		{
			if (_moveTile.CurrentTile == null)
				return;

			TileVisu tempTile = _moveTile.CurrentTile;
			_moveTile.CurrentTile = null;
            tempTile.ResetValidityVisual();

            if (_zoneHolder.IsInHand(position))
			{
				_zoneHolder.GiveTileToHand(tempTile.gameObject);
				return;
			}

			Vector2Int pos = Vector2Int.FloorToInt(Camera.main.ScreenToWorldPoint(position));
			TileVisu targetTile = _gridManager.GetTile(pos);

			
			if (targetTile != null && targetTile.TileData == null)
			{
				int connectionCount = _gridManager.GetPlacementConnectionCount(tempTile.TileData, pos);
				if (connectionCount == 0)
				{
					//Debug.Log("Placement invalide");
					_zoneHolder.GiveTileToHand(tempTile.gameObject);
					return;
				}

                tempTile.TileData.OwnerPlayerIndex = GameManager.Instance.PlayerIndex; // On donne l'index du joueur a la tile

                if (GameManager.Instance.FlagTurn)
				{
                    tempTile.TileData.HasFlag = true;
                    targetTile.AddFlagVisual(); // On add le visuel flag manuellement, car filer la data suffit pas a faire ca
                }
                //else { Debug.Log($"Was not flag turn, as it's turn {GameManager.Instance.LocalPlayerTurn}"); }

                _gridManager.SetTile(tempTile.TileData, pos);
				_sender.SendInfoTilePlaced(tempTile.TileData, pos);

				targetTile.SetNewOwner(); // On set l'owner de la tile


                if (!_sender.SendTurnFinished())
				{
					for (int i = 0; i < connectionCount; i++)
					{
						SoloDrawCard();
					}
				}

				TilePlaced = true;

                GameObject.Destroy(tempTile.gameObject);
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
	}
}