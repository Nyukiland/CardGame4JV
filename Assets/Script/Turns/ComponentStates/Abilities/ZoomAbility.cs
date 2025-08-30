using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoomAbility : Ability
	{
		[SerializeField] private float _minZoom = 2f;
		private float _maxZoom = 10f;

		private float _startdist = 0;
		private float _startZoom = 0;

		private bool _inZoom;

		private Camera _cam;
		private ZoneHolderResource _zoneHolder;
		private GridManagerResource _gridManager;

		public bool InZoom => _inZoom;

		public bool _manualZoomDoOnce;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cam = Camera.main;
			_maxZoom = _cam.orthographicSize;

			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
		}

		public void StartZoom(Vector2 posTouch1, Vector2 posTouch2)
		{
			_inZoom = true;
			_startdist = Vector2.Distance(posTouch1, posTouch2);
			_startZoom = _cam.orthographicSize;

			float width = _gridManager.TopRightMostTile.x - _gridManager.BottomLeftMostTile.x;
			float height = _gridManager.TopRightMostTile.y - _gridManager.BottomLeftMostTile.y;
			_maxZoom = Mathf.Max(Mathf.Max(width, height) * 0.5f + 1f, _maxZoom); // +1f c'est la marge
			_zoneHolder.HideMyHand(true);
		}

		public void ZoomInProcess(Vector2 posTouch1, Vector2 posTouch2)
		{
			if (!_inZoom) return;

			float currentDist = Vector2.Distance(posTouch1, posTouch2);
			float zoomFactor = _startdist / currentDist;

			_cam.orthographicSize = Mathf.Clamp(_startZoom * zoomFactor, _minZoom, _maxZoom);

			_zoneHolder.UpdatePlacementInHand(true);
		}

		public void ZoomInProcess(float value)
		{
			if (value == 0 && _manualZoomDoOnce)
			{
				_manualZoomDoOnce = false;
				_zoneHolder.HideMyHand(false);
			}
			else if (value != 0 && !_manualZoomDoOnce)
			{
				_manualZoomDoOnce = true;
				_zoneHolder.HideMyHand(true);
			}

			_cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + (value), _minZoom, _maxZoom);

			_zoneHolder.UpdatePlacementInHand(true);
			_zoneHolder.UpdateTileInHandSize();
		}

		public void StopZoom()
		{
			_inZoom = false;
			_startdist = 0;
			_startZoom = 0;

			_zoneHolder.UpdateTileInHandSize();
			_zoneHolder.UpdatePlacementInHand(true);
			_zoneHolder.HideMyHand(false);
		}
	}
}