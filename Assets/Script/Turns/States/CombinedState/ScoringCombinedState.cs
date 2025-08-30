using CardGame.StateMachine;
using System;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringCombinedState : CombinedState
	{
		private ScoringAbility _scoring;
		private HUDResource _hud;

		public ScoringCombinedState()
		{
			AddSubState(new MoveTileSubState(false));
		}

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _scoring);
			GetStateComponent(ref _hud);

			_scoring.CallScoring();
			_hud.ChangeTurnFeedback(HUDResource.TurnState.Scoring);
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (_scoring.IsScoringFinished)
			{
				if (_scoring.NextState == null) Controller.SetState<PlaceTileCombinedState>();
				else Controller.SetState(_scoring.NextState);
			}
		}
	}
}