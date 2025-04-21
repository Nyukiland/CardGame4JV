using System;
using UnityEngine;

namespace CardGame.Card
{
	[Serializable]
	public abstract class CardEffect
	{
		[SerializeField, Disable]
		protected CardInfo _card;

		public void Init(CardInfo card)
		{
			_card = card;
		}

		public virtual void OnCardPlaced()
		{

		}

		public virtual void OnCardRetrieve()
		{

		}

		public virtual void BeforeScoring()
		{

		}

		public virtual void Scoring()
		{

		}

		public virtual void AfterScoring()
		{

		}
	}
}