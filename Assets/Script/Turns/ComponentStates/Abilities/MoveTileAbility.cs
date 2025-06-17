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

            // Deplacement de la tuile
            Vector3 pos = Camera.main.ScreenToWorldPoint(position);
            pos = Vector3Int.FloorToInt(pos);
            pos += Camera.main.transform.forward * 2;
            _currentTile.transform.position = pos;

            // Verification de sa validity
            if (_handResource.IsInHand(position)) //on fait rien quand c'est dans la main
            {
                _currentTile.ResetValidityVisual();
                return;
            }

            Vector2Int gridPos = Vector2Int.FloorToInt(Camera.main.ScreenToWorldPoint(position));
            TileVisu target = _gridManager.GetTile(gridPos);

            if (target == null || target.TileData != null)
            {
                _currentTile.ChangeValidityVisual(false); // noir
            }
            else
            {
                int connections = GetPlacementConnectionCount(_currentTile.TileData, gridPos);
                _currentTile.ChangeValidityVisual(connections > 0); // jaune si > 0, sinon noir
            }
        }



        public void ReleaseCard(Vector2 position)
        {
            if (_currentTile == null)
                return;

            if (_handResource.IsInHand(position))
            {
                _currentTile.ResetValidityVisual();
                _handResource.GiveTileToHand(_currentTile.gameObject);
                return;
            }

            Vector2Int pos = Vector2Int.FloorToInt(Camera.main.ScreenToWorldPoint(position));
            TileVisu targetTile = _gridManager.GetTile(pos);

            _currentTile.ResetValidityVisual();

            if (CanPlaceOnGrid && targetTile != null && targetTile.TileData == null)
            {
                int connectionCount = GetPlacementConnectionCount(_currentTile.TileData, pos);
                if (connectionCount == 0)
                {
                    //Debug.Log("Placement invalide");
                    _handResource.GiveTileToHand(_currentTile.gameObject);
                    return;
                }

                _gridManager.SetTile(_currentTile.TileData, pos);
                _sender.SendInfoTilePlaced(_currentTile.TileData, pos);

                if (!_sender.SendTurnFinished())
                {
                    for (int i = 0; i < connectionCount; i++)
                    {
                        SoloDrawCard();
                    }
                }

                Owner.SetState<NextPlayerCombinedState>();
                GameObject.Destroy(_currentTile.gameObject);
            }
            else
            {
                _handResource.GiveTileToHand(_currentTile.gameObject);
            }
        }


        private int GetPlacementConnectionCount(TileData tileData, Vector2Int pos)
        {
            int connections = 0;

            ZoneData[] myZones = tileData.Zones;

            (GridManagerResource grid, TileVisu neighbor, TileData neighborData) = (_gridManager, null, null);

            // Nord vs sud
            neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y + 1));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[0].environment != neighborData.Zones[2].environment)
                    return 0;
                connections++;
            }

            // Est vs Ouest
            neighbor = grid.GetTile(new Vector2Int(pos.x + 1, pos.y));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[1].environment != neighborData.Zones[3].environment)
                    return 0;
                connections++;
            }

            // Sud vs Nord
            neighbor = grid.GetTile(new Vector2Int(pos.x, pos.y - 1));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[2].environment != neighborData.Zones[0].environment)
                    return 0;
                connections++;
            }

            // Ouest vs est
            neighbor = grid.GetTile(new Vector2Int(pos.x - 1, pos.y));
            if (neighbor != null && (neighborData = neighbor.TileData) != null)
            {
                if (myZones[3].environment != neighborData.Zones[1].environment)
                    return 0;
                connections++;
            }

            //Debug.Log($"Placement valide avec {connections}");
            return connections;
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