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
		public RectTransform RectTransform { get; set; }

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			
			foreach (CardData cardData in _tempCardData)
			{
				_cards.Add(cardData.CreateCardUI(transform));
			}
		}

		public bool ContainsCard(Vector3 position, out CardData cardData)
		{
			cardData = null;

			if (_cards.Count >= _maxCard)
				return false;
			
			foreach (CardUI currentCardUI in _cards)
			{
				if (currentCardUI == null)
					continue;

				if (!RectTransformUtility.RectangleContainsScreenPoint(currentCardUI.CardRectTransform, position))
					continue;
				
				cardData = currentCardUI.CardData;
				return true;
			}
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
