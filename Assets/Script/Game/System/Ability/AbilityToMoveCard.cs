using CardGame.Card;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.StateMachine
{
	public class AbilityToMoveCard : Ability
	{
		[Header("SetUp")]

		[SerializeField]
		private float _speed;

		private CardInfo _card;
		private CardContainer _prevContainer;

		private CardManagementResource _cardManager;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cardManager = owner.GetStateComponent<CardManagementResource>();
		}

		public void PickCard(Vector3 pos)
		{
			List<CardInfo> cards = new();
			cards.AddRange(_cardManager.InHandCardContainer.GetComponentsInChildren<CardInfo>());
			cards.AddRange(_cardManager.PlayedCardContainer.GetComponentsInChildren<CardInfo>());

			foreach (CardInfo card in cards)
			{
				if (card.ContainsPos(pos))
				{
					_card = card;
					break;
				}
			}

			if (_card == null) return;

			_prevContainer = _card.GetComponentInParent<CardContainer>();
			_card.transform.parent = _prevContainer.GetComponentInParent<Canvas>().transform;
			_cardManager.UpdateAllCardContainerPos();
		}

		public void MoveCard(Vector3 pos)
		{
			if (_card == null) return;

			_card.MoveCard(pos, _speed);
		}

		public void ReleaseCard()
		{
			if (_card == null) return;

			if (_cardManager.InHandCardContainer.IsInRect(_card.transform.position))
			{
				_card.transform.parent = _cardManager.InHandCardContainer.transform;
			}
			else if (_cardManager.PlayedCardContainer.IsInRect(_card.transform.position))
			{
				_card.transform.parent = _cardManager.PlayedCardContainer.transform;
			}
			else
			{
				_card.transform.parent = _prevContainer.transform;
			}

			_cardManager.UpdateAllCardContainerPos();
		}

		public override string DisplayInfo()
		{
			string text = $"-- {nameof(AbilityToMoveCard)} -- \n" +
				$"- SelectedCard: {_card.name} \n" +
				$"	PrevContainer: {_prevContainer}";
			return text;
		}
	}
}