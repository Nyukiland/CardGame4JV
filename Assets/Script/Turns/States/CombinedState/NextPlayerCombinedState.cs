using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class NextPlayerCombinedState : CombinedState
	{
        public NextPlayerCombinedState()
        {
            AddSubState(new CheckVictorySubState());
            AddSubState(new DrawCardSubState());
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