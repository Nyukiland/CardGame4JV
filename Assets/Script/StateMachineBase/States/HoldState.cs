using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.StateMachine
{
    public class HoldState : State
    {
        private float _timer;

        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0f;
            Debug.Log("Entered HoldState");
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _timer += deltaTime;

            if (_timer >= 2f) //On attend 2sec, avec la marge du tickrate
            {
                Controller.SetState<ClickState>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Exiting HoldState");
        }
    }
}
