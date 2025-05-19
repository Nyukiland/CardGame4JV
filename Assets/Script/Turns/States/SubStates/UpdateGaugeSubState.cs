using CardGame.StateMachine;

namespace CardGame.Turns
{
    public class UpdateGaugeSubState : State
    {

        private HandleGaugeResource _handleGaugeResource;

        public override void OnEnter()
        {
            base.OnEnter();
            _handleGaugeResource = Controller.GetStateComponent<HandleGaugeResource>();
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