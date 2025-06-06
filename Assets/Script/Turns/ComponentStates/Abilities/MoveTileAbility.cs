using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		private GridManager _gridManager;

		private SendInfoAbility _sender;
		private ZoneHolderResource _handResource;

		private TileVisu _currentTile;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_handResource = owner.GetStateComponent<ZoneHolderResource>();
			_sender = owner.GetStateComponent<SendInfoAbility>();
            _gridManager = owner.GetStateComponent<GridManager>();

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

			if (_gridManager.GetTile(pos) != null && _gridManager.GetTile(pos).TileData == null)
			{
				_gridManager.SetTile(_currentTile.TileData, pos);

				_sender.SendInfoTilePlaced(_currentTile.TileData, pos);
				_sender.SendTurnFinished();

				Owner.SetState<NextPlayerCombinedState>();

				GameObject.Destroy(_currentTile.gameObject);
			}
			else 
				_handResource.GiveTileToHand(_currentTile.gameObject);

		}
	}
}