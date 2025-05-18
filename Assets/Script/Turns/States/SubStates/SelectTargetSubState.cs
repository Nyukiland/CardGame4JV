namespace CardGame.StateMachine
{
	public class SelectTargetSubState : State
	{
        private SelectTargetAbility _selectTargetAbility;
        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _selectTargetAbility);
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
