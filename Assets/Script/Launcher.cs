using System.Collections.Generic;
using CardGame.Useful;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;

namespace CardGame
{
    // Script used to initalize the game, might be moved later
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private EventSystem _eventSystem;
        public static Launcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            ResourceManager.ExceptionHandler = AddressableErrorHandler.HandleAddressableException;
            
            if (SceneManager.loadedSceneCount > 1)
                _eventSystem.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (this == Instance)
                Instance = null;
        }
    }
}