using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class NextPlayerCombinedState : CombinedState
	{
		private NetworkResource _net;
		private HUDResource _hudResource;
		private HUDAbility _hudAbility;
		private AutoPlayAbility _autoPlayAbility;

        public NextPlayerCombinedState()
        {
			//AddSubState(new RotateTileSubState());
			//AddSubState(new MoveTileSubState());
		}

        public override void OnEnter()
        {
            base.OnEnter();
			GetStateComponent(ref _net);
			GetStateComponent(ref _hudResource);
			GetStateComponent(ref _hudAbility);

            if (!_net.IsNetActive())
				GetStateComponent(ref _autoPlayAbility);
        }
    }
}