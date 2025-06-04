using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		private ZoneHolderResource _cardManager;
		private TileUI _currentTile;
		private TileContainer _previousCardContainer;
		private int _previousCardIndex;
		
		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cardManager = owner.GetStateComponent<ZoneHolderResource>();
		}
		
		public void PickCard(Vector2 position)
		{
			if (!_cardManager.ContainsContainer(position, out TileContainer container))
				return;
			
			if (!container.ContainsTile(position, out TileUI tile))
				return;

            _currentTile = tile;
			_previousCardContainer = container;
			_previousCardIndex = container.RemoveTile(_currentTile);
            _currentTile.ChangeParent(_cardManager.MainCanvas.transform);
		}
		
		public void MoveCard(Vector2 position)
		{
			if (_currentTile == null)
				return;

            _currentTile.transform.position = position;
		}

		public void ReleaseCard(Vector2 position)
		{
			if (_currentTile == null)
				return;

			if (!_cardManager.ContainsContainer(position, out TileContainer container))
			{
				SendCardBack();
				return;
			}

			if (!container.GetMouseBetweenIndexes(position, _cardManager.MainCanvas, out int listIndex))
			{
				SendCardBack();
				return;
			}

			container.AddTile(_currentTile, listIndex);
            _currentTile.ChangeParent(container.transform);
            _currentTile.transform.SetSiblingIndex(listIndex);
			_previousCardContainer = null;
            _currentTile = null;
		}

		private void SendCardBack()
		{
			_previousCardContainer.AddTile(_currentTile, _previousCardIndex);
            _currentTile.ChangeParent(_previousCardContainer.transform);
            _currentTile.transform.SetSiblingIndex(_previousCardIndex);
			_previousCardContainer = null;
			_previousCardIndex = -1;
            _currentTile = null;
		}
	}
}