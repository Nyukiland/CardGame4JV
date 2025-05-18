using UnityEngine;

namespace CardGame.StateMachine
{
    public class ControlCombinedState : CombinedState
	{
        public ControlCombinedState()
        {
            Debug.Log("ControlCombinedState");
            AddSubState(new ClickSubState());
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // Handle screen touches.
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Controller.SetState<HoldCombinedState>();
                }
            }
        }
    }
}