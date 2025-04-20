using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Card
{
	[CreateAssetMenu(fileName = "NewCardScriptable", menuName = "CardScriptable")]
	public class CardScriptable : ScriptableObject
	{
		public string CardName;
		public string CardDescritpion;
		public Sprite Visual;

		[Space(10)]

		[SubclassSelector(typeof(CardEffect))]
		public List<CardEffect> CardEffects;

		[SubclassSelector(typeof(CardEffect))]
		public List<CardEffect> VisualEffect;
	}
}