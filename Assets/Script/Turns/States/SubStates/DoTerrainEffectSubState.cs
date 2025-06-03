using CardGame.StateMachine;

namespace CardGame.Turns
{
    public class DoTerrainEffectSubState : State
    {
        private DoTerrainEffectAbility _doEffectAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _doEffectAbility);
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