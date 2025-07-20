using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class EndGameCombinedState : CombinedState
	{
		private HUDResource _hud;
		private ZoneHolderResource _holder;

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _hud);
			GetStateComponent(ref _holder);

			_holder.HideMyHand(true);

			if (GameManager.Instance.AmIWinning())
				_hud.OpenWin();
			else
				_hud.OpenLoose();
		}
	}
}