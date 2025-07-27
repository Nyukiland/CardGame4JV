using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace CardGame.Turns
{
	public class TauntButtonAbility : Ability
	{
		[SerializeField]
		private List<ButtonTaunt> _taunts = new();
		public List<ButtonTaunt> Taunts => _taunts;

		public void CallEvent(TauntScriptableObject tauntAction)
		{
			 if (!tauntAction.FmodEvent.IsNull)
			 {
			 	FMODUnity.RuntimeManager.PlayOneShot(tauntAction.FmodEvent);
			 }

			if (tauntAction.Anim.Count != 0)
			{
				PlayTauntAnim(tauntAction.Anim.ToArray(), tauntAction.WaitTime).Forget();
			}
		}

		private async UniTask PlayTauntAnim(Image[] anim, float waitTime)
		{
			foreach(Image frame in anim)
			{
				//frame;

				await UniTask.WaitForSeconds(waitTime);
			}
		}

		[Serializable]
		public class ButtonTaunt
		{
			public Button Button { get; set; }
			public TauntScriptableObject Taunt;
		}
	}
}