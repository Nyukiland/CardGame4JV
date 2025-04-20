using UnityEngine;
using CardGame.Card;

namespace CardGame.StateMachine
{
	public class CardManagementResource : Resource
	{
		[SerializeField]
		private CardContainer _playedCardContainer;

		[Space(10)]

		[SerializeField]
		private CardContainer _inHandCardContainer;

		public CardContainer PlayedCardContainer
		{
			get => _playedCardContainer;
			private set => _playedCardContainer = value;
		}

		public CardContainer InHandCardContainer
		{
			get => _inHandCardContainer;
			private set => _inHandCardContainer = value;
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