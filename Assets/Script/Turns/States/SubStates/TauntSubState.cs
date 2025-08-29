using CardGame.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class TauntSubState : State
	{
		private TauntShakeTileAbility _tauntShakeTile;
		private HUDResource _hudResource;
		private MoveTileAbility _moveTile;
		private TauntButtonAbility _tauntButton;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _tauntShakeTile);
			GetStateComponent(ref _hudResource);
			GetStateComponent(ref _moveTile);
			GetStateComponent(ref _tauntButton);
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name != "Touch")
				return;

			if (context.phase == InputActionPhase.Performed)
			{
				Vector2 touchPos = Controller.GetActionValue<Vector2>("TouchPos");

				if (_hudResource.AmIClickingOnUI(touchPos) 
					|| _moveTile.QuickCheckRay(touchPos))
					return;

				_tauntShakeTile.ShakeTile(Controller.GetActionValue<Vector2>("TouchPos"));
			}
		}
	}
}