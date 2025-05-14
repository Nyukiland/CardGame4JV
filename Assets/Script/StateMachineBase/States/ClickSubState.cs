using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.StateMachine
{
    public class ClickSubState : State
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Entered ClickSubState");
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Controller.SetState<HoldCombinedState>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Exiting ClickSubState");
        }
    }
}
