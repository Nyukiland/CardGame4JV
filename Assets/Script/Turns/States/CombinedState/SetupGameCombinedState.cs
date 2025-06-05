using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class SetupGameCombinedState : CombinedState
	{
		private SendInfoAbility _sender;
		private NetworkResource _net;

		public SetupGameCombinedState()
		{
			AddSubState(new PlaceElementsSubState());
		}

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _net);
			GetStateComponent(ref _sender);

			if (_net.IsNetActive()) _sender.AskForSetUp();
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			//in the update so it has a delay
			if (!_net.IsNetActive())
				Controller.SetState<PlaceTileCombinedState>();
			else
				Controller.SetState<NextPlayerCombinedState>();

		}
	}
}