using CardGame.StateMachine;
using System;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringCombinedState : CombinedState
	{
		private ScoringAbility _scoring;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _scoring);
			if (_scoring.TilePlacedPosition == new Vector2Int(-100, -100))
				return;

			_scoring.CallScoring();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_scoring.IsScoringFinished)
			{
				Controller.SetState(_scoring.NextState);
			}
		}
	}
}