using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.UI;
using UnityEngine;
using CardGame.Card;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		private GridManagerResource _gridManager;
		private SendInfoAbility _sender;
		private ZoneHolderResource _handResource;
		private CreateHandAbility _createHand;

		private TileVisu _currentTile;

		public bool CanPlaceOnGrid { get; set; } = false;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_handResource = owner.GetStateComponent<ZoneHolderResource>();
			_sender = owner.GetStateComponent<SendInfoAbility>();
            _gridManager = owner.GetStateComponent<GridManagerResource>();
			_createHand = owner.GetStateComponent<CreateHandAbility>();
        }

		public void PickCard(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					_handResource.RemoveTileFromHand(visu.gameObject);
					_currentTile = visu;
				}
			}
		}

		public void MoveCard(Vector2 position)
		{
			if (_currentTile == null)
				return;

			Vector3 pos = Camera.main.ScreenToWorldPoint(position);
			pos = Vector3Int.FloorToInt(pos);
			pos += Camera.main.transform.forward * 2;

			_currentTile.transform.position = pos;
		}

		public void ReleaseCard(Vector2 position)
		{
			if (_currentTile == null)
				return;

			if (_handResource.IsInHand(position))
			{
				_handResource.GiveTileToHand(_currentTile.gameObject);
				return;
			}

			Vector2Int pos = Vector2Int.FloorToInt(Camera.main.ScreenToWorldPoint(position));

			if (CanPlaceOnGrid && _gridManager.GetTile(pos) != null && _gridManager.GetTile(pos).TileData == null)
			{
				_gridManager.SetTile(_currentTile.TileData, pos);

				_sender.SendInfoTilePlaced(_currentTile.TileData, pos);
				if (!_sender.SendTurnFinished())
				{
					SoloDrawCard();
				}

				Owner.SetState<NextPlayerCombinedState>();

				GameObject.Destroy(_currentTile.gameObject);
			}
			else 
				_handResource.GiveTileToHand(_currentTile.gameObject);
		}

		private void SoloDrawCard()
		{
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			int tileId = drawPile.GetTileIDFromDrawPile();
			if (tileId != -1) return;

			TileSettings tileSettings = null;
			foreach (TileSettings setting in drawPile.AllTileSettings)
			{
				if (setting.IdCode == tileId)
				{
					tileSettings = setting;
					break;
				}
			}

			UnityEngine.Debug.Log("t");
			_createHand.CreateTile(tileSettings);
		}
	}
}