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
        
        [Header("AfterClient")]
        [SerializeField] private GameObject _afterClientGameObject;
        [SerializeField] private Button _quitGameButton;
        
        private CurrentScreen _currentScreen;
        private bool _isPublicShown;
        private string _sessionName;
        private string _code = "ouais le code";
        private string _playerName;
        private string _password;

        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            // Buttons
            _mainHostButton.onClick.AddListener(OpenHost);
            _mainConnectButton.onClick.AddListener(OpenClient);
            _publicHostButton.onClick.AddListener(TogglePublicGames);
            _hostButton.onClick.AddListener(WaitForPlayers);
            _copyCodeButton.onClick.AddListener(CopyHostCode);
            _unHostButton.onClick.AddListener(Unhost);
            _playButton.onClick.AddListener(StartGame);
            _connectButton.onClick.AddListener(WaitForHost);
            _publicFindButton.onClick.AddListener(TogglePublicGames);
            _quitGameButton.onClick.AddListener(QuitHostedGame);
            
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
        
        #region ChangePanels
        
        public void OpenMainMenu()
        {
            OpenPanel(_mainMenuGameObject);
            _currentScreen = CurrentScreen.MainMenu;
            
            OnValidate();
        }
        
        public void OpenHost()
        {
            OpenPanel(_beforeHostGameObject);
            _currentScreen = CurrentScreen.BeforeHost;
            
            OnValidate();
        }

        public void WaitForPlayers()
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

        public void OpenClient()
        {
            OpenPanel(_beforeClientGameObject);
            _currentScreen = CurrentScreen.BeforeClient;
            
            OnValidate();
        }

        public void WaitForHost()
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
            OpenPanel(_beforeHostGameObject);
            
            // Interrupt hosting
        }

        private void CopyHostCode()
        {
            // Copy code
        }

        public void StartGame()
        {
            // Launch game with current players
        }

        public void QuitHostedGame()
        {
            OpenPanel(_beforeClientGameObject);
            
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