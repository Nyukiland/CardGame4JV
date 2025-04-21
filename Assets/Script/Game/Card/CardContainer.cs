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
		if (considerMax) return _rect.rect.Contains(pos) && GetComponentsInChildren<CardInfo>().Length < MaxCard;
		return _rect.rect.Contains(pos);
	}

	public void GiveCardPosition()
	{
		CardInfo[] cards = GetComponentsInChildren<CardInfo>();

		Vector3 sideMove = _cardSpacing * (float)cards.Length/2 * Vector3.left;

		for (int i = 0; i < cards.Length; i++)
		{
			CardInfo card = cards[i];
			Vector3 offset = _cardSpacing * i * Vector3.right;
			card.MoveCard(transform.position + sideMove + offset, _cardSpeed);
		}
	}
}