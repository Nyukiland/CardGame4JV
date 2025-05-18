namespace CardGame.StateMachine
{
	public class PlaceElementsSubState : State
	{
        private CalculateHandResource _calculateHandResource;
        private HandleGaugeResource _handleGaugeResource;
        private CreateHandAbility _createHandAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            _calculateHandResource = Controller.GetStateComponent<CalculateHandResource>();
            _handleGaugeResource = Controller.GetStateComponent<HandleGaugeResource>();
            GetStateComponent(ref _createHandAbility);
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