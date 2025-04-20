using UnityEngine;

namespace CardGame.Card
{
	public abstract class CardEffect
	{
		protected GameObject _card;

		public void Init(GameObject card)
		{
			_card = card;
		}

		public virtual void OnCardPlaced()
		{

		}

		public virtual void OnCardDestroy()
		{
			GameObject.Destroy(_card);
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