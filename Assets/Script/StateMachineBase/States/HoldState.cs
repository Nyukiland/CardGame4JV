using CardGame.StateMachine;

namespace CardGame.StateMachine
{
    public class HoldState : State
    {
        private TimerAbility _timer;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _timer);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_timer != null && _timer.IsFinished())
            {
                Controller.SetState<ClickState>();
            }
        }
    }
}
