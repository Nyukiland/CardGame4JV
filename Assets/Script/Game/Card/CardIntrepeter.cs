using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Card
{
	public class CardIntrepeter : MonoBehaviour
	{
		CardIntrepeter(CardScriptable cardInfo)
		{
			_cardInfo = cardInfo;
		}

		private CardScriptable _cardInfo;

		public CardScriptable CardInfo
		{
			get => _cardInfo;
			private set => _cardInfo = value;
		}

		public Vector3 GoToPos { get; set; }
		public float Speed { get; set; }

		private void Start()
		{
			_cardInfo.VisualEffect.ForEach(x => x.Init(this));
			_cardInfo.CardEffects.ForEach(x => x.Init(this));
		}

		private void Update()
		{
			transform.position = Vector3.Lerp(transform.position, GoToPos, Time.deltaTime);
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