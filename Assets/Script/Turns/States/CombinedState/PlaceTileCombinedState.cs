using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class PlaceTileCombinedState : CombinedState
	{
		private MoveTileAbility _moveTileAbility;

		public PlaceTileCombinedState()
		{
            AddSubState(new MoveTileSubState());
        }

        public override void OnEnter()
        {
            base.OnEnter();

			GetStateComponent(ref _moveTileAbility);
			_moveTileAbility.CanPlaceOnGrid = true;
        }

        public override void OnExit()
        {
            base.OnExit();
			_moveTileAbility.CanPlaceOnGrid = false;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}