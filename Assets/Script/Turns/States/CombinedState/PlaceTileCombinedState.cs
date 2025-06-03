using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class PlaceTileCombinedState : CombinedState
	{
		public PlaceTileCombinedState()
		{
            AddSubState(new MoveTileSubState());
            AddSubState(new RotateTileSubState());
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