using System;
using UnityEngine;

namespace CardGame.StateMachine
{
	[System.Serializable]
	public abstract class StateComponent
	{
		[HideInInspector]
		public bool Enabled = false;

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
				OnEnable();
			else
				OnDisable();
		}

		public virtual void OnEnable() { }

		public virtual void OnDisable() { }

		public virtual void Update(float deltaTime) { }

		public virtual void FixedUpdate(float fixedDeltaTime) { }

		public virtual void OnValidate() { }

		public virtual void OnDestroy() { }

		public virtual string DisplayInfo()
		{
			//copy this in children
			if (!Enabled) return "";

			return "";
		}
	}
}