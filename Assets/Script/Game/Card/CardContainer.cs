using CardGame.Card;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardContainer : MonoBehaviour
{
	[SerializeField]
	private float _cardSpeed;
	[SerializeField]
	private float _cardSpacing;

	[SerializeField, Min(0)]
	private int _maxCard;

	public int MaxCard
	{
		get => _maxCard;
		private set => _maxCard = value;
	}

	private RectTransform _rect;

	private void Start()
	{
		_rect = GetComponent<RectTransform>();
	}

	public bool IsInRect(Vector3 pos, bool considerMax = true)
	{
		if (considerMax && GetComponentsInChildren<CardInfo>().Length >= MaxCard) return false;

		return RectTransformUtility.RectangleContainsScreenPoint(_rect, pos);
	}

	public void GiveCardPosition()
	{
		CardInfo[] cards = GetComponentsInChildren<CardInfo>();
		int cardCount = cards.Length;

		float startOffset = -(cardCount - 1) / 2f;

		for (int i = 0; i < cards.Length; i++)
		{
			CardInfo card = cards[i];
			Vector3 offset = _cardSpacing * (startOffset + i) * Vector3.right;
			card.MoveCard(transform.position + offset, _cardSpeed);
		}
	}
}