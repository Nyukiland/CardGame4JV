using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
	public class ScoreUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _scoreText;
		[SerializeField] private TextMeshProUGUI _scoreAdd;
		[SerializeField] private Image _profilePictureImage;
		[SerializeField] private Image _crownImage;
		[SerializeField] private Button _scoreButton;

		[SerializeField] private Color _colorPlayer1;
		[SerializeField] private Color _colorPlayer2;

		[SerializeField] private Image _background;
		[SerializeField] private Color _baseColor;
		[SerializeField] private Color _notMyTurnColor;

		[SerializeField] private Material _textMatPlayer1;
		[SerializeField] private Material _textMatPlayer2;

		private bool _isYou;

		public int PlayerIndex { get; private set; }
		public Button ScoreButton => _scoreButton;

		public void Setup(int playerIndex, bool isPlayer)
		{
			_isYou = isPlayer;
			PlayerIndex = playerIndex;

			_profilePictureImage.color = playerIndex == 0 ? _colorPlayer1 : _colorPlayer2;
			_scoreAdd.material = playerIndex == 0 ? _textMatPlayer1 : _textMatPlayer2;
			_crownImage.enabled = false;

			// _profilePictureImage.sprite = ????

			SetScore(0f);
		}

		public void SetScore(float score)
		{
			string text = score.ToString();
			if (_isYou) text += " (You)";
			_scoreText.text = text;
		}

		public void SetCummulativeScore(int score)
		{
			if (score == 0)
			{
				_scoreAdd.text = "";
				return;
			}

			_scoreAdd.text = "+ " + score;
		}

		public void SetCrown(bool amIWinning) => _crownImage.enabled = amIWinning;

		public void IsMyTurn(bool isIt)
		{
			_background.color = _isYou == isIt ? _baseColor : _notMyTurnColor;
		}
	}
}