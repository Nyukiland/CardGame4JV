using CardGame.StateMachine;
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

			_zoneHolder.HideMyHand(true);
		}

		public void MoveCamera(Vector2 pos)
		{
			if (!_inUse) return;

			Vector3 move = _cam.ScreenToWorldPoint(_startPos) - _cam.ScreenToWorldPoint(pos);
			Vector3 targetPos = _camPos + move * _moveFactor;

			Vector3 camShift = _cam.transform.position - targetPos;

			// Shifted world positions of corners
			Vector3 shiftedBL = _gridManager.BottomLeftMostTile + camShift;
			Vector3 shiftedTR = _gridManager.TopRightMostTile + camShift;

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
		}
	}
}