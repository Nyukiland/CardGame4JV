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
            TileVisu targetTile = _gridManager.GetTile(pos);

            if (CanPlaceOnGrid && targetTile != null && targetTile.TileData == null)
            {
                if (!IsPlacementValid(_currentTile.TileData, pos))
                {
                    Debug.Log("Placement invalide");
                    _handResource.GiveTileToHand(_currentTile.gameObject);
                    return;
                }

                // Debug.Log("Placement valide");
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
            {
                _handResource.GiveTileToHand(_currentTile.gameObject);
            }
        }

        private bool IsPlacementValid(TileData tileData, Vector2Int pos)
        {
            GridManagerResource grid = _gridManager;

            TileVisu neighbor;
            TileData neighborData;

            ZoneData[] myZones = tileData.Zones;

            // Nord vs sud
            neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y + 1));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[0].environment != neighborData.Zones[2].environment)
                    return false;
            }

            // Est vs Ouest
            neighbor = grid.GetTile(new Vector2Int(pos.x + 1, pos.y));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[1].environment != neighborData.Zones[3].environment)
                    return false;
            }

            // Sud vs Nord
            neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y - 1));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[2].environment != neighborData.Zones[0].environment)
                    return false;
            }

            // Ouest vs est
            neighbor = grid.GetTile(new Vector2Int(pos.x - 1, pos.y));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[3].environment != neighborData.Zones[1].environment)
                    return false;
            }

            return true;
        }

        private void SoloDrawCard()
		{
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			int tileId = drawPile.GetTileIDFromDrawPile();
			if (tileId == -1) return;

			TileSettings tileSettings = null;
			foreach (TileSettings setting in drawPile.AllTileSettings)
			{
				if (setting.IdCode == tileId)
				{
					tileSettings = setting;
					break;
				}
			}

			_createHand.CreateTile(tileSettings);
		}
	}
}