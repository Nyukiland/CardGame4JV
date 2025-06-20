using UnityEngine.InputSystem;

namespace CardGame.StateMachine
{
	public abstract class State
	{
		private Controller _controller;
		public virtual Controller Controller
		{
			get { return _controller; }
			set { _controller = value; }
		}

		protected void GetStateComponent<T>(ref T component, bool enable = true) where T : StateComponent
		{
			component ??= _controller.GetStateComponent<T>();
			if (enable)
				component?.OnEnableController();
		}

		public virtual void OnEnter() { }

		public virtual void OnExit() { }

		public virtual void OnLateInit() { }

		public virtual void Update(float deltaTime) { }

		public virtual void FixedUpdate(float fixedDeltaTime) { }

		public virtual void OnActionTriggered(InputAction.CallbackContext context) { }

		public string DisplayInfoController()
		{
			string text = "\n -----" + this.GetType().Name;
			text += DisplayInfo();

			//still don't like the cast
			if (this is CombinedState combined)
				text += combined.GetSubstateDisplayInfo();

			return text;
		}

		public virtual string DisplayInfo()
		{
			return string.Empty;
		}
	}
}