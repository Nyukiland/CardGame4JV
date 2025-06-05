using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class NetworkUI : MonoBehaviour
    {
        #region Variables
        
        [Header("MainMenu")]
        [SerializeField] private GameObject _mainMenuGameObject;
        [SerializeField] private Button _mainHostButton;
        [SerializeField] private Button _mainConnectButton;
        
        [Header("BeforeHost")]
        [SerializeField] private GameObject _beforeHostGameObject;
        [SerializeField] private TMP_InputField _sessionNameInput;
        [SerializeField] private TMP_InputField _passwordInputHost;
        [SerializeField] private Button _publicHostButton;
        [SerializeField] private Image _publicHostImage;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Image _hostButtonGrey;
        [SerializeField] private Button _hostBackButton;
        
        [Header("AfterHost")]
        [SerializeField] private GameObject _afterHostGameObject;
        [SerializeField] private TextMeshProUGUI _sessionNameText;
        [SerializeField] private TextMeshProUGUI _codeText;
        [SerializeField] private TextMeshProUGUI _passwordText;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private Button _unHostButton;
        [SerializeField] private TextMeshProUGUI _playersNumberText;
        [SerializeField] private Button _playButton;
        [SerializeField] private Image _playButtonGrey;
        
        [Header("BeforeClient")]
        [SerializeField] private GameObject _beforeClientGameObject;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _codeInput;
        [SerializeField] private TMP_InputField _passwordInputClient;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Image _connectButtonGrey;
        [SerializeField] private Button _publicFindButton;
        [SerializeField] private Image _publicFindImage;
        [SerializeField] private GameObject _publicHostsContainer;
        [SerializeField] private Button _clientBackButton;
        
        [Header("AfterClient")]
        [SerializeField] private GameObject _afterClientGameObject;
        [SerializeField] private Button _quitGameButton;

        // Events
        public StringEvent CopyCodeEvent;
        public Action PastCodeEvent;
        
        // Delegates
        public delegate void StringEvent(string eventString);
        
        // private variables
        private CurrentScreen _currentScreen;
        private bool _isPublicShown;
        private string _sessionName = "No session name found";
        private string _code = "No code found";
        private string _playerName = "No player name found";
        private string _password = "No password found";

        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            // Buttons
            _mainHostButton.onClick.AddListener(OpenBeforeHost);
            _mainConnectButton.onClick.AddListener(OpenBeforeClient);
            _publicHostButton.onClick.AddListener(TogglePublicGames);
            _hostButton.onClick.AddListener(OpenAfterHost);
            _hostBackButton.onClick.AddListener(OpenMainMenu);
            _copyCodeButton.onClick.AddListener(CopyHostCode);
            _unHostButton.onClick.AddListener(Unhost);
            _playButton.onClick.AddListener(StartGame);
            _connectButton.onClick.AddListener(OpenAfterClient);
            _publicFindButton.onClick.AddListener(TogglePublicGames);
            _quitGameButton.onClick.AddListener(QuitHostedGame);
            _clientBackButton.onClick.AddListener(OpenMainMenu);
            
            // Inputs fields
            _sessionNameInput.onEndEdit.AddListener(UpdateInputField);
            _passwordInputHost.onEndEdit.AddListener(UpdateInputField);
            _nameInput.onEndEdit.AddListener(UpdateInputField);
            _codeInput.onEndEdit.AddListener(UpdateInputField);
            _passwordInputClient.onEndEdit.AddListener(UpdateInputField);

            OpenMainMenu();
        }

        private void OnValidate()
        {
            switch (_currentScreen)
            {
                case CurrentScreen.None:
                    return;
                case CurrentScreen.MainMenu:
                    break;
                case CurrentScreen.BeforeHost:
                    _publicHostImage.gameObject.SetActive(_isPublicShown);
                    if (string.IsNullOrEmpty(_sessionNameInput.text))
                    {
                        _hostButtonGrey.gameObject.SetActive(true);
                        _hostButton.interactable = false;
                    }
                    else
                    {
                        _hostButtonGrey.gameObject.SetActive(false);
                        _hostButton.interactable = true;
                    }
                    break;
                case CurrentScreen.AfterHost:
                    int playerNumber = NetworkManager.Singleton.ConnectedClients.Count;
                    _playersNumberText.text = $"{playerNumber}/4 players";
                    if (playerNumber < 1)
                    {
                        _playButtonGrey.gameObject.SetActive(true);
                        _playButton.interactable = false;
                    }
                    else
                    {
                        _playButtonGrey.gameObject.SetActive(true);
                        _playButton.interactable = false;
                    }
                    break;
                case CurrentScreen.BeforeClient:
                    _publicFindImage.gameObject.SetActive(_isPublicShown);
                    _publicHostsContainer.SetActive(_isPublicShown);
                    if (string.IsNullOrEmpty(_nameInput.text) || string.IsNullOrEmpty(_codeInput.text))
                    {
                        _connectButtonGrey.gameObject.SetActive(true);
                        _connectButton.interactable = false;
                    }
                    else
                    {
                        _connectButtonGrey.gameObject.SetActive(false);
                        _connectButton.interactable = true;
                    }
                    break;
                case CurrentScreen.AfterClient:
                    break;
                default:
                    break;
            }
        }

        #endregion
        
        #region Change Panels
        
        public void OpenMainMenu()
        {
            OpenPanel(_mainMenuGameObject);
            _currentScreen = CurrentScreen.MainMenu;
            
            OnValidate();
        }
        
        public void OpenBeforeHost()
        {
            OpenPanel(_beforeHostGameObject);
            _currentScreen = CurrentScreen.BeforeHost;
            
            OnValidate();
        }

        public void OpenAfterHost()
        {
            _sessionName = _sessionNameInput.text;
            _password = _passwordInputHost.text;
            if (string.IsNullOrEmpty(_password)) _password = "- None -";
            
            OpenPanel(_afterHostGameObject);
            _currentScreen = CurrentScreen.AfterHost;
            
            OnValidate();

            _sessionNameText.text = _sessionName;
            _codeText.text = _code;
            _passwordText.text = _password;
        }

        public void OpenBeforeClient()
        {
            OpenPanel(_beforeClientGameObject);
            _currentScreen = CurrentScreen.BeforeClient;
            
            OnValidate();
        }

        public void OpenAfterClient()
        {
            OpenPanel(_afterClientGameObject);
            _currentScreen = CurrentScreen.AfterClient;
            
            OnValidate();
        }

        public void CloseMenu()
        {
            OpenPanel(null, true);
            _currentScreen = CurrentScreen.None;
        }
        
        #endregion
        
        #region Methods

        private void OpenPanel(GameObject panel, bool closeAll = false)
        {
            // Warning : only open from Change Panel methods 
            _mainMenuGameObject.SetActive(false);
            _beforeHostGameObject.SetActive(false);
            _afterHostGameObject.SetActive(false);
            _beforeClientGameObject.SetActive(false);
            _afterClientGameObject.SetActive(false);
            
            if (!closeAll)
                panel.SetActive(true);
        }

        private void TogglePublicGames()
        {
            _isPublicShown = !_isPublicShown;

            if (_currentScreen == CurrentScreen.BeforeHost)
            {
                _publicHostImage.gameObject.SetActive(_isPublicShown);
                // Hosting will be public
                return;
            }

            if (_currentScreen == CurrentScreen.BeforeClient)
            {
                _publicFindImage.gameObject.SetActive(_isPublicShown);
                _publicHostsContainer.SetActive(_isPublicShown);
                // Show public hosted games
                return;
            }
            
            Debug.LogWarning($"The public hosting just got toggled in a screen that shouldn't be able to do it ({_currentScreen}).)");
        }

        private void UpdateInputField(string newText)
        {
            OnValidate();
        }

        private void Unhost()
        {
            OpenBeforeHost();
            
            // Interrupt hosting
        }

        private void CopyHostCode()
        {
            CopyCodeEvent?.Invoke(_code);
        }

        public void StartGame()
        {
            // Launch game with current players
        }

        public void QuitHostedGame()
        {
            OpenBeforeClient();
            
            // Remove player from game
        }

        #endregion
    }

    public enum CurrentScreen
    {
        None,
        MainMenu,
        BeforeHost,
        AfterHost,
        BeforeClient,
        AfterClient,
    }
}