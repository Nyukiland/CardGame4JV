using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class DiscardCardAbility : Ability
	{
		[SerializeField]
		private int _maxTileInHand = 7;

		[SerializeField]
		private DrawPile _drawPile;

		[SerializeField]
		private RectTransform _discardArea;

		private MoveTileAbility _moveTile;
		private ZoneHolderResource _holderResource;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_moveTile = owner.GetStateComponent<MoveTileAbility>();
			_holderResource = owner.GetStateComponent<ZoneHolderResource>();

			_discardArea.gameObject.SetActive(false);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			_discardArea.gameObject.SetActive(true);
		}

		public override void OnDisable()
		{
			base.OnDisable();

			_discardArea.gameObject.SetActive(false);
		}

		public void ReleaseCard(Vector2 pos)
		{
			if (_moveTile.CurrentTile == null)
				return;

			TileVisu tile = _moveTile.CurrentTile;
			_moveTile.CurrentTile = null;

			if (!RectTransformUtility.RectangleContainsScreenPoint(_discardArea, pos))
			{
				_holderResource.GiveTileToHand(tile.gameObject);
				return;
			}

			_drawPile.DiscardTile(tile.TileData.TileSettings);
			GameObject.Destroy(tile.gameObject);
		}

		public bool DiscardFinished()
		{
			return _holderResource.TileInHandCount <= _maxTileInHand && _moveTile.CurrentTile == null;
		}
	}
}