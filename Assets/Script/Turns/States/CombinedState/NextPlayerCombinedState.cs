using CardGame.StateMachine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class NextPlayerCombinedState : CombinedState
	{
		private NetworkResource _net;
		private HUDResource _hud;
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
			GetStateComponent(ref _hud);
			GetStateComponent(ref _moveTile);
	        GetStateComponent(ref _autoPlayAbility);

			_hud.OpenWaitingScreen();

			_moveTile.CanPlaceOnGrid = false;

			if (!_net.IsNetActive())
			{
				GetStateComponent(ref _autoPlayAbility);
				_autoPlayAbility.CallBotTurn();
			}
			Controller.GetStateComponent<HUDResource>().UpdateTurnValue();
		}

		public override void OnExit()
		{
			base.OnExit();

			_hud.CloseWaitingScreen();

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

			if (GameManager.Instance.GameIsFinished)
			{ 
				Controller.GetStateComponent<ScoringAbility>().SetState(typeof(EndGameCombinedState));
				Controller.SetState<ScoringCombinedState>();
			}

			if (!_net.IsNetActive())
			{
				if (_autoPlayAbility.IsFinished)
				{
					Controller.GetStateComponent<ScoringAbility>().SetState(typeof(PlaceTileCombinedState));
					Controller.SetState<ScoringCombinedState>();
				}
			}
			else if (_net.IsFinished)
			{
				_net.IsFinished = false;
				Controller.GetStateComponent<ScoringAbility>().SetState(typeof(PlaceTileCombinedState));
				Controller.SetState<ScoringCombinedState>();
			}
		}
	}
}