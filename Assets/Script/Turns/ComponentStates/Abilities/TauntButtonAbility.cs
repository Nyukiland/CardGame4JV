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
		private List<TauntScriptableObject> _taunts = new();
		public List<TauntScriptableObject> Taunts => _taunts;

		private SendInfoAbility _sendInfo;
		private HUDResource _hud;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_sendInfo = owner.GetStateComponent<SendInfoAbility>();
			_hud = owner.GetStateComponent<HUDResource>();
		}

		public void CallEvent(string name)
		{
			foreach (TauntScriptableObject tauntB in _taunts)
			{
				if (name == tauntB.Text)
				{
					CallEvent(tauntB, false);
					return;
				}
			}
		}

		public void CallEvent(TauntScriptableObject tauntAction, bool self = true)
		{
			if (!tauntAction.FmodEvent.IsNull)
			{
				FMODUnity.RuntimeManager.PlayOneShot(tauntAction.FmodEvent);
			}

			if (tauntAction.Anim.Count != 0)
			{
				PlayTauntAnim(tauntAction.Anim.ToArray(), tauntAction.WaitTime).Forget();
			}

			if (self)
				_sendInfo.SendTaunt(tauntAction.name);

			_hud.SendTaunt(tauntAction.Text, self);
		}

		private async UniTask PlayTauntAnim(Image[] anim, float waitTime)
		{
			foreach (Image frame in anim)
			{
				//frame;

				await UniTask.WaitForSeconds(waitTime);
			}
		}
	}
}