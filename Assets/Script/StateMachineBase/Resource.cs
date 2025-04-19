namespace CardGame.StateMachine
{
	public class Resource : StateComponent
	{
		public override void EarlyInit()
		{
			base.EarlyInit();
			Enabled = true;
		}
	}
}