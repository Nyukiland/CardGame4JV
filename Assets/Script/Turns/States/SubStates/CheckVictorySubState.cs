namespace CardGame.StateMachine
{
	public class CheckVictorySubState : State
	{
        private HandleStatsResource _handleStatsResource;
        public override void OnEnter()
        {
            base.OnEnter();
            _handleStatsResource = Controller.GetStateComponent<HandleStatsResource>();
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