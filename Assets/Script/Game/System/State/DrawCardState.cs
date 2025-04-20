namespace CardGame.StateMachine
{
	public class DrawCardState : State
	{
		private AbilityToCreateCard _createCard;

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _createCard);
			_createCard.GenerateCards();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_createCard.IsFinished())
				Controller.SetState<PlayCardCombinedState>();
		}
	}
}