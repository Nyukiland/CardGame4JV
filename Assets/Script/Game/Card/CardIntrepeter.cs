using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Card
{
	public class CardIntrepeter : MonoBehaviour
	{
		private CardScriptable _cardInfo;

		public CardScriptable CardInfo
		{
			get => _cardInfo;
			private set => _cardInfo = value;
		}

		public Vector3 GoToPos { get; set; }
		public float Speed { get; set; }

		public void SetUp(CardScriptable cardInfo)
		{
			_cardInfo = cardInfo;

			_cardInfo.VisualEffect.ForEach(x => x.Init(this));
			_cardInfo.CardEffects.ForEach(x => x.Init(this));
		}

		private void Update()
		{
			if (!IsAtDestination()) transform.position = Vector3.Lerp(transform.position, GoToPos, Time.deltaTime);
			else transform.position = GoToPos;
		}

		public bool IsAtDestination()
		{
			return Vector3.Distance(transform.position, GoToPos) < 0.1f;
		}

		#region CardEffect
		public void OnCardPlaced()
		{
			_cardInfo.VisualEffect.ForEach(x => x.OnCardPlaced());
		}

		public void OnCardRetrieve()
		{
			_cardInfo.VisualEffect.ForEach(x => x.OnCardRetrieve());
		}

		public void BeforeScoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.BeforeScoring());
		}

		public void Scoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.Scoring());
		}

		public void AfterScoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.AfterScoring());
		}
		#endregion
	}
}