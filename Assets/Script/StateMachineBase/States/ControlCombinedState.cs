using UnityEngine;

namespace CardGame.StateMachine
{
    public class ControlCombinedState : CombinedState
    {
        private MoveCardAbility _moveAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _moveAbility);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 pos = touch.position;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _moveAbility.OnTouchDown(pos);
                        break;

                    case TouchPhase.Moved:

                    case TouchPhase.Stationary:
                        _moveAbility.OnTouchHold(pos);
                        break;

                    case TouchPhase.Ended:

                    case TouchPhase.Canceled:
                        if (_moveAbility.OnTouchRelease())
                        {
                            Controller.SetState<HoldCombinedState>();
                        }
                        break;
                }
            }
        }
    }
}
