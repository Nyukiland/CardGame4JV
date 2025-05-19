using UnityEngine;

namespace CardGame.StateMachine
{
    public class ClickSubState : State
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("ClickSubState");
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}