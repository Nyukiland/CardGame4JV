namespace CardGame.StateMachine
{
	public abstract class Ability : StateComponent
	{
		public override void OnEnable()
		{
			base.OnEnable();
			Enabled = true;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			Enabled = false;
		}
	}
}