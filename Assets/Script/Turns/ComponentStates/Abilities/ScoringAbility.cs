using CardGame.StateMachine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringAbility : Ability
	{
		private GridManagerResource _gridManager;

		public Vector2Int TilePlaced
		{
			get;
			private set;
		}

		public Type NextState
		{
			get;
			private set;
		}

		public bool IsScoringFinished
		{
			get;
			private set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_gridManager = owner.GetStateComponent<GridManagerResource>();
		}

		public void SetScoringPos(Vector2Int pos)
		{
			TilePlaced = pos;
		}

		public void SetState(Type type)
		{
			NextState = type;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			TilePlaced = new(-100, -100);
			IsScoringFinished = false;
		}

		public void CallScoring()
		{
			ScoringAsync().Forget();
		}

		private async UniTask ScoringAsync()
		{


			await UniTask.Yield();

			IsScoringFinished = true;
		}
	}
}