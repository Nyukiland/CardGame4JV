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

		[SerializeField, Min(0)]
		private float _limitCamMove;

		private Vector2 _startPos;
		private Vector3 _camPos;
		private Camera _cam;

		private Vector3 _topRightCorner;
		private Vector3 _bottomLeftCorner;

		private bool _inUse;

		private GridManagerResource _gridManager;

		public override void LateInit()
		{
			base.LateInit();
			_cam = Camera.main;
			_gridManager = Owner.GetStateComponent<GridManagerResource>();
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

					_bottomLeftCorner.x = tilePos.x < _bottomLeftCorner.x ? tilePos.x : _bottomLeftCorner.x;
					_bottomLeftCorner.y = tilePos.y < _bottomLeftCorner.y ? tilePos.y : _bottomLeftCorner.y;

					_bottomLeftCorner.x = tilePos.x > _topRightCorner.x ? tilePos.x : _topRightCorner.x;
					_bottomLeftCorner.y = tilePos.y > _topRightCorner.y ? tilePos.y : _topRightCorner.y;
				}
			}

			_bottomLeftCorner += Vector3.one;
			_topRightCorner -= Vector3.one;
		}

		public void MoveCamera(Vector2 pos)
		{
			if (!_inUse) return;

			Vector3 move = _cam.ScreenToWorldPoint(_startPos) - _cam.ScreenToWorldPoint(pos);

			Vector2 viewport = _cam.WorldToViewportPoint(_bottomLeftCorner + move);
			if (viewport.x > 1 || viewport.x < 0 ||
				viewport.y > 1 || viewport.x < 0)
				return;

			_cam.transform.DOMove(_camPos + (move * _moveFactor), 0.1f);
		}

		public void StopMoving()
		{
			_inUse = false;
			_startPos = Vector2.zero;
			_camPos = Vector3.zero;
		}
	}
}