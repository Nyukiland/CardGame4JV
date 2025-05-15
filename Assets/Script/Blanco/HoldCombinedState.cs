using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class HoldCombinedState : CombinedState
    {
        private TimerAbility _timerAbility;
        
        public HoldCombinedState()
        {
            AddSubState(new ClickSubState());
        }
        
        public override void OnEnter()
        {
            GetStateComponent(ref _timerAbility);
        }

        public override void Update(float deltaTime)
        {
            if (_timerAbility.ElapsedTime > TimerAbility.HOLD_DURATION)
            {
                Controller.SetState(typeof(ControlCombinedState));
            }
        }
    }
}