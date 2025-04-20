namespace CardGame.StateMachine
{
	public class PlayCardCombinedState : CombinedState
	{
		public PlayCardCombinedState()
		{
			AddSubState(new SelectCardSubState());
		}
	}
}