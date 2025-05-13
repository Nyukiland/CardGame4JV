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

		// Are u sure it's the correct name ?
		// I would expect it to return a StateComponent considering the name
		// Blanco
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
	}
}