using TMPro;
using UnityEngine;

namespace CardGame.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        public string PlayerName { get; private set; }
        public int PlayerIndex { get; private set; }

        public void Setup(int playerIndex, string playerName)
        {
            PlayerName = playerName;
            PlayerIndex = playerIndex;

            SetScore(0f);
        }
        
        public void SetScore(float score)
        {
            _scoreText.text = $"{PlayerName} : {score}";
        }
    }
}