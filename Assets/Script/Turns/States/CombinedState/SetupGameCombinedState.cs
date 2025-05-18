namespace CardGame.StateMachine
{
	public class SetupGameCombinedState : CombinedState
	{
        public SetupGameCombinedState()
        {
            AddSubState(new PlaceElementsSubState()); 
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