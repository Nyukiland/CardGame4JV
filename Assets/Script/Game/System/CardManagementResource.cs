using UnityEngine;
using CardGame.Card;

namespace CardGame.StateMachine
{
	public class CardManagementResource : Resource
	{
		[SerializeField]
		private Transform _playedCardContainer;
		[SerializeField, Min(0)]
		private int _maxCardPlayedCard;

		[Space(10)]

		[SerializeField]
		private Transform _inHandCardContainer;
		[SerializeField, Min(0)]
		private int _maxCardInHand;

		public int MaxCardPlayedCard
		{
			get => _maxCardPlayedCard;
			private set => _maxCardPlayedCard = value;
		}

		public int MaxCardInHand
		{
			get => _maxCardInHand;
			private set => _maxCardInHand = value;
		}

		private CardScriptable[] _cards;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cards = Resources.LoadAll<CardScriptable>("Cards");
		}

		public CardInterpeter[] GetCardPlayed()
		{
			return _playedCardContainer.GetComponentsInChildren<CardInterpeter>();
		}

		public CardInterpeter[] GetCardInHand()
		{
			return _inHandCardContainer.GetComponentsInChildren<CardInterpeter>();
		}

		public CardScriptable GetRandomCard()
		{
			int rand = Random.Range(0, _cards.Length);
			return _cards[rand];
		}
	}
}