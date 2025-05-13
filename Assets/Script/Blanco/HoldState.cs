using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class HoldState : State
    {
        private TimerAbility _timerAbility;
        
        public override void OnEnter()
        {
            GetStateComponent(ref _timerAbility);
        }

        public override void Update(float deltaTime)
        {
            if (_timerAbility.ElapsedTime > TimerAbility.HOLD_DURATION)
            {
                Controller.SetState(typeof(ClickState));
            }
        }
    }
}