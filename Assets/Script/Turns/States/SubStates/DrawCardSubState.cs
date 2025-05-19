using CardGame.StateMachine;

namespace CardGame.Turns
{
	public class DrawCardSubState : State
	{
        private HoldHandResource _holdHandResource;
        private CreateCardAbility _createCardAbility;

        public override void OnEnter()
        {
            base.OnEnter();
            _holdHandResource = Controller.GetStateComponent<HoldHandResource>();
            GetStateComponent(ref _createCardAbility);
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