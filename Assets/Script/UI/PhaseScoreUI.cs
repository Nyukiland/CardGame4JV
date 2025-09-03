using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame
{
    public class PhaseScoreUI : MonoBehaviour
    {
		[SerializeField] private TextMeshProUGUI _phaseScoreText;
		//[SerializeField] private Image _phasePicture;
		//[SerializeField] private Button _scoreButton;

		[SerializeField] private Image _background;
		[SerializeField] private Color _colorPlayer1;
		[SerializeField] private Color _colorPlayer2;

		[SerializeField] private Image _classicPhaseImage;
		[SerializeField] private Image _flagPhaseImage;

		public int PlayerIndex { get; private set; }

		public void Setup(int playerIndex, bool isFlagPhase)
		{
			PlayerIndex = playerIndex;

			if(isFlagPhase)
			{
				_classicPhaseImage.gameObject.SetActive(false);
				_flagPhaseImage.gameObject.SetActive(true);
                _flagPhaseImage.color = playerIndex == 0 ? _colorPlayer1 : _colorPlayer2;

            } else
			{
				_flagPhaseImage.gameObject.SetActive(false);
				_classicPhaseImage.gameObject.SetActive(true);
                _classicPhaseImage.color = playerIndex == 0 ? _colorPlayer1 : _colorPlayer2;
            }
            _flagPhaseImage.color = playerIndex == 0 ? _colorPlayer1 : _colorPlayer2;

			SetScore(0f);
		}

		public void SetScore(float score)
		{
			string text = "+ " + score.ToString();
			_phaseScoreText.text = text;
		}
	}
}
