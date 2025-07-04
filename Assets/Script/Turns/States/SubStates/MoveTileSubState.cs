using CardGame.StateMachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	//	This is now the worst script of the entire project
	//	I highly regret not deviding it but changing it is too much work and i'm kinda bored
	//	So yep sorry
	//	
	//	---Roman---
	public class MoveTileSubState : State
	{
		private MoveTileAbility _moveCardAbility;
		private RotateTileAbility _rotateCardAbility;
		private MoveCameraAbility _moveCameraAbility;
		private TauntShakeTileAbility _tauntShakeTileAbility;
		private ZoomAbility _zoomAbility;

		private bool _isHolding;
		private CancellationTokenSource _cancelToken;

		private Vector2 _startPos;

		private readonly bool MoveCam;

		public MoveTileSubState()
		{
			MoveCam = false;
		}

		public MoveTileSubState(bool canMoveCam)
		{
			MoveCam = canMoveCam;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _moveCardAbility);
			GetStateComponent(ref _rotateCardAbility);
			GetStateComponent(ref _moveCameraAbility);
			GetStateComponent(ref _tauntShakeTileAbility);
			GetStateComponent(ref _zoomAbility);
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name == "Touch")
			{
				if (context.phase == InputActionPhase.Performed)
				{
					_startPos = Controller.GetActionValue<Vector2>("TouchPos");
					//Debug.Log("Start touching");

					_cancelToken = new CancellationTokenSource();
					if (_moveCardAbility.QuickCheckRay(_startPos))
						DetectHold(_startPos, _cancelToken.Token).Forget();
					else if (!_tauntShakeTileAbility.QuickCheckRay(_startPos))
						_moveCameraAbility.StartMoving(_startPos);
				}
				else if (context.phase == InputActionPhase.Canceled)
				{
					//Debug.Log("End touching");

					_cancelToken?.Cancel();
					_cancelToken = null;

					if (!_isHolding)
					{
						//Debug.Log("Tap detected -> Rotate");
						_rotateCardAbility.RotateCard(_startPos);
					}

					_moveCameraAbility.StopMoving();

					_isHolding = false;
				}
			}

			if (context.action.name == "Touch2")
			{

				if (context.phase == InputActionPhase.Performed)
				{
					if (!_moveCameraAbility.InUse)
						return;

					_moveCameraAbility.StopMoving();

					Vector2 pos1 = Controller.GetActionValue<Vector2>("TouchPos");
					Vector2 pos2 = Controller.GetActionValue<Vector2>("TouchPos2");
					_zoomAbility.StartZoom(pos1, pos2);
				}
				else if (context.phase == InputActionPhase.Canceled)
				{
					if (!_zoomAbility.InZoom)
						return;

					_zoomAbility.StopZoom();

					if (context.action.actionMap.FindAction("Touch").phase == InputActionPhase.Performed)
						_moveCameraAbility.StartMoving(Controller.GetActionValue<Vector2>("TouchPos"));
				}
			}
		}

		private async UniTaskVoid DetectHold(Vector2 pos, CancellationToken token)
		{
			try
			{
				await UniTask.Delay(200, cancellationToken: token); // 100 ms to hold

				if (token.IsCancellationRequested)
					return;

				_isHolding = true;
				//Debug.Log("Start holding");

				_moveCardAbility.PickCard(_startPos);
			}
			catch (OperationCanceledException)
			{

			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			Vector2 currentPos = Controller.GetActionValue<Vector2>("TouchPos");

			if (_isHolding)
				_moveCardAbility.MoveCard(currentPos);

			_moveCameraAbility.MoveCamera(currentPos);
			_zoomAbility.ZoomInProcess(currentPos, Controller.GetActionValue<Vector2>("TouchPos2"));
		}
	}
}
