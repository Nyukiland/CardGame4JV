using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class FightCombinedState : CombinedState
	{
        public FightCombinedState()
        {
            AddSubState(new UpdateGaugeSubState());
            AddSubState(new DoTerrainEffectSubState());
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