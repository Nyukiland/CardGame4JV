using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using UnityEngine;

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
			await UniTask.Delay(200);

			if (_net.IsNetActive())
			{
				Controller.SetState<NextPlayerCombinedState>();
				await UniTask.Delay(100);
				_sender.AskForSetUp();
			}
			else
			{
				Controller.SetState<PlaceTileCombinedState>();
				_createHandAbility.GenerateTiles(_createHandAbility.CountCard);
			}
		}
	}
}