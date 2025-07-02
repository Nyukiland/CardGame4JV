using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using UnityEngine;
using CardGame.Utility;

namespace CardGame.Turns
{
	public class SetupGameCombinedState : CombinedState
	{
		private SendInfoAbility _sender;
		private NetworkResource _net;
		private CreateHandAbility _createHandAbility;
		private AutoPlayAbility _autoPlay;
		private GridManagerResource _gridManager;

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
			GetStateComponent(ref _autoPlay, false);
			GetStateComponent(ref _gridManager);


			WaitStart().Forget();
		}

		private async UniTask WaitStart()
		{
			await UniTask.Delay(200);
			await UniTask.WaitUntil(() => Storage.Instance.GetElement<DrawPile>().AllTileSettings.Count != 0);

			if (_net.IsNetActive())
			{
				_sender.SendGridToOthers();
				await UniTask.Delay(100);
				_sender.AskForSetUp();
				Controller.SetState<NextPlayerCombinedState>();
			}
			else
			{
				_gridManager.GenerateBonusTiles();

				Controller.SetState<PlaceTileCombinedState>();
				GameManager.Instance.ResetManager();
				GameManager.Instance.SetPlayerInfo(1111, "Player");
				GameManager.Instance.SetPlayerInfo(2222, "Bot");
				_createHandAbility.GenerateTiles(_createHandAbility.CountCard);
				_autoPlay.GenerateTheoreticalHand(_createHandAbility.CountCard);
			}
		}
	}
}