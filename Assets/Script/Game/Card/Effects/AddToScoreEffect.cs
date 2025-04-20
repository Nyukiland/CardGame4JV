using System;
using UnityEngine;

namespace CardGame.Card
{
	[Serializable]
	public class AddToScoreEffect : CardEffect
	{
		[SerializeField]
		private int scoreAdd;

		public override void Scoring()
		{
			base.Scoring();
			GameManager.Instance.Score += scoreAdd;
		}
	}
}