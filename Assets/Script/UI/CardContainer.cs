using System.Collections.Generic;
using CardGame.Card;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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

		public int RemoveCard(CardData cardData)
		{
			int index = _cards.IndexOf(cardData.CardUI);
			_cards.Remove(cardData.CardUI);
			return index;
		}

		public void AddCard(CardData cardData, int index)
		{
			_cards.Insert(index, cardData.CardUI);
		}
		
		public bool GetMouseBetweenIndexes(Vector2 mousePosition, Canvas mainCanvas, out int listIndex)
		{
			listIndex = 0;

			if (_cards.Count >= _maxCard)
				return false;
			
			if (_cards.Count <= 0)
				return true;
			
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				mainCanvas.transform as RectTransform, mousePosition, mainCanvas.worldCamera, out Vector2 localPoint);
			
			List<float> positionsX = new();
			foreach (CardUI cardUI in _cards)
			{
				positionsX.Add(cardUI.CardRectTransform.localPosition.x);
			}

			// Left of all cards
			if (localPoint.x < positionsX[0])
			{
				listIndex = 0;
				return true;
			}

			// Right of all cards
			if (localPoint.x > positionsX[^1])
			{
				listIndex = positionsX.Count;
				return true;
			}

			// Between two cards
			for (int i = 0; i < positionsX.Count - 1; i++)
			{
				if (!(localPoint.x >= positionsX[i]) || !(localPoint.x <= positionsX[i + 1]))
					continue;
				
				listIndex = i + 1;
				return true;
			}

			return false;
		}
	}
}
