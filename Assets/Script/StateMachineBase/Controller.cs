using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class Controller : MonoBehaviour
{
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

	public void SetState<T>() where T : State, new()
	{
		SetState(new T());
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

	//public void SetConfigurableState<TState, TConfig>(in TConfig config)
	//	where TState : ConfigurableCCState<TConfig>, new()
	//	where TConfig : struct
	//{
	//	var state = new TState();
	//	state.Config = config;
	//	SetState(state);
	//}

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
			if (component is Ability) component.Disable();
		}
	}

	private void OnActionTriggered(InputAction.CallbackContext context)
	{
		_state?.OnActionTriggered(context);
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
	#endregion

	#region component

	[SerializeField, SerializeReference, /*SubclassSelector*/]
	private List<StateComponent> _components;

	protected virtual void Awake()
	{
		_components.ForEach(comp => comp.EarlyInit());
		_components.ForEach(comp => comp.Init(owner: this));
	}

	protected virtual void Start()
	{
		_components.ForEach(comp => comp.LateInit());
	}

	public void AddComponents(StateComponent[] components)
	{
		if (components == null)
		{
			UnityEngine.Debug.LogError("Null Component");
			return;
		}

		foreach (StateComponent component in components)
		{
			_components.Add(component);
		}

		Array.ForEach(components, comp => comp.EarlyInit());
		Array.ForEach(components, comp => comp.Init(owner: this));
		Array.ForEach(components, comp => comp.SetActive(isActiveAndEnabled));
		Array.ForEach(components, comp => comp.LateInit());
	}

	public void RemoveComponents(StateComponent[] components)
	{
		foreach (StateComponent component in components)
		{
			component.Disable();
			_components.Remove(component);
		}
	}

	private void OnEnable()
	{
		foreach (StateComponent comp in _components)
		{
			EnableComponent(comp);
		}

		SetDefaultState();
		Storage.Instance.Register(this);
	}

	private void OnDisable()
	{
		foreach (StateComponent comp in _components)
		{
			DisableComponent(comp);
		}

		Storage.Instance.Delete(this);
	}

	public void EnableComponent(StateComponent component)
	{
		int index = _components.IndexOf(component);

		component.OnEnable();
	}

	public void DisableComponent(StateComponent component)
	{
		int index = _components.IndexOf(component);

		component.OnDisable();
	}

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
			component.Update(deltaTime);
		}
	}

	private void ComponentFixedUpdate(float deltaTime)
	{
		foreach (StateComponent component in _components)
		{
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
}