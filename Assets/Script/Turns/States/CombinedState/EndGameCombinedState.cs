using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class EndGameCombinedState : CombinedState
	{
		private HUDResource _hud;

		public override void OnEnter()
		{
			base.OnEnter();

			GetStateComponent(ref _hud);

			if (GameManager.Instance.AmIWinning())
				_hud.OpenWin();
			else
				_hud.OpenLoose();
		}
	}
}