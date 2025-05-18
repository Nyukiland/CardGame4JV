namespace CardGame.StateMachine
{
	public class FightCombinedState : CombinedState
	{
        public FightCombinedState()
        {
            AddSubState(new UpdateGaugeSubState());
            AddSubState(new DoCardEffectSubState());
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