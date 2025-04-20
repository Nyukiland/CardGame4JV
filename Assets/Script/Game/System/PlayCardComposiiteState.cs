namespace CardGame.StateMachine
{
	public class PlayCardComposiiteState : CombinedState
	{
		PlayCardComposiiteState()
		{
			AddSubState(new SelectCardSubState());
		}
	}
}