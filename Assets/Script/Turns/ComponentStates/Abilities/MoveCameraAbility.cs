using CardGame.StateMachine;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCameraAbility : Ability
	{
		[SerializeField]
		private float _moveFactor;

		private Vector2 _startPos;
		private Vector3 _camPos;
		private Camera _cam;

		private bool _inUse;

		public override void LateInit()
		{
			base.LateInit();
			_cam = Camera.main;
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
		}

		public void MoveCamera(Vector2 pos)
		{
			if (!_inUse) return;

			Vector3 move = _startPos - pos;

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