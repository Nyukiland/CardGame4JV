namespace CardGame.StateMachine
{
	public class SelectFightersCombinedState : CombinedState
	{
        public SelectFightersCombinedState()
        {
            AddSubState(new SelectTargetSubState());
        }

        public override void OnEnter()
        {
            base.OnEnter();
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