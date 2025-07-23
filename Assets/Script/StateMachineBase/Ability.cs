namespace CardGame.StateMachine
{
	public abstract class Ability : StateComponent
	{
		protected override bool CanChangeActivity => true;
	}
}