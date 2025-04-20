using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	[SerializeField]
	private float _score;

	public float Score { get => _score; set => _score = value; }
}