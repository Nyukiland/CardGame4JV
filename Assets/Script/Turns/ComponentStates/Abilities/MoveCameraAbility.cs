using CardGame.StateMachine;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCameraAbility : Ability
	{
		[SerializeField]
		private float _moveFactorX = 1;

		[SerializeField]
		private float _moveFactorY = 2;

		[SerializeField]
		private Vector2 _limitCamMove = new(1, 10);

		[SerializeField]
		private float _maxDistanceConsidered = 10f;

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

			Vector3 dir = _cam.ScreenToWorldPoint(_startPos) - _cam.ScreenToWorldPoint(pos);
			dir.x *= _moveFactorX;
			dir.y *= _moveFactorY;
			dir.z = 0;

			Vector3 gridCenter = new(_gridManager.Center.x, _gridManager.Center.y - 10, 0);

			float distBL = Vector3.Distance(gridCenter, _gridManager.BottomLeftMostTile);
			float distTR = Vector3.Distance(gridCenter, _gridManager.TopRightMostTile);
			float furthestDist = Mathf.Max(distBL, distTR);

			float t = Mathf.Clamp01(furthestDist / _maxDistanceConsidered);

			Vector3 tempPos = _camPos + dir;
			tempPos.z = 0;
			dir = tempPos - gridCenter;

			dir = Vector3.ClampMagnitude(dir, Mathf.Lerp(_limitCamMove.x, _limitCamMove.y, t));

			Vector3 targetPos = gridCenter + dir;
			targetPos.z = _cam.transform.position.z;

			_cam.transform.DOMove(targetPos, 0.1f);
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