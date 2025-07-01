using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoomAbility : Ability
	{
		private float _startdist = 0;
		private float _startZoom = 0;

		private bool _inZoom;

		private Camera _cam;
		private ZoneHolderResource _zoneHolder;

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
		}

		public void ZoomInProcess(Vector2 posTouch1, Vector2 posTouch2)
		{
			if (!_inZoom) return;

			_zoneHolder.UpdatePlacementInHand();
		}

		public void StopZoom()
		{
			_inZoom = false;
			_startdist = 0;
			_startZoom = 0;
		}
	}
}