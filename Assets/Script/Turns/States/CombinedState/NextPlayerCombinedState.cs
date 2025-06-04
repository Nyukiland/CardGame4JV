using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class NextPlayerCombinedState : CombinedState
	{
		private NetworkResource _net;

        public NextPlayerCombinedState()
        {
			AddSubState(new RotateTileSubState());
			AddSubState(new MoveTileSubState());
        }

        public override void OnEnter()
        {
            base.OnEnter();
			GetStateComponent(ref _net);

			if (!_net.IsNetActive())
				Controller.SetState<PlaceTileCombinedState>();
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