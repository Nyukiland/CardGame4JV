using CardGame.StateMachine;
using CardGame.Utility;
using Cysharp.Threading.Tasks;

namespace CardGame.Turns
{
	public class SetupGameCombinedState : CombinedState
	{
		private SendInfoAbility _sender;
		private NetworkResource _net;
		private CreateHandAbility _createHandAbility;

		public SetupGameCombinedState()
		{
			AddSubState(new PlaceElementsSubState());
		}

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _net);
			GetStateComponent(ref _sender);
			GetStateComponent(ref _createHandAbility);


			WaitStart().Forget();
		}

		private async UniTask WaitStart()
		{
			await UniTask.Delay(500);

			if (_net.IsNetActive())
			{
				_sender.AskForSetUp();
				Controller.SetState<NextPlayerCombinedState>();
			}
			else
			{
				Controller.SetState<PlaceTileCombinedState>();
				_createHandAbility.GenerateTiles(_createHandAbility.CountCard);
			}
		}
	}
}