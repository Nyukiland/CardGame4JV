using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class PlaceTileCombinedState : CombinedState
	{
		private MoveTileAbility _moveTile;
		private ZoneHolderResource _zoneResource;
		private PlaceTileOnGridAbility _placeTileOnGrid;
		private GridManagerResource _gridManagerRessource;

		private List<TileVisu> _previewTiles;
		private HUDResource _hudResource;

		public PlaceTileCombinedState()
		{
			AddSubState(new MoveTileSubState(true));
			AddSubState(new TauntSubState());
		}

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _moveTile);
			GetStateComponent(ref _zoneResource);
			GetStateComponent(ref _placeTileOnGrid);
			GetStateComponent(ref _gridManagerRessource);

			_moveTile.OnCardPicked += HandleCardPicked;
			_placeTileOnGrid.OnCardReleased += HandleCardReleased;

			_moveTile.CanPlaceOnGrid = true;
			_previewTiles = new List<TileVisu>();

			_hudResource = Controller.GetStateComponent<HUDResource>();
			_hudResource.UpdateFlag();
			_hudResource.ToggleNextTurnButton(true);
			_hudResource.UpdateTurnValue();
		}

		public override void OnExit()
		{
			base.OnExit();

			if (_moveTile.CurrentTile != null)
			{
				_zoneResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}

			_moveTile.OnCardPicked -= HandleCardPicked;
			_placeTileOnGrid.OnCardReleased -= HandleCardReleased;
			_hudResource.ToggleNextTurnButton(false);
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name != "Touch")
				return;

			if (context.phase == InputActionPhase.Canceled)
			{
				_placeTileOnGrid.ReleaseTile(Controller.GetActionValue<Vector2>("TouchPos"));
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_placeTileOnGrid.TilePlaced)
				Controller.SetState<DiscardCombinedState>();

		}

		private void HandleCardPicked()
		{
			foreach (var tile in _gridManagerRessource.SurroundingTilePos)
			{
				TileVisu tempTile = _gridManagerRessource.GetTile(tile.x, tile.y);

				if (tempTile.TileData != null) continue;

				_previewTiles.Add(tempTile); //On pourrait le mettre en dessous et stocker que les modified, mais ca pourrait servir

				if (_gridManagerRessource.GetPlacementConnectionCount(_moveTile.CurrentTile.TileData, tile) != 0)
				{
					tempTile.ChangeValidityVisual(true); // Si valide, on affiche un feedback, on fait rien sinon
					_previewTiles.Add(tempTile);
				}
			}
		}

		private void HandleCardReleased()
		{
			foreach (var tile in _previewTiles)
			{
				tile.ResetValidityVisual();
			}
			_previewTiles.Clear();
		}

	}
}