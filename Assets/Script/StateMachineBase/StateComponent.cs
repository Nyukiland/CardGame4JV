[System.Serializable]
public abstract class StateComponent
{
	public Controller Owner { get; private set; }

	public virtual void EarlyInit() { }

	public virtual void Init(Controller owner)
	{
		Owner = owner;
	}

	public virtual void LateInit() { }

	public void SetActive(bool value)
	{
		if (value)
			Enable();
		else
			Disable();
	}

	public void Enable()
	{
		//Owner.EnableComponent(this);
	}

	public void Disable()
	{
		//Owner.DisableComponent(this);
	}

	public virtual void OnEnable() { }

	public virtual void OnDisable() { }

	public virtual void Update(float deltaTime) { }

	public virtual void FixedUpdate(float fixedDeltaTime) { }

	public virtual void OnValidate() { }

	public virtual void OnDestroy() { }
}