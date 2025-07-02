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

		public override void LateInit()
		{
			base.OnEnable();

			for (int i = 0; i < _taunts.Count; i++)
			{
				_taunts[i].Button.onClick.AddListener(() => CallEvent(_taunts[i].Taunt));
			}
		}

		public void CallEvent(TauntScriptableObject tauntAction)
		{
			if (!string.IsNullOrEmpty(tauntAction.FmodEvent))
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
		private class ButtonTaunt
		{
			public Button Button;
			public TauntScriptableObject Taunt;
		}
	}
}