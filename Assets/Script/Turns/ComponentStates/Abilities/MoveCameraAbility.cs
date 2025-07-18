using CardGame.StateMachine;
using CardGame.UI;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCameraAbility : Ability
	{
		[SerializeField]
		private float _moveFactor;

		[SerializeField]
		private float _limitCamMove;

		private Vector2 _startPos;
		private Vector3 _camPos;
		private Camera _cam;

		private Vector3 _topRightCorner = new(0, 0, 0);
		private Vector3 _bottomLeftCorner = new(100, 100, 0);

		private bool _inUse;

		private GridManagerResource _gridManager;
		private ZoneHolderResource _zoneHolder;

		public bool InUse => _inUse;

		public override void LateInit()
		{
			base.LateInit();
			_cam = Camera.main;
			_gridManager = Owner.GetStateComponent<GridManagerResource>();
			_zoneHolder = Owner.GetStateComponent<ZoneHolderResource>();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			StopMoving();
		}

		public void StartMoving(Vector2 pos)
		{
			_inUse = true;
			_startPos = pos;
			_camPos = _cam.transform.position;

			for (int i = 0; i < _gridManager.Width; i++)
			{
				for (int j = 0; j < _gridManager.Height; j++)
				{
					TileVisu tile = _gridManager.GetTile(i, j);

					if (tile.TileData == null) continue;

					Vector3 tilePos = tile.transform.position;

					_bottomLeftCorner.x = Mathf.Min(_bottomLeftCorner.x, tilePos.x);
					_bottomLeftCorner.y = Mathf.Min(_bottomLeftCorner.y, tilePos.y);
					_topRightCorner.x = Mathf.Max(_topRightCorner.x, tilePos.x);
					_topRightCorner.y = Mathf.Max(_topRightCorner.y, tilePos.y);
				}
			}

			_bottomLeftCorner -= new Vector3(_limitCamMove, _limitCamMove, 0);
			_topRightCorner += new Vector3(_limitCamMove, _limitCamMove, 0);

			_zoneHolder.HideMyHand(true);
		}

		public void MoveCamera(Vector2 pos)
		{
			if (!_inUse) return;

			Vector3 move = _cam.ScreenToWorldPoint(_startPos) - _cam.ScreenToWorldPoint(pos);
			Vector3 targetPos = _camPos + move * _moveFactor;

			Vector3 camShift = _cam.transform.position - targetPos;

			// Shifted world positions of corners
			Vector3 shiftedBL = _bottomLeftCorner + camShift;
			Vector3 shiftedTR = _topRightCorner + camShift;

			// Viewport space
			Vector3 viewBL = _cam.WorldToViewportPoint(shiftedBL);
			Vector3 viewTR = _cam.WorldToViewportPoint(shiftedTR);

			// Check if either corner is still visible
			bool topRightVisible = viewTR.x >= 0.01f && viewTR.x <= 0.99f &&
								   viewTR.y >= 0.01f && viewTR.y <= 0.99f;

			bool bottomLeftVisible = viewBL.x >= 0.01f && viewBL.x <= 0.99f &&
									 viewBL.y >= 0.01f && viewBL.y <= 0.99f;

			if (topRightVisible || bottomLeftVisible)
			{
				_cam.transform.DOMove(targetPos, 0.1f);
			}
		}


		public void StopMoving()
		{
			_zoneHolder.HideMyHand(false);
			_inUse = false;
			_startPos = Vector2.zero;
			_camPos = Vector3.zero;

			_topRightCorner = new(0, 0, 0);
			_bottomLeftCorner = new(100, 100, 0);
		}
	}
}