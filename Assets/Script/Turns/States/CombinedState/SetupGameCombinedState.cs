using CardGame.StateMachine;
using CardGame.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CardGame.Turns
{
	public class SetupGameCombinedState : CombinedState
	{
		private SendInfoAbility _sender;
		private NetworkResource _net;
		private CreateHandAbility _createHandAbility;
		private AutoPlayAbility _autoPlay;
		private GridManagerResource _gridManager;
		private HUDResource _hudResource;
		private ZoomAbility _zoomAbility;

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _net);
			GetStateComponent(ref _sender);
			GetStateComponent(ref _createHandAbility);
			GetStateComponent(ref _autoPlay, false);
			GetStateComponent(ref _gridManager);
			GetStateComponent(ref _hudResource);
			GetStateComponent(ref _zoomAbility);

			WaitStart().Forget();
		}

		private async UniTask WaitStart()
		{
			await UniTask.DelayFrame(70); //the worst I did so far
			await UniTask.WaitUntil(() => _net.IsWaitNetComplete);
			await UniTask.WaitUntil(() => Storage.Instance.GetElement<DrawPile>().AllTileSettings.Count != 0);

			Controller.GetStateComponent<ZoneHolderResource>().UpdatePlacementInHand();

			if (_net.IsNetActive())
			{
				await UniTask.DelayFrame(5);
				_sender.SendGridToOthers();
				await UniTask.DelayFrame(10);
				_sender.AskForSetUp();
				Controller.SetState<NextPlayerCombinedState>();
			}
			else
			{
				_gridManager.GenerateBonusTiles();

				GameManager.Instance.ResetManager();
				GameManager.Instance.SetPlayerInfo(1111, "Player");
				GameManager.Instance.SetPlayerInfo(2222, "Bot");

				await UniTask.Yield();

				_createHandAbility.GenerateTiles(_createHandAbility.CountCard);
				_autoPlay.GenerateTheoreticalHand(_createHandAbility.CountCard);
				Controller.GetStateComponent<HUDResource>().InitScores();

				Controller.SetState<PlaceTileCombinedState>();
				_hudResource.CloseLoadingScreen();
			}

			while (Camera.main.orthographicSize > 3.5f)
			{
				UnityEngine.Debug.Log("t");
				_zoomAbility.ZoomInProcess(-0.01f);
				await UniTask.Yield();
			}
		}
	}
}