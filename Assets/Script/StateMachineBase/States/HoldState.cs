using UnityEngine;

namespace CardGame.StateMachine
{
	public class HoldState : State
	{
        private float _elapsedTime;

        public override void OnEnter()
        {
            Debug.Log("HoldState");
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _elapsedTime += deltaTime;

            if (_elapsedTime >= 2.0f)
                Controller.SetState<ClickState>();
        }
    }
}
