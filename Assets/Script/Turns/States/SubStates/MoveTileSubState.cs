using CardGame.StateMachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
    public class MoveTileSubState : State
    {
        private MoveTileAbility _moveCardAbility;
        private RotateTileAbility _rotateCardAbility;

        private bool _isHolding;
        private CancellationTokenSource _cancelToken;

        private Vector2 _startPos;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _moveCardAbility);
            GetStateComponent(ref _rotateCardAbility);
        }

        public override void OnActionTriggered(InputAction.CallbackContext context)
        {
            base.OnActionTriggered(context);

            if (context.action.name != "Touch")
                return;

            if (context.phase == InputActionPhase.Performed)
            {
                _startPos = Controller.GetActionValue<Vector2>("TouchPos");
                //Debug.Log("Start touching");

                _cancelToken = new CancellationTokenSource();
                DetectHold(_startPos, _cancelToken.Token).Forget();
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

                _isHolding = false;
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

                _moveCardAbility.PickCard(pos);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isHolding)
            {
                Vector2 currentPos = Controller.GetActionValue<Vector2>("TouchPos");
                _moveCardAbility.MoveCard(currentPos);
            }
        }
    }
}
