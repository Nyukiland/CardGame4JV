using UnityEngine;

namespace CardGame.StateMachine
{
    public class ControlCombinedState : CombinedState
	{

        private MoveCardAbility _moveCardAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _moveCardAbility);
            Debug.Log("ControlCombinedState", Controller);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // Handle screen touches.
            if (Input.touchCount != 1)
                return;

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _moveCardAbility.OnClick(touch.position);
            }

            if (touch.phase == TouchPhase.Moved)
            {
                _moveCardAbility.OnMaintain(touch.position);
            }

            if (touch.phase == TouchPhase.Ended)
            {
                _moveCardAbility.OnRelease();
            }
        }
    }
}