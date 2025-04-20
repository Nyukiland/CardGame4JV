using UnityEngine;

namespace CardGame.Card
{
	public abstract class CardEffect
	{
		protected CardIntrepeter _card;

		public void Init(CardIntrepeter card)
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