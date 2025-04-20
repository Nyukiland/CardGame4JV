namespace CardGame.StateMachine
{
	public class PlayCardCombinedState : CombinedState
	{
		PlayCardCombinedState()
		{
			AddSubState(new SelectCardSubState());
		}
	}
}