using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCardAbility : Ability
	{
		private ZoneHolderResource _cardManager;
		private CardData _currentCardData;
		private CardContainer _previousCardContainer;
		private int _previousCardIndex;
		
		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cardManager = owner.GetStateComponent<ZoneHolderResource>();
		}
		
		public void PickCard(Vector2 position)
		{
			if (!_cardManager.ContainsContainer(position, out CardContainer container))
				return;
			
			if (!container.ContainsCard(position, out CardData cardData))
				return;
			
			_currentCardData = cardData;
			_previousCardContainer = container;
			_previousCardIndex = container.RemoveCard(_currentCardData);
			_currentCardData.CardUI.ChangeParent(_cardManager.MainCanvas.transform);
		}
		
		public void MoveCard(Vector2 position)
		{
			if (_currentCardData == null)
				return;
			
			_currentCardData.CardUI.transform.position = position;
		}

		public void ReleaseCard(Vector2 position)
		{
			if (_currentCardData == null)
				return;

			if (!_cardManager.ContainsContainer(position, out CardContainer container))
			{
				SendCardBack();
				return;
			}

			if (!container.GetMouseBetweenIndexes(position, _cardManager.MainCanvas, out int listIndex))
			{
				SendCardBack();
				return;
			}

			container.AddCard(_currentCardData, listIndex);
			_currentCardData.CardUI.ChangeParent(container.transform);
			_currentCardData.CardUI.transform.SetSiblingIndex(listIndex);
			_previousCardContainer = null;
			_currentCardData = null;
		}

		private void SendCardBack()
		{
			_previousCardContainer.AddCard(_currentCardData, _previousCardIndex);
			_currentCardData.CardUI.ChangeParent(_previousCardContainer.transform);
			_currentCardData.CardUI.transform.SetSiblingIndex(_previousCardIndex);
			_previousCardContainer = null;
			_previousCardIndex = -1;
			_currentCardData = null;
		}
	}
}