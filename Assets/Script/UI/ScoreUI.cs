using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
	public class ScoreUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private Image _profilePictureImage;
		[SerializeField] private Button _scoreButton;

		[SerializeField] private Color _colorPlayer1;
		[SerializeField] private Color _colorPlayer2;

		[SerializeField] private Image _background;
		[SerializeField] private Color _baseColor;
		[SerializeField] private Color _notMyTurnColor;

		private bool _isYou;

		public int PlayerIndex { get; private set; }
		public Button ScoreButton => _scoreButton;

		public void Setup(int playerIndex, bool isPlayer)
		{
			_isYou = isPlayer;
			PlayerIndex = playerIndex;

			_profilePictureImage.color = playerIndex == 0 ? _colorPlayer1 : _colorPlayer2;

			// _profilePictureImage.sprite = ????

			SetScore(0f);
		}

		public void SetScore(float score)
		{
			string text = score.ToString();
			if (_isYou) text += " (You)";
			_scoreText.text = text;
		}

		public void IsMyTurn(bool isIt)
		{
			_background.color = _isYou == isIt ? _baseColor : _notMyTurnColor;
		}
	}
}