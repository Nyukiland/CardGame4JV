using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace CardGame.StateMachine
{
	public class SelectCardSubState : State
	{
		private AbilityToMoveCard _moveCard;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _moveCard);
		}
	}
}