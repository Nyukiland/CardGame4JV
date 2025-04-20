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
		public Sprite Background;

		[Space(10)]

		[SerializeReference, SubclassSelector(typeof(CardEffect))]
		public List<CardEffect> CardEffects = new();

		[SerializeReference, SubclassSelector(typeof(CardEffect))]
		public List<CardEffect> VisualEffect = new();
	}
}