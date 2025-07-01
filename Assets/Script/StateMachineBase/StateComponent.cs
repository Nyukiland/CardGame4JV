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

		public void InitController(Controller owner)
		{
			Owner = owner;

			//i don't like the cast but i guess should do it
			if (this is Resource) Enabled = true;

			Init(owner);
		}


		public virtual void Init(Controller owner) {}

		public virtual void LateInit() { }

		public void SetActive(bool value)
		{
			if (value)
				OnEnableController();
			else
				OnEnableController();
		}

		public void OnEnableController()
		{
			//i don't like the cast but i guess should do it
			if (this is Ability) Enabled = true;

			OnEnable();
		}

		public void OnDisableController()
		{
			//i don't like the cast but i guess should do it
			if (this is Ability) Enabled = false;

			OnDisable();
		}

		public virtual void OnEnable()
		{

		}

		public virtual void OnDisable()
		{

		}

		public virtual void Update(float deltaTime) { }

		public virtual void FixedUpdate(float fixedDeltaTime) { }

		public virtual void OnValidate() { }

		public string DisplayInfoController()
		{
			if (DisplayInfo() == "") return "";

			return $"--{GetType().Name} \n" + DisplayInfo();
		}

		public virtual string DisplayInfo()
		{


			return "";
		}
	}
}