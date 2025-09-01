using CardGame.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class DiscardCombinedState : CombinedState
	{
		private DiscardCardAbility _discardCard;
		private MoveTileAbility _moveTile;
		private SendInfoAbility _sendInfo;
		private ZoneHolderResource _zoneHolder;
		private NetworkResource _networkResource;

		public DiscardCombinedState()
		{
			AddSubState(new MoveTileSubState(true));
		}

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _discardCard);
			GetStateComponent(ref _moveTile);
			GetStateComponent(ref _sendInfo);
			GetStateComponent(ref _zoneHolder);
			GetStateComponent(ref _networkResource);

			_moveTile.CanPlaceOnGrid = false;

			CheckDiscardState().Forget();
		}

		private async UniTask CheckDiscardState()
		{
			if (_networkResource.IsNetActive())
			{
				await UniTask.WaitUntil(() => _networkResource.TileToReceive == 0);
			}

			UnityEngine.Debug.Log("t");

			if (_discardCard.DiscardFinished())
			{
				CallEndTurn();
				return;
			}

			_discardCard.ShowDiscardArea(true);
			Controller.GetStateComponent<HUDResource>().ChangeTurnFeedback(HUDResource.TurnState.Discard);
		}

		public override void OnExit()
		{
			base.OnExit();

			if (_moveTile.CurrentTile != null)
			{
				_zoneHolder.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}

			_networkResource.TileToReceive = 100;
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name != "Touch")
				return;

			if (context.phase == InputActionPhase.Canceled)
			{
				_discardCard.ReleaseCard(Controller.GetActionValue<Vector2>("TouchPos"));
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_discardCard.DiscardFinished())
			{
				CallEndTurn();
			}
		}

		private void CallEndTurn()
		{
			_discardCard.ShowDiscardArea(false);
			_sendInfo.SendTurnFinished();

			Controller.GetStateComponent<ScoringAbility>().SetState(typeof(NextPlayerCombinedState));
			Controller.SetState<ScoringCombinedState>();
		}
	}
}