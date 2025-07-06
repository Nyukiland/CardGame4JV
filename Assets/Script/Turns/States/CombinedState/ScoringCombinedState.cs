using CardGame.StateMachine;
using System;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringCombinedState : CombinedState
	{
		private ScoringAbility _scoring;
		private HUDResource _hud;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _scoring);
			GetStateComponent(ref _hud);

			//exception
			if (_scoring.TilePlacedPosition == new Vector2Int(-100, -100))
			{
				Controller.SetState<PlaceTileCombinedState>();
				return;
			}

			_hud.OpenScoringScreen();
			_scoring.CallScoring();
		}

		public override void OnExit()
		{
			base.OnExit();
			_hud.CloseScoringScreen();
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