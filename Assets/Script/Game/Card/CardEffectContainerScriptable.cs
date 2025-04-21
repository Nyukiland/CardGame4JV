using UnityEngine;

namespace CardGame.Card
{
	[CreateAssetMenu(fileName = "CardEffectContainer", menuName = "Card/CardEffectContainer")]
	public class CardEffectContainerScriptable : ScriptableObject
	{
		[SerializeReference, SubclassSelector(typeof(CardEffect))]
		public CardEffect Effect;
	}
}