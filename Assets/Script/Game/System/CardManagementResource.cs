using UnityEngine;
using CardGame.Card;

namespace CardGame.StateMachine
{
	public class CardManagementResource : Resource
	{
		[SerializeField]
		private CardContainer _playedCardContainer;
		[SerializeField, Min(0)]
		private int _maxCardPlayedCard;

		[Space(10)]

		[SerializeField]
		private CardContainer _inHandCardContainer;
		[SerializeField, Min(0)]
		private int _maxCardInHand;

		public CardContainer PlayedCardContainer
		{
			get => _playedCardContainer;
			private set => _playedCardContainer = value;
		}

		public int MaxCardPlayedCard
		{
			get => _maxCardPlayedCard;
			private set => _maxCardPlayedCard = value;
		}

		public CardContainer InHandCardContainer
		{
			get => _inHandCardContainer;
			private set => _inHandCardContainer = value;
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

		public CardScriptable GetRandomCard()
		{
			int rand = Random.Range(0, _cards.Length);
			return _cards[rand];
		}
	}
}