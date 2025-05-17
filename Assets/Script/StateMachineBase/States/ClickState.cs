using UnityEngine;

namespace CardGame.StateMachine
{
    public class ClickState : State
    {
        public override void OnEnter()
        {
            Debug.Log("ClickState");
        }


        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            // Handle screen touches.
            if (Input.touchCount == 1)
                Controller.SetState<HoldState>();
        }
    }
}