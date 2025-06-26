using CardGame.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class TauntSubState : State
	{
		private TauntShakeTileAbility _tauntShakeTile;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _tauntShakeTile);
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name != "Touch")
				return;

			if (context.phase == InputActionPhase.Performed)
			{
				_tauntShakeTile.ShakeTile(Controller.GetActionValue<Vector2>("TouchPos"));
			}
		}
	}
}