using CardGame.StateMachine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.Turns
{
	public class ScoringCombinedState : CombinedState
	{
		private ScoringAbility _scoring;
		private MoveTileAbility _moveTile;
		private ZoneHolderResource _handResource;
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
			GetStateComponent(ref _moveTile);
			GetStateComponent(ref _handResource);

			_scoring.CallScoring();
		}

		public override void OnExit()
		{
			base.OnExit();

			if (_moveTile.CurrentTile != null)
			{
				_handResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}
		}

		public override void OnActionTriggered(InputAction.CallbackContext context)
		{
			base.OnActionTriggered(context);

			if (context.action.name != "Touch")
				return;

			if (context.phase == InputActionPhase.Canceled)
			{
				if (_moveTile.CurrentTile == null)
					return;

				_handResource.GiveTileToHand(_moveTile.CurrentTile.gameObject);
				_moveTile.CurrentTile = null;
			}
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