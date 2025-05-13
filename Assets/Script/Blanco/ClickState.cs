using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class ClickState : State
    {
        public override void Update(float deltaTime)
        {
            if (Input.touchCount > 0)
            {
                Touch currentTouch = Input.GetTouch(0);
                if (currentTouch.phase == TouchPhase.Began)
                {
                    Controller.SetState(typeof(HoldState));
                }
            }
        }
    }
}