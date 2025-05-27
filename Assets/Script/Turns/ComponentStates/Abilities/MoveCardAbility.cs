using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCardAbility : Ability
	{
		private ZoneHolderResource _cardManager;
		private CardData _currentCardData;
		private CardContainer _previousCardContainer;
		
		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cardManager = owner.GetStateComponent<ZoneHolderResource>();
		}
		
		public void PickCard(Vector2 position)
		{
			if (!_cardManager.ContainsContainer(position, out CardContainer container))
				return;

			_previousCardContainer = container;
			
			if (!container.ContainsCard(position, out CardData cardData))
				return;
			
			_currentCardData = cardData;
			container.RemoveCard(_currentCardData);
			_currentCardData.CardUI.ChangeParent(_cardManager.MainCanvas);
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
				_previousCardContainer.AddCard(_currentCardData);
				_currentCardData.CardUI.ChangeParent(_previousCardContainer.transform);
				_previousCardContainer = null;
				_currentCardData = null;
				return;
			}
			
			container.AddCard(_currentCardData);
			_currentCardData.CardUI.ChangeParent(container.transform);
			_previousCardContainer = null;
			_currentCardData = null;
		}
	}
}