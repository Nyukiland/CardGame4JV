using CardGame.StateMachine;
using CardGame.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		[SerializeField]
		private VerticalLayoutGroup _discardVerticalLayout;

		[SerializeField]
		private Transform _discardIn;

		[SerializeField]
		private Transform _discardOut;

		[SerializeField]
		private GameObject _discardIcone;

		private MoveTileAbility _moveTile;
		private SendInfoAbility _sendInfo;
		private ZoneHolderResource _holderResource;
		private NetworkResource _networkResource;

		private bool _isIn;
		private List<GameObject> _discardItems = new();

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_moveTile = owner.GetStateComponent<MoveTileAbility>();
			_sendInfo = owner.GetStateComponent<SendInfoAbility>();
			_holderResource = owner.GetStateComponent<ZoneHolderResource>();
			_networkResource = owner.GetStateComponent<NetworkResource>();

			ShowDiscardArea(false);
			_discardArea.transform.position = _discardOut.position;
		}

		public override void OnDisable()
		{
			base.OnDisable();

			ShowDiscardArea(false);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			_discardArea.gameObject.SetActive(!_holderResource.IsHidden || _moveTile.CurrentTile != null);
		}

		public void ShowDiscardArea(bool display)
		{
			if (_isIn == display)
				return;

			_isIn = display;
			_discardArea.transform.DOKill();

			if (_isIn)
			{
				for (int i = 0; i < _holderResource.TileInHandCount - _maxTileInHand; i++)
					_discardItems.Add(GameObject.Instantiate(_discardIcone, _discardVerticalLayout.transform));
			}

			Vector3 posToGo = display ? _discardIn.position : _discardOut.position;
			posToGo = _discardArea.parent.InverseTransformPoint(posToGo);

			_discardArea.transform.DOLocalMove(posToGo, 1f).SetEase(Ease.InOutSine);
		}

		public void ReleaseCard(Vector2 pos)
		{
			if (_moveTile.CurrentTile == null)
				return;

			TileVisu tile = _moveTile.CurrentTile;
			_moveTile.CurrentTile = null;

			if (!RectTransformUtility.RectangleContainsScreenPoint(_discardArea, pos, Camera.main))
			{
				_holderResource.GiveTileToHand(tile.gameObject);
				return;
			}

			int tileId = tile.TileData.TileSettings.IdCode;

			if (!_sendInfo.SendDiscardTile(tileId))
			{
				_drawPile.DiscardTile(tile.TileData.TileSettings.IdCode);
			}

			GameObject temp = _discardItems[0];
			_discardItems.RemoveAt(0);
			GameObject.Destroy(temp);

			GameObject.Destroy(tile.gameObject);
		}

		public bool DiscardFinished()
		{
			return _holderResource.TileInHandCount <= _maxTileInHand && _moveTile.CurrentTile == null;
		}
	}
}