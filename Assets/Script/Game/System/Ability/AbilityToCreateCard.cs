using CardGame.Card;
using UnityEngine;
using System.Collections.Generic;

namespace CardGame.StateMachine
{
	public class AbilityToCreateCard : Ability
	{
		[SerializeField]
		private GameObject _prefabCard;

		private CardManagementResource _manager;

		private List<CardIntrepeter> _cards;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_manager = owner.GetStateComponent<CardManagementResource>();
		}

		public void GenerateCards()
		{
			_cards.Clear();

			int cardNumber = _manager.InHandCardContainer.MaxCard;

			for (int i = 0; i < cardNumber; i++)
			{
				CardIntrepeter card = GameObject.Instantiate(_prefabCard).GetComponent<CardIntrepeter>();
				card.SetUp(_manager.GetRandomCard());
				card.transform.position = new Vector3(-1000, -1000, 0);
				card.transform.parent = _manager.InHandCardContainer.transform;
				_cards.Add(card);
			}

			_manager.InHandCardContainer.GiveCardPosition();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_cards.Count == 0) return;

			for (int i = _cards.Count - 1; i >= 0; i--)
			{
				if (_cards[i].IsAtDestination()) _cards.RemoveAt(i);
			}
		}

		public bool IsFinished()
		{
			return _cards.Count == 0;
		}
	}
}