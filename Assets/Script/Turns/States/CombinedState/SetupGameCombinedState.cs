using CardGame.StateMachine;

namespace CardGame.Turns
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

			//in the update so it has a delay
			Controller.SetState<PlaceTileCombinedState>();
        }
    }
}