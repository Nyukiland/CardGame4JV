using UnityEngine;

namespace CardGame.StateMachine
{
	public class AbilityToMoveCard : Ability
	{
		[SerializeField]
		private float t;

		public override string DisplayInfo()
		{
			string text = $"-- {nameof(AbilityToMoveCard)} -- \n" +
				$"- {t.ToString()}";
			return text;
		}
	}
}