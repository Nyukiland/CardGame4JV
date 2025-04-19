namespace CardGame.StateMachine
{
	public abstract class Resource : StateComponent
	{
		public override void EarlyInit()
		{
			base.EarlyInit();
			Enabled = true;
		}
	}
}