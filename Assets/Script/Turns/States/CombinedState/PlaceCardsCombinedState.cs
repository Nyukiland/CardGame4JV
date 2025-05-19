using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class PlaceCardsCombinedState : CombinedState
	{
		public PlaceCardsCombinedState()
		{
            AddSubState(new UpdateGaugeSubState());
            AddSubState(new DoCardEffectSubState());
            AddSubState(new MoveCardSubState());
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