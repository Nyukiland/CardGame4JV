using UnityEngine.InputSystem;
using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class MoveCardSubState : State
	{
		private MoveCardAbility _moveCardAbility;

		private bool _touch;
		private Vector3 _touchPos;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _moveCardAbility);
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name == "Touch")
			{
				if (context.phase == InputActionPhase.Performed)
				{
					_touchPos = Controller.GetActionValue<Vector2>("TouchPos");
					_moveCardAbility.PickCard(_touchPos);
					_touch = true;
				}
				else if (context.phase == InputActionPhase.Canceled)
				{
					_moveCardAbility.ReleaseCard(_touchPos);
					_touchPos = Vector2.zero;
					_touch = false;
				}
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (!_touch)
				return;

			_touchPos = Controller.GetActionValue<Vector2>("TouchPos");
			_moveCardAbility.MoveCard(_touchPos);
		}
	}
}