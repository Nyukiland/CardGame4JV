using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoomAbility : Ability
	{
		[SerializeField]
		private float _minZoom = 2f;

		private float _startdist = 0;
		private float _startZoom = 0;

		private bool _inZoom;

		private Camera _cam;
		private ZoneHolderResource _zoneHolder;

		private const float MaxZoom = 10f;

		public bool InZoom => _inZoom;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cam = Camera.main;

			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
		}

		public void StartZoom(Vector2 posTouch1, Vector2 posTouch2)
		{
			_inZoom = true;
			_startdist = Vector2.Distance(posTouch1, posTouch2);
			_startZoom = _cam.orthographicSize;

			_zoneHolder.HideMyHand(true);
		}

		public void ZoomInProcess(Vector2 posTouch1, Vector2 posTouch2)
		{
			if (!_inZoom) return;

			float currentDist = Vector2.Distance(posTouch1, posTouch2);
			float zoomFactor = _startdist / currentDist;

			_cam.orthographicSize = Mathf.Clamp(_startZoom * zoomFactor, _minZoom, MaxZoom);

			_zoneHolder.UpdatePlacementInHand(true);
		}

		public void StopZoom()
		{
			_inZoom = false;
			_startdist = 0;
			_startZoom = 0;

			_zoneHolder.HideMyHand(false);
		}
	}
}