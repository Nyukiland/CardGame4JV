using System.Collections.Generic;
using CardGame.Card;
using UnityEngine;

namespace CardGame.UI
{
	public class CardContainer : MonoBehaviour
	{
		[SerializeField, Min(0)] private int _maxCard;
		public int MaxCard => _maxCard;

		private List<CardUI> _cards = new();
		[SerializeField] private List<CardData> _tempCardData;

		private void Start()
		{
			foreach (CardData cardData in _tempCardData)
			{
				_cards.Add(cardData.CreateCardUI(transform));
			}
		}

		public bool ContainsCard(Vector3 position, out CardData cardData)
		{
			foreach (CardUI currentCardUI in _cards)
			{
				if (currentCardUI == null)
					continue;
				
				if (RectTransformUtility.RectangleContainsScreenPoint(currentCardUI.CardRectTransform, position))
				{
					cardData = currentCardUI.CardData;
					return true;
				}
			}

			cardData = null;
			return false;
		}

		public void RemoveCard(CardData cardData)
		{
			_cards.Remove(cardData.CardUI);
		}

		public void AddCard(CardData cardData)
		{
			_cards.Add(cardData.CardUI);
		}
	}
}
