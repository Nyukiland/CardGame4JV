using System.Collections.Generic;
using CardGame.Managers;
using CardGame.Useful;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;

namespace CardGame
{
    // Script used to initalize the game, must be moved later
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private TextMeshProUGUI _scoreText;

        #region Singleton
        public static Launcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ScoreManager.ScoringEvent += UpdateScore;
        }
        
        private void OnDestroy()
        {
            ScoreManager.ScoringEvent -= UpdateScore;
            
            if (this == Instance)
                Instance = null;
        }
        
        #endregion
        
        private void Start()
        {
            ResourceManager.ExceptionHandler = AddressableErrorHandler.HandleAddressableException;
            
            if (SceneManager.loadedSceneCount > 1)
                _eventSystem.gameObject.SetActive(false);
        }

        private void UpdateScore(int score)
        {
            _scoreText.text = $"Score : {score}";
        }
    }
}