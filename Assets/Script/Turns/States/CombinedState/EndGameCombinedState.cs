using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class EndGameCombinedState : CombinedState
	{
		public override void OnEnter()
		{
			base.OnEnter();

			UnityEngine.Debug.Log($"Fini \n Score: {GameManager.Instance.PlayerScore}");
		}
	}
}