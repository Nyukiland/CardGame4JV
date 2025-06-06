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

		[SerializeField] private GameObject _inputBlocker;

		[Header("MainMenu")]
		[SerializeField] private GameObject _mainMenuGameObject;
		[SerializeField] private Button _mainHostButton;
		[SerializeField] private Button _mainConnectButton;
		[SerializeField] private Button _mainSoloButton;

		[Header("BeforeHost")]
		[SerializeField] private GameObject _beforeHostGameObject;
		[SerializeField] private TMP_InputField _sessionNameInput;
		[SerializeField] private TMP_InputField _passwordInputHost;
		[SerializeField] private Toggle _publicHostToggle;
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
		[SerializeField] private TMP_InputField _playerNameInput;
		[SerializeField] private TMP_InputField _codeInput;
		[SerializeField] private TMP_InputField _passwordInputClient;
		[SerializeField] private Button _connectButton;
		[SerializeField] private Image _connectButtonGrey;
		[SerializeField] private Toggle _publicFindToggle;
		[SerializeField] private GameObject _publicHostsContainer;
		[SerializeField] private Button _clientBackButton;

		[Header("AfterClient")]
		[SerializeField] private GameObject _afterClientGameObject;
		[SerializeField] private Button _quitGameButton;

		// public variables
		public GameObject PublicHostsContainer => _publicHostsContainer;
		public string Code { get; set; } = "No code found";
		public string Password { get; set; } = "No password found";
		public string SessionName { get; set; } = "No session name found";
		public bool IsPublicShown { get; private set; }
		public string PlayerName { get; private set; } = "No player name found";

		// private variables
		private CurrentScreen _currentScreen;

		// Events
		public StringEvent CopyCodeEvent;
		public BoolEvent TogglePublicEvent;
		public Action StartHostEvent;
		public Action JoinGameEvent;
		public Action UnhostEvent;
		public Action QuitGameEvent;
		public Action PlayGameEvent;

		// Delegates
		public delegate void StringEvent(string stringEvent);
		public delegate void BoolEvent(bool boolEvent);

		#endregion

		private void Start()
		{
			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
		}

		#region Unity Methods

		[Header("Test")]
		[SerializeField] private TextMeshProUGUI _codeTest;
		[SerializeField] private TextMeshProUGUI _passwordTest;
		[SerializeField] private TextMeshProUGUI _sessionNameTest;
		[SerializeField] private TextMeshProUGUI _playerNameTest;
		private void Update()
		{
			_codeTest.text = "Code : " + Code;
			_passwordTest.text = "Password : " + Password;
			_sessionNameTest.text = "SessionName : " + SessionName;
			_playerNameTest.text = "PlayerName : " + PlayerName;
		}

		private void Awake()
		{
			// Buttons
			_mainHostButton.onClick.AddListener(OpenBeforeHost);
			_mainConnectButton.onClick.AddListener(OpenBeforeClient);
			_mainSoloButton.onClick.AddListener(StartGame);
			_hostButton.onClick.AddListener(CallStartHostEvent);
			_hostBackButton.onClick.AddListener(OpenMainMenu);
			_copyCodeButton.onClick.AddListener(CallCopyEvent);
			_unHostButton.onClick.AddListener(CallUnhostEvent);
			_playButton.onClick.AddListener(StartGame);
			_connectButton.onClick.AddListener(CallJoinGameEvent);
			_quitGameButton.onClick.AddListener(QuitClientGame);
			_clientBackButton.onClick.AddListener(OpenMainMenu);

			// Inputs fields
			_sessionNameInput.onEndEdit.AddListener(UpdateHostInputs);
			_passwordInputHost.onEndEdit.AddListener(UpdateHostInputs);
			_playerNameInput.onEndEdit.AddListener(UpdateClientInputs);
			_codeInput.onEndEdit.AddListener(UpdateClientInputs);
			//_codeInput.contentType = TMP_InputField.ContentType.Alphanumeric;
			//_codeInput.onValidateInput += delegate (string s, int i, char c) { return char.ToUpper(c); };
			_passwordInputClient.onEndEdit.AddListener(UpdateClientInputs);

			// Toggles
			_publicHostToggle.onValueChanged.AddListener(TogglePublicGames);
			_publicFindToggle.onValueChanged.AddListener(TogglePublicGames);

			OpenMainMenu();
		}

		private void OnDestroy()
		{
			// Buttons
			_mainHostButton.onClick.RemoveListener(OpenBeforeHost);
			_mainConnectButton.onClick.RemoveListener(OpenBeforeClient);
			_mainSoloButton.onClick.RemoveListener(StartGame);
			_hostButton.onClick.RemoveListener(CallStartHostEvent);
			_hostBackButton.onClick.RemoveListener(OpenMainMenu);
			_copyCodeButton.onClick.RemoveListener(CallCopyEvent);
			_unHostButton.onClick.RemoveListener(CallUnhostEvent);
			_playButton.onClick.RemoveListener(StartGame);
			_connectButton.onClick.RemoveListener(CallJoinGameEvent);
			_quitGameButton.onClick.RemoveListener(QuitClientGame);
			_clientBackButton.onClick.RemoveListener(OpenMainMenu);

			// Inputs fields
			_sessionNameInput.onEndEdit.RemoveListener(UpdateHostInputs);
			_passwordInputHost.onEndEdit.RemoveListener(UpdateHostInputs);
			_playerNameInput.onEndEdit.RemoveListener(UpdateClientInputs);
			_codeInput.onEndEdit.RemoveListener(UpdateClientInputs);
			_passwordInputClient.onEndEdit.RemoveListener(UpdateClientInputs);

			// Toggles
			_publicHostToggle.onValueChanged.RemoveListener(TogglePublicGames);
			_publicFindToggle.onValueChanged.RemoveListener(TogglePublicGames);
		}

		#endregion

		#region Update Visuals

		public void UpdateHostInputs(string inputString)
		{
			UpdateBeforeHost();
		}

		public void UpdateClientInputs(string inputString)
		{
			UpdateBeforeClient();
		}

		public void UpdateBeforeHost()
		{
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
		}

		public void UpdateAfterHost()
		{
			int playerNumber = NetworkManager.Singleton.ConnectedClients.Count;
			_playersNumberText.text = $"{playerNumber}/4 players";
			if (playerNumber < 2)
			{
				_playButtonGrey.gameObject.SetActive(true);
				_playButton.interactable = false;
			}
			else
			{
				_playButtonGrey.gameObject.SetActive(false);
				_playButton.interactable = true;
			}
		}

		public void UpdateBeforeClient()
		{
			_publicHostsContainer.SetActive(IsPublicShown);
			if (string.IsNullOrEmpty(_playerNameInput.text) || string.IsNullOrEmpty(_codeInput.text))
			{
				_connectButtonGrey.gameObject.SetActive(true);
				_connectButton.interactable = false;
			}
			else
			{
				_connectButtonGrey.gameObject.SetActive(false);
				_connectButton.interactable = true;
			}
		}

		#endregion

		#region Change Panels

		private void OpenMainMenu()
		{
			OpenPanel(_mainMenuGameObject, CurrentScreen.MainMenu);
		}

		private void OpenBeforeHost()
		{
			OpenPanel(_beforeHostGameObject, CurrentScreen.BeforeHost);

			UpdateBeforeHost();
		}

		private void OpenAfterHost()
		{
			SessionName = _sessionNameInput.text;
			Password = _passwordInputHost.text;
			if (string.IsNullOrEmpty(Password)) Password = "- None -";

			OpenPanel(_afterHostGameObject, CurrentScreen.AfterHost);

			UpdateAfterHost();

			_sessionNameText.text = SessionName;
			_codeText.text = Code;
			_passwordText.text = Password;
		}

		private void OpenBeforeClient()
		{
			OpenPanel(_beforeClientGameObject, CurrentScreen.BeforeClient);

			UpdateBeforeClient();
		}

		public void OpenAfterClient()
		{
			OpenPanel(_afterClientGameObject, CurrentScreen.AfterClient);
		}

		public void CloseMenu()
		{
			OpenPanel(null, CurrentScreen.None, true);
		}

		#endregion

		#region Methods

		private void OpenPanel(GameObject panel, CurrentScreen nextScreen, bool closeAll = false)
		{
			// Warning : only open from Change Panel methods 
			_mainMenuGameObject.SetActive(false);
			_beforeHostGameObject.SetActive(false);
			_afterHostGameObject.SetActive(false);
			_beforeClientGameObject.SetActive(false);
			_afterClientGameObject.SetActive(false);

			if (!closeAll)
			{
				panel.SetActive(true);
				_currentScreen = nextScreen;
			}
		}

		private void TogglePublicGames(bool toggle)
		{
			IsPublicShown = toggle;

			if (_currentScreen == CurrentScreen.BeforeHost)
			{
				// Hosting will be public
				return;
			}

			if (_currentScreen == CurrentScreen.BeforeClient)
			{
				_publicHostsContainer.SetActive(toggle);
				TogglePublicEvent?.Invoke(toggle);
				return;
			}

			Debug.LogWarning($"The public hosting just got toggled in a screen that shouldn't be able to do it ({_currentScreen}).)");
		}

		private void StartGame()
		{
			PlayGameEvent?.Invoke();
			CloseMenu();
		}

		public void QuitClientGame()
		{
			QuitGameEvent?.Invoke();

			OpenBeforeClient();
		}

		public void ToggleInputBlock(bool toggle)
		{
			_inputBlocker.SetActive(toggle);
		}

		#endregion

		#region Event Methods

		private void CallStartHostEvent()
		{
			StartHostEvent?.Invoke();
			OpenAfterHost();
		}

		private void CallJoinGameEvent()
		{
			PlayerName = _playerNameInput.text;
			Code = _codeInput.text;
			if (!string.IsNullOrEmpty(_passwordInputClient.text))
				Password = _passwordInputClient.text;

			JoinGameEvent?.Invoke();
		}

		private void CallUnhostEvent()
		{
			UnhostEvent?.Invoke();
			OpenBeforeHost();
		}

		private void CallCopyEvent()
		{
			CopyCodeEvent?.Invoke(Code);
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
		AfterClient
	}
}