using System.Collections.Generic;

namespace CardGame.StateMachine
{
	public class CombinedState : State
	{
		private List<State> _subStates;

		public override Controller Controller
		{
			get { return base.Controller; }
			set
			{
				foreach (State subState in _subStates)
				{
					subState.Controller = value;
				}
				base.Controller = value;
			}
		}

		public CombinedState()
		{
			_subStates = new List<State>();
		}

		protected void AddSubState(State subState)
		{
			_subStates.Add(subState);
		}

		public override void OnEnter()
		{
			base.OnEnter();
			foreach (State subState in _subStates)
			{
				subState.OnEnter();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			foreach (State subState in _subStates)
			{
				subState.OnExit();
			}
		}

		public override void OnLateInit()
		{
			base.OnLateInit();
			foreach (State subState in _subStates)
			{
				subState.OnLateInit();
			}
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			foreach (State subState in _subStates)
			{
				subState.Update(deltaTime);
			}
		}

		public override void FixedUpdate(float fixedDeltaTime)
		{
			base.FixedUpdate(fixedDeltaTime);
			foreach (State subState in _subStates)
			{
				subState.FixedUpdate(fixedDeltaTime);
			}
		}

		public string GetSubstateDisplayInfo()
		{
			string t = string.Empty;

			foreach (State subState in _subStates)
			{
				t += "\n" + subState.DisplayInfoController();
			}

			return t;
		}
	}
}