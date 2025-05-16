using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class ControlCombinedState : CombinedState
    {
        private MoveCardAbility _moveCardAbility;
        private ObjectManagerResource _objectManagerResource;
        private Touch _currentTouch;

        public override void OnEnter()
        {
            GetStateComponent(ref _moveCardAbility);
            GetStateComponent(ref _objectManagerResource);
        }

        public override void Update(float deltaTime)
        {
            if (Input.touchCount > 0)
            {
                _currentTouch = Input.GetTouch(0);
                switch (_currentTouch.phase)
                {
                    case TouchPhase.Began :
                        _moveCardAbility.Click(_currentTouch.position);
                        break;
                    case TouchPhase.Moved :
                        _moveCardAbility.HoldClick(_currentTouch.position);
                        break;
                    case TouchPhase.Ended :
                        _moveCardAbility.ReleaseClick();
                        break;
                }
            }
        }
    }
}