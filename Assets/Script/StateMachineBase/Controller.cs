using CardGame.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.StateMachine
{
	public class Controller : MonoBehaviour, ISelectableInfo
	{
		[SerializeField, SerializeReference, SubclassSelector(typeof(StateComponent))]
		private List<StateComponent> _components = new();

		protected virtual void Awake()
		{
			_components.ForEach(comp => comp.EarlyInit());
			_components.ForEach(comp => comp.Init(owner: this));
		}

		protected virtual void Start()
		{
			_components.ForEach(comp => comp.LateInit());
		}

		private void OnEnable()
		{
			SetDefaultState();
			Storage.Instance.Register(this);
		}

		private void OnDisable()
		{
			foreach (StateComponent comp in _components)
			{
				comp.OnDisable();
			}

			Storage.Instance.Delete(this);
		}

		public void Update()
		{
			float deltaTime = Time.deltaTime;
			_state?.Update(deltaTime);
			ComponentUpdate(deltaTime);
		}

		public void FixedUpdate()
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			_state?.FixedUpdate(fixedDeltaTime);
			ComponentFixedUpdate(fixedDeltaTime);
		}

		#region state
		private State _state;

		public Type DefaultStateType { get; set; }

		public string PrevState { get; set; }

		public string CurrentState { get => _state?.GetType().Name ?? "none"; private set { } }

		public State State { get => _state; private set => _state = value; }

		public bool IsInState<T>()
		{
			return _state?.GetType() == typeof(T);
		}

		public void SetDefaultState()
		{
			if (DefaultStateType != null)
				SetState(DefaultStateType);
			else
				SetState(state: null);
		}

		public void SetState<T>() where T : State
		{
			SetState(typeof(T));
		}

		public void SetState(Type type)
		{
			if (!type.IsSubclassOf(typeof(State)))
			{
				throw new ArgumentException(
					nameof(type),
					$"The type should be a subclass of {typeof(State).Name}"
				);
			}

			SetState((State)Activator.CreateInstance(type));
		}

		public void LateInitState()
		{
			_state.OnLateInit();
		}

		private void SetState(State state)
		{
			_state?.OnExit();
			DisableAllAbilities();

			if (_state != null) PrevState = _state.GetType().Name;

			_state = state;
			if (_state != null)
			{
				_state.Controller = this;
				_state.OnEnter();
			}
		}

		private void DisableAllAbilities()
		{
			foreach (StateComponent component in _components)
			{
				if (component is Ability) component.OnDisable();
			}
		}
		#endregion

		#region component

		public T GetStateComponent<T>() where T : StateComponent
		{
			foreach (StateComponent component in _components)
			{
				if (component is T tComponent)
					return tComponent;
			}
			return null;
		}

		private void ComponentUpdate(float deltaTime)
		{
			foreach (StateComponent component in _components)
			{
				if (!component.Enabled) continue;
				component.Update(deltaTime);
			}
		}

		private void ComponentFixedUpdate(float deltaTime)
		{
			foreach (StateComponent component in _components)
			{
				if (!component.Enabled) continue;
				component.FixedUpdate(Time.fixedDeltaTime);
			}
		}

		private void OnDestroy()
		{
			Storage.Instance.Delete(this);

			for (int i = 0; i < _components.Count; i++)
			{
				_components[i].OnDestroy();
			}
		}

		private void OnValidate()
		{
			for (int i = 0; i < _components.Count; i++)
			{
				_components[i].OnValidate();
			}
		}
		#endregion

		string ISelectableInfo.GetInfo()
		{
			string text = "Controller: \n" +
				$"Previous State: {PrevState} \n" + 
				$"Current State: {CurrentState}";

			foreach (StateComponent component in _components)
			{
				string t = component.DisplayInfo();
				if (!string.IsNullOrEmpty(t))
				{
					text += "\n" + t;
				}
			}

			return text;
		}
	}
}