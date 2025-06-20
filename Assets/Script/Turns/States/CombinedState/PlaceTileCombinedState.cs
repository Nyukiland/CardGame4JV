using UnityEngine.InputSystem;
using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class PlaceTileCombinedState : CombinedState
	{
		private MoveTileAbility _moveTile;
		private ZoneHolderResource _zoneResource;
		private PlaceTileOnGridAbility _placeTileOnGrid;

		public PlaceTileCombinedState()
		{
            AddSubState(new MoveTileSubState());
        }

        public override void OnEnter()
        {
            base.OnEnter();

			GetStateComponent(ref _moveTile);
			GetStateComponent(ref _zoneResource);
			GetStateComponent(ref _placeTileOnGrid);
			_moveTile.CanPlaceOnGrid = true;
        }

        public override void OnExit()
        {
            base.OnExit();

			if (_moveTile.CurrentTile != null)
			{
				_zoneResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}
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
    }
}