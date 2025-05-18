using UnityEngine;

namespace CardGame.StateMachine
{
	public class HoldCombinedState : CombinedState
    {
        private TimerAbility _timerAbility;

        public HoldCombinedState()
        {
            Debug.Log("HoldCombinedState");
            AddSubState(new ClickSubState());
        }

        public override void OnEnter()
        {
            GetStateComponent(ref _timerAbility);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_timerAbility.GetElapsedTime() >= 2.0f)
                Controller.SetState<ControlCombinedState>();

            // Handle screen touches.
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Controller.SetState<ControlCombinedState>();
                }
            }
        }
    }
}
