using CardGame.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Turns
{
	public class TauntButtonAbility : Ability
	{
		[SerializeField]
		private List<TauntScriptableObject> _taunts = new();

		[SerializeField]
		private List<TauntScriptableObject> _extraTaunts = new();

		public List<TauntScriptableObject> Taunts => _taunts;

		private SendInfoAbility _sendInfo;
		private HUDResource _hud;
		private TauntShakeTileAbility _shakeTile;

		private bool _readyUnlock = false;
		private bool _unlocked = false;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_sendInfo = owner.GetStateComponent<SendInfoAbility>();
			_hud = owner.GetStateComponent<HUDResource>();
			_shakeTile = owner.GetStateComponent<TauntShakeTileAbility>();
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

			foreach (TauntScriptableObject tauntB in _extraTaunts)
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
			if (_shakeTile.MiddleTileCounter == 3 && tauntAction.Text == "Tu devrais la fermer")
			{
				_readyUnlock = true;
			}
			else _readyUnlock = false;

			if (!tauntAction.FmodEvent.IsNull)
			{
				FMODUnity.RuntimeManager.PlayOneShot(tauntAction.FmodEvent);
			}

			if (tauntAction.Anim.Length != 0)
				_hud.SendTaunt(tauntAction.Anim, tauntAction.WaitTime, self);
			else
				_hud.SendTaunt(tauntAction.Text, self);


			if (self)
				_sendInfo.SendTaunt(tauntAction.Text);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_unlocked)
				return;

			if (!_readyUnlock)
				return;

			if (_shakeTile.MiddleTileCounter < 6)
				return;

			_unlocked = true;
			_taunts.AddRange(_extraTaunts);
			_hud.AddTaunt(_extraTaunts);
		}
	}
}