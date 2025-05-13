using CardGame.StateMachine;

namespace CardGame.Blanco
{
    public class HoldState : State
    {
        private float _elapsedTime;
        private const float HOLD_DURATION = 2f;

        public override void OnEnter()
        {
            _elapsedTime = 0f;
        }

        public override void Update(float deltaTime)
        {
            if (_elapsedTime > HOLD_DURATION)
                Controller.SetState(typeof(ClickState));
            else
                _elapsedTime += deltaTime;
        }
    }
}