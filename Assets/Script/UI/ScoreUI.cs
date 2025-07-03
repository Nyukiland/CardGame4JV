using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Image _profilePictureImage;
        public string PlayerName { get; private set; }
        public int PlayerIndex { get; private set; }

        public void Setup(int playerIndex, string playerName)
        {
            PlayerName = playerName;
            PlayerIndex = playerIndex;
            // _profilePictureImage.sprite = ????

            SetScore(0f);
        }
        
        public void SetScore(float score)
        {
            _scoreText.text = $"{PlayerName} : {score}";
        }
    }
}