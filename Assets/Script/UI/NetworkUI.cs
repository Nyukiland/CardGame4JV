using System.Collections.Generic;
using CardGame.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CardGame.UI
{
	public class NetworkUI : MonoBehaviour
	{
		#region Variables

		[SerializeField] private GameObject _inputBlocker;
		[SerializeField] private CanvasGroup _transitionScreenCanvasGroup;
		[SerializeField] private Image _backgroundImage;
		
		[Header("SceneManagement")]
		[SerializeField] private string _sceneName;
		
		[Header("MainMenu")]
		[SerializeField] private GameObject _mainMenuGameObject;
		[SerializeField] private Button _mainHostButton;
		[SerializeField] private RectTransform _mainHostRectTransform;
		[SerializeField] private Button _mainConnectButton;
		[SerializeField] private RectTransform _mainConnectRectTransform;
		[SerializeField] private Button _mainSoloButton;
		[SerializeField] private RectTransform _mainSoloRectTransform;

		[Header("BeforeHost")]
		[SerializeField] private GameObject _beforeHostGameObject;
		[SerializeField] private Button _hostDistantButton;
		[SerializeField] private Image _hostDistantButtonImage;
		[SerializeField] private Button _hostLocalButton;
		[SerializeField] private Image _hostLocalButtonImage;
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
		[SerializeField] private Button _clientDistantButton;
		[SerializeField] private Image _clientDistantButtonImage;
		[SerializeField] private Button _clientLocalButton;
		[SerializeField] private Image _clientLocalButtonImage;
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

		[Header("PopUp")]
		[SerializeField] private GameObject _popUpContainer;
		[SerializeField] private RectTransform _popUpRectTransform;
		[SerializeField] private TextMeshProUGUI _popUpTMP;

		// public variables
		public GameObject PublicHostsContainer => _publicHostsContainer;
		public const string NONE_BASE_VALUE = "- None -";
		public string Code { get; set; } = "No code found";
		public string Password { get; set; } = "No password found";
		public string SessionName { get; set; } = "No session name found";
		public bool IsPublicShown { get; private set; } = true;

		private List<Tween> _currentMenuTweens = new List<Tween>();
		private List<Vector3> _currentMenuPosition = new List<Vector3>();
		private List<RectTransform> _currentMenuRectTransform = new List<RectTransform>();

		// private variables
		private CurrentScreen _currentScreen;
		private bool _isDistant = true;

		// Events
		public StringEvent CopyCodeEvent;
		public BoolEvent TogglePublicEvent;
		public BoolEvent ToggleDistantEvent;
		public SingleEvent StartHostEvent;
		public SingleEvent JoinGameEvent;
		public SingleEvent UnhostEvent;
		public SingleEvent QuitGameEvent;
		public SingleEvent PlayGameEvent;

		// Delegates
		public delegate void SingleEvent();
		public delegate void StringEvent(string stringEvent);
		public delegate void BoolEvent(bool boolEvent);

		#endregion

		#region Unity Methods

		private void Start()
		{
			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			_popUpContainer.SetActive(false);
		}

		private void Awake()
		{
			Storage.Instance.Register(this);

			// Buttons
			_mainHostButton.onClick.AddListener(() => OpenBeforeHost().Forget());
			_mainConnectButton.onClick.AddListener(() => OpenBeforeClient().Forget());
			_mainSoloButton.onClick.AddListener(StartSoloGame);
			_hostButton.onClick.AddListener(CallStartHostEvent);
			_hostBackButton.onClick.AddListener(() => OpenMainMenu().Forget());
			_copyCodeButton.onClick.AddListener(CallCopyEvent);
			_unHostButton.onClick.AddListener(CallUnhostEvent);
			_playButton.onClick.AddListener(StartMultiGame);
			_connectButton.onClick.AddListener(CallJoinGameEvent);
			_quitGameButton.onClick.AddListener(QuitClientGame);
			_clientBackButton.onClick.AddListener(() => OpenMainMenu().Forget());
			_hostDistantButton.onClick.AddListener(() => ToggleDistant(true));
			_hostLocalButton.onClick.AddListener(() => ToggleDistant(false));
			_clientDistantButton.onClick.AddListener(() => ToggleDistant(true));
			_clientLocalButton.onClick.AddListener(() => ToggleDistant(false));

			// Inputs fields
			_sessionNameInput.onEndEdit.AddListener(UpdateHostInputs);
			_passwordInputHost.onEndEdit.AddListener(UpdateHostInputs);
			_codeInput.onEndEdit.AddListener(UpdateClientInputs);
			//_codeInput.contentType = TMP_InputField.ContentType.Alphanumeric;
			//_codeInput.onValidateInput += delegate (string s, int i, char c) { return char.ToUpper(c); };
			_passwordInputClient.onEndEdit.AddListener(UpdateClientInputs);

			// Toggles
			_publicHostToggle.onValueChanged.AddListener(TogglePublicGames);
			_publicFindToggle.onValueChanged.AddListener(TogglePublicGames);

			_transitionScreenCanvasGroup.alpha = 1f;

			OpenMainMenu().Forget();
		}

		private void OnDestroy()
		{
			// Buttons
			_mainHostButton.onClick.RemoveListener(() => OpenBeforeHost().Forget());
			_mainConnectButton.onClick.RemoveListener(() => OpenBeforeClient().Forget());
			_mainSoloButton.onClick.RemoveListener(StartSoloGame);
			_hostButton.onClick.RemoveListener(CallStartHostEvent);
			_hostBackButton.onClick.RemoveListener(() => OpenMainMenu().Forget());
			_copyCodeButton.onClick.RemoveListener(CallCopyEvent);
			_unHostButton.onClick.RemoveListener(CallUnhostEvent);
			_playButton.onClick.RemoveListener(StartMultiGame);
			_connectButton.onClick.RemoveListener(CallJoinGameEvent);
			_quitGameButton.onClick.RemoveListener(QuitClientGame);
			_clientBackButton.onClick.RemoveListener(() => OpenMainMenu().Forget());
			_hostDistantButton.onClick.RemoveListener(() => ToggleDistant(true));
			_hostLocalButton.onClick.RemoveListener(() => ToggleDistant(false));
			_clientDistantButton.onClick.RemoveListener(() => ToggleDistant(true));
			_clientLocalButton.onClick.RemoveListener(() => ToggleDistant(false));

			// Inputs fields
			_sessionNameInput.onEndEdit.RemoveListener(UpdateHostInputs);
			_passwordInputHost.onEndEdit.RemoveListener(UpdateHostInputs);
			_codeInput.onEndEdit.RemoveListener(UpdateClientInputs);
			_passwordInputClient.onEndEdit.RemoveListener(UpdateClientInputs);

			// Toggles
			_publicHostToggle.onValueChanged.RemoveListener(TogglePublicGames);
			_publicFindToggle.onValueChanged.RemoveListener(TogglePublicGames);
		}

		#endregion

		#region Update Visuals

		private void UpdateHostInputs(string inputString)
		{
			UpdateBeforeHost();
		}

		private void UpdateClientInputs(string inputString)
		{
			UpdateBeforeClient();
		}

		private UniTask UpdateBeforeHost()
		{
			ToggleDistant(_isDistant);
			TogglePublicGames(IsPublicShown);

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
			
			return UniTask.CompletedTask;
		}

		public UniTask UpdateAfterHost()
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
			
			return UniTask.CompletedTask;
		}

		private void UpdateBeforeClient()
		{
			ToggleDistant(_isDistant);
			TogglePublicGames(IsPublicShown);

			_publicHostsContainer.SetActive(IsPublicShown);
			if (string.IsNullOrEmpty(_codeInput.text))
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

		private void ToggleDistant(bool isDistant)
		{
			_isDistant = isDistant;
			ToggleDistantEvent?.Invoke(isDistant);

			if (isDistant)
			{
				ToggleButtons(_hostDistantButtonImage, _hostLocalButtonImage);
				ToggleButtons(_clientDistantButtonImage, _clientLocalButtonImage);
			}
			else
			{
				ToggleButtons(_hostLocalButtonImage, _hostDistantButtonImage);
				ToggleButtons(_clientLocalButtonImage, _clientDistantButtonImage);
			}
		}

		private void ToggleButtons(Image onButtonImage, Image offButtonImage)
		{
			onButtonImage.color = Color.green;
			offButtonImage.color = Color.white;
		}

		#endregion

		#region Change Panels

		public async UniTask OpenMainMenu()
		{
			await OpenPanel(_mainMenuGameObject, CurrentScreen.MainMenu);
			Sequence mainMenuSequence = DOTween.Sequence();
			mainMenuSequence.Join(await SaveTween(_mainHostRectTransform, -500f, 0f));
			mainMenuSequence.Join(await SaveTween(_mainConnectRectTransform, -500f, 0f, 1.5f));
			mainMenuSequence.Join(await SaveTween(_mainSoloRectTransform, -500f, 0f, 2f));
			mainMenuSequence.Play();
		}

		private async UniTask OpenBeforeHost()
		{
			await OpenPanel(_beforeHostGameObject, CurrentScreen.BeforeHost);

			UpdateBeforeHost();
		}

		private async UniTask OpenAfterHost()
		{
			SessionName = _sessionNameInput.text;
			Password = _passwordInputHost.text;
			if (string.IsNullOrEmpty(Password)) Password = NONE_BASE_VALUE;

			await OpenPanel(_afterHostGameObject, CurrentScreen.AfterHost);

			UpdateAfterHost();

			_sessionNameText.text = SessionName;
			_codeText.text = Code;
			_passwordText.text = Password;

			_inputBlocker.SetActive(true);
		}

		public async UniTask OpenBeforeClient()
		{
			await OpenPanel(_beforeClientGameObject, CurrentScreen.BeforeClient);

			UpdateBeforeClient();
		}

		public async UniTask OpenAfterClient()
		{
			await OpenPanel(_afterClientGameObject, CurrentScreen.AfterClient);
		}

		public void CloseMenu()
		{
			ClosePrivateMenu().Forget();
		}
		private async UniTask ClosePrivateMenu()
		{
			await OpenPanel(null, CurrentScreen.None, true);
			_backgroundImage.gameObject.SetActive(false);
		}

		private async UniTask<Tween> SaveTween(RectTransform target, float xOffset, float yOffset, float duration = 0, bool reverse = false)
		{
			if (duration == 0) duration = 1f;

			Canvas.ForceUpdateCanvases();
			await UniTask.WaitForFixedUpdate();

			Vector2 fromPosition = new(target.anchoredPosition.x + xOffset, target.anchoredPosition.y + yOffset);
			Tween currentTween = reverse ? target.DOAnchorPos(fromPosition, duration) : target.DOAnchorPos(fromPosition, duration).From();

			_currentMenuTweens.Add(currentTween);
			_currentMenuPosition.Add(fromPosition);
			_currentMenuRectTransform.Add(target);

			return currentTween;
		}

		private void StopTweens()
		{
			// foreach (Tween tween in _currentMenuTweens)
			// {
			// 	tween.Kill();
			// }
			// _currentMenuTweens.Clear();
			//
			// for (int i = 0; i < _currentMenuPosition.Count; i++)
			// {
			// 	_currentMenuRectTransform[i].anchoredPosition = _currentMenuPosition[i];
			// }
			// _currentMenuPosition.Clear();
			// _currentMenuRectTransform.Clear();
		}

		#endregion

		#region Methods

		private async UniTask OpenPanel(GameObject panel, CurrentScreen nextScreen, bool closeAll = false)
		{
			StopTweens();
			bool fadeIn = false;
			_backgroundImage.gameObject.SetActive(true);

			Sequence fadeSequence = DOTween.Sequence();
			if (_transitionScreenCanvasGroup.alpha < 0.5f)
			{
				fadeIn = true;
				fadeSequence.Append(DOTween.To(() => _transitionScreenCanvasGroup.alpha,
					x => _transitionScreenCanvasGroup.alpha = x, 1f, 0.3f).OnComplete(ShowPanel));
			}
			
			fadeSequence.Append(DOTween.To(() => _transitionScreenCanvasGroup.alpha, x => _transitionScreenCanvasGroup.alpha = x, 0f, 0.3f));
			fadeSequence.Play();
			
			// await fadeSequence.AsyncWaitForCompletion();

			// Warning : only open from Change Panel methods 
			_mainMenuGameObject.SetActive(false);
			_beforeHostGameObject.SetActive(false);
			_afterHostGameObject.SetActive(false);
			_beforeClientGameObject.SetActive(false);
			_afterClientGameObject.SetActive(false);

			if (!fadeIn)
				ShowPanel();

			void ShowPanel()
			{
				if (closeAll)
					return;

				panel.SetActive(true);
				_currentScreen = nextScreen;
			}
		}

		public void UpdateCodeAfterHost()
		{
			_codeText.text = Code;
			_inputBlocker.SetActive(false);
		}

		private void TogglePublicGames(bool toggle)
		{
			IsPublicShown = toggle;

			if (_currentScreen == CurrentScreen.BeforeHost)
			{
				// Hosting will be public
				_publicHostToggle.isOn = IsPublicShown;
				return;
			}

			if (_currentScreen == CurrentScreen.BeforeClient)
			{
				_publicHostsContainer.SetActive(IsPublicShown);
				TogglePublicEvent?.Invoke(IsPublicShown);
				
				_publicFindToggle.isOn = IsPublicShown;
				return;
			}

			Debug.LogWarning($"The public hosting just got toggled in a screen that shouldn't be able to do it ({_currentScreen}).)");
		}

		private void StartMultiGame()
		{
			PlayGameEvent?.Invoke();
			_backgroundImage.gameObject.SetActive(false);
		}

		private void StartSoloGame()
		{
			CloseMenu();

			if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
			{
				SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
			}
		}

		public void QuitClientGame()
		{
			QuitGameEvent?.Invoke();

			OpenBeforeClient().Forget();
		}

		public void ToggleInputBlock(bool toggle)
		{
			_inputBlocker.SetActive(toggle);
		}

		public async UniTask SpawnPopUp(string text, float duration)
		{
			_popUpTMP.text = text;
			_popUpContainer.SetActive(true);
			StopTweens();

			// Sequence popUpSequence = DOTween.Sequence();
			// popUpSequence.Append(await SaveTween(_popUpRectTransform, 0f, -100f));
			// popUpSequence.Append(await SaveTween(_popUpRectTransform, 0f, 100f, 1.5f, true));
			// popUpSequence.Play();

			await UniTask.WaitForSeconds(duration);
			_popUpContainer.SetActive(false);
		}

		#endregion

		#region Event Methods

		private void CallStartHostEvent()
		{
			StartHostEvent?.Invoke();
			OpenAfterHost().Forget();
		}

		private void CallJoinGameEvent()
		{
			Code = _codeInput.text;
			if (!string.IsNullOrEmpty(_passwordInputClient.text))
				Password = _passwordInputClient.text;

			JoinGameEvent?.Invoke();
			OpenAfterClient().Forget();
		}

		private void CallUnhostEvent()
		{
			UnhostEvent?.Invoke();
			OpenBeforeHost().Forget();
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