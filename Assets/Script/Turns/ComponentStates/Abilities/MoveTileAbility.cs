using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		private ZoneHolderResource _cardManager;
		private TileSettings _currentTileSettings;
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
			
			if (!container.ContainsTile(position, out TileSettings tileSettings))
				return;

            _currentTileSettings = tileSettings;
			_previousCardContainer = container;
			_previousCardIndex = container.RemoveTile(_currentTileSettings);
            _currentTileSettings.TileUI.ChangeParent(_cardManager.MainCanvas.transform);
		}
		
		public void MoveCard(Vector2 position)
		{
			if (_currentTileSettings == null)
				return;

            _currentTileSettings.TileUI.transform.position = position;
		}

		public void ReleaseCard(Vector2 position)
		{
			if (_currentTileSettings == null)
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

			container.AddTile(_currentTileSettings, listIndex);
            _currentTileSettings.TileUI.ChangeParent(container.transform);
            _currentTileSettings.TileUI.transform.SetSiblingIndex(listIndex);
			_previousCardContainer = null;
            _currentTileSettings = null;
		}

		private void SendCardBack()
		{
			_previousCardContainer.AddTile(_currentTileSettings, _previousCardIndex);
            _currentTileSettings.TileUI.ChangeParent(_previousCardContainer.transform);
            _currentTileSettings.TileUI.transform.SetSiblingIndex(_previousCardIndex);
			_previousCardContainer = null;
			_previousCardIndex = -1;
            _currentTileSettings = null;
		}
	}
}