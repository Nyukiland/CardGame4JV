using CardGame.Card;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardContainer : MonoBehaviour
{
	[SerializeField]
	private float _cardSpeed;
	[SerializeField]
	private float _cardSpacing;

	private RectTransform _rect;

	private void Start()
	{
		_rect = GetComponent<RectTransform>();
	}

	public bool IsInRect(Vector3 pos)
	{
		return _rect.rect.Contains(pos);
	}

	public void GiveCardPosition()
	{
		CardIntrepeter[] cards = GetComponentsInChildren<CardIntrepeter>();

		Vector3 sideMove = _cardSpacing * (float)cards.Length/2 * Vector3.left;

		for (int i = 0; i < cards.Length; i++)
		{
			CardIntrepeter card = cards[i];
			Vector3 offset = _cardSpacing * i * Vector3.right;
			card.GoToPos = transform.position + sideMove + offset;
			card.Speed = _cardSpeed;
		}
	}
}