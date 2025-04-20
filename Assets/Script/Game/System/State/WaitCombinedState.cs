namespace CardGame.StateMachine
{
	public class WaitCombinedState : CombinedState
	{
		WaitCombinedState()
		{
			AddSubState(new SelectCardSubState());
		}
	}
}