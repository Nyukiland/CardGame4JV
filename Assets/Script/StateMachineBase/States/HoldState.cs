using UnityEngine;

namespace CardGame.StateMachine
{
	public class HoldState : State
	{
        private TimerAbility _timerAbility;

        public override void OnEnter()
        {
            Debug.Log("HoldState");
            GetStateComponent(ref _timerAbility);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_timerAbility.GetElapsedTime() >= 2.0f)
                Controller.SetState<ClickState>();
        }
    }
}
