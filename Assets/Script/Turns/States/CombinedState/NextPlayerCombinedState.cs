using CardGame.StateMachine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class NextPlayerCombinedState : CombinedState
	{
		private NetworkResource _net;
		private HUDAbility _hudAbility;
		private AutoPlayAbility _autoPlayAbility;
		private ZoneHolderResource _handResource;
		private MoveTileAbility _moveTile;

        public NextPlayerCombinedState()
        {
			AddSubState(new MoveTileSubState(true));
			AddSubState(new TauntSubState());
		}

        public override void OnEnter()
        {
            base.OnEnter();
			GetStateComponent(ref _net);
			GetStateComponent(ref _handResource);
			GetStateComponent(ref _hudAbility);
			GetStateComponent(ref _moveTile);

			_moveTile.CanPlaceOnGrid = false;

            if (!_net.IsNetActive())
				GetStateComponent(ref _autoPlayAbility);
        }

		public override void OnExit()
		{
			base.OnExit();

			if (_moveTile.CurrentTile != null)
			{
				_handResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
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
				if (_moveTile.CurrentTile == null)
					return;

				_handResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (!_net.IsNetActive())
			{
				if (_autoPlayAbility.IsFinished)
					Controller.SetState<PlaceTileCombinedState>();
			}
			else if (_net.IsFinished)
			{
				_net.IsFinished = false;
				Controller.SetState<PlaceTileCombinedState>();
			}
		}
    }
}