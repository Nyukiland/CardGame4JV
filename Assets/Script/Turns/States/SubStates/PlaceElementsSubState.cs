using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class PlaceElementsSubState : State
	{
		private CreateHandAbility _createHandAbility;

		private NetworkResource _network;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _createHandAbility);

			//add later
			//if (!_network.IsNetActive())

			_createHandAbility.GenerateTiles(_createHandAbility.CountCard);
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