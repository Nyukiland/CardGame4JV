using CardGame.StateMachine;

namespace CardGame.Turns
{
    public class HUDAbility : Ability
    {
		private HUDResource _hudResource;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_hudResource = owner.GetStateComponent<HUDResource>();
			_hudResource.WaitingScreen.SetActive(false);
		}

		public override void OnEnable()
		{
			_hudResource.WaitingScreen.SetActive(true);
		}

		public override void OnDisable()
		{
			_hudResource?.WaitingScreen.SetActive(false);
			base.OnDisable();
		}
	}
}