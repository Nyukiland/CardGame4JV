using CardGame.StateMachine;

namespace CardGame.Turns
{
    public class MoveCardSubState : State
    {
        private MoveCardAbility _moveCardAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _moveCardAbility);
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