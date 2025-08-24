using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CardGame.Turns
{
	public class HUDResource : Resource
	{
		[SerializeField]
		private Canvas _canvas;
		[SerializeField] private float _distanceToCamera = 10;
		[Space(10)]

		[SerializeField] private GameObject _loadingScreen;
		[Space(10)]
		[Header("Hud")]
		[SerializeField] private CanvasGroup _hudScreen;
		[Space(10)]
		[Header("WinScreen")]
		[SerializeField] private GameObject _winScreen;
		[SerializeField] private Button _winContinueButton;
		[SerializeField] private TextMeshProUGUI _winScore;
		[Space(10)]
		[Header("LooseScreen")]
		[SerializeField] private GameObject _looseScreen;
		[SerializeField] private Button _looseContinueButton;
		[SerializeField] private TextMeshProUGUI _looseScore;
		[Space(10)]
		[Header("Score")]
		[SerializeField] private Transform _scoreContainer;
		[SerializeField] private ScoreUI _scorePrefab;
		[Space(10)]
		[Header("FlagCounter")]
		[SerializeField] private Image _actualArrow;
		[SerializeField] private Image _firstCircle;
		[SerializeField] private Image _secondCircle;
		[SerializeField] private Image _flag;
		[Space(10)]
		[Header("Next Turn Button")]
		[SerializeField] private TextMeshProUGUI _turnCounter;
		[SerializeField] private Slider _nextTurnSlider;
		[SerializeField] private Button _nextTurnButton;
		[SerializeField] private Image _nextTurnFillImage;
		[SerializeField] private Color _startSliderColor;
		[SerializeField] private Color _endSliderColor;
		[Space(10)]
		[Header("Taunt")]
		[SerializeField] private Transform _tauntVisualContainer;
		[SerializeField] private Transform _tauntButtonParent;
		[SerializeField] private Button _tauntButtonPrefab;
		[SerializeField] private Button _tauntOpenMenu;
		[SerializeField] private ShowPopUpInfo _playerTaunt;
		[SerializeField] private ShowPopUpInfo _enemyTaunt;
		private List<Button> _tauntButtonsList = new();
		private List<TextMeshProUGUI> _tauntTMPList = new();
		[SerializeField] private Transform _tauntInTransform;
		[SerializeField] private Transform _tauntOutTransform;
		[Space(10)]
		[Header("Pause")]
		[SerializeField] private GameObject _pauseScreen;
		[SerializeField] private Button _pauseButton;
		[SerializeField] private Button _pausePlayButton;
		[SerializeField] private Button _pauseQuitButton;
		[Space(10)]
		[Header("Turn info")]
		[SerializeField] private Image _blurImage;
		[SerializeField] private Color _playerTurnColor;
		[SerializeField] private Color _otherTurnColor;
		[SerializeField] private Color _discardTurnColor;
		[SerializeField] private Color _scoringTurnColor;
		[SerializeField] private CanvasGroup _waitingScreen;
		[SerializeField] private CanvasGroup _scoringScreen;
		[Space(10)]
		[Header("Scene")]
		public string SceneName;

		private PlaceTileOnGridAbility _placeTileOnGrid;
		private ZoneHolderResource _zoneHolder;
		private NetworkResource _networkResource;

		private readonly List<ScoreUI> _scoreList = new();

		private bool _isHudOpen;
		private Tween _lastHudTween;

		private TauntButtonAbility _tauntAbility;
		private bool _tauntOpen;

		#region Unity Methods

		public override void EarlyInit()
		{
			base.EarlyInit();

			_canvas.worldCamera = Camera.main;
			_canvas.planeDistance = _distanceToCamera;
			Canvas.ForceUpdateCanvases();
			_loadingScreen.SetActive(true);
		}

		public override void Init(Controller owner)
		{
			OpenHud();
			_placeTileOnGrid = owner.GetStateComponent<PlaceTileOnGridAbility>();
			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
			_networkResource = owner.GetStateComponent<NetworkResource>();

			_nextTurnSlider.maxValue = _placeTileOnGrid.MaxTimeTurn;
			_waitingScreen.alpha = 0;
			_scoringScreen.alpha = 0;

			_tauntVisualContainer.position = _tauntOutTransform.position;
		}

		public override void OnEnable()
		{
			_winContinueButton.onClick.AddListener(OpenLobby);
			_looseContinueButton.onClick.AddListener(OpenLobby);
			_nextTurnButton.onClick.AddListener(NextTurn);
			GameManager.Instance.ScoreEvent += UpdateScore;
			_pauseButton.onClick.AddListener(OpenPauseScreen);
			_pausePlayButton.onClick.AddListener(ClosePauseScreen);
			_pauseQuitButton.onClick.AddListener(OpenLobby);
			_tauntOpenMenu.onClick.AddListener(() => OpenTauntMenu(true));

			InitTaunt();
		}
		public override void OnDisable()
		{
			_winContinueButton.onClick.RemoveListener(OpenLobby);
			_looseContinueButton.onClick.RemoveListener(OpenLobby);
			_nextTurnButton.onClick.RemoveListener(NextTurn);
			GameManager.Instance.ScoreEvent -= UpdateScore;
			_pauseButton.onClick.RemoveListener(OpenPauseScreen);
			_pausePlayButton.onClick.RemoveListener(ClosePauseScreen);
			_pauseQuitButton.onClick.RemoveListener(OpenLobby);
			_tauntOpenMenu.onClick.RemoveListener(() => OpenTauntMenu(true));

			if (_tauntButtonsList.Count != _tauntTMPList.Count) return;
			for (int i = 0; i < _tauntButtonsList.Count; i++)
			{
				int index = i;
				_tauntButtonsList[i].onClick.RemoveListener(() =>
				{
					_tauntAbility.CallEvent(_tauntAbility.Taunts[index]);
					OpenTauntMenu(false);
				});
			}
		}

		public override void Update(float deltaTime)
		{
			//using a slider for this is cursed
			//image fill with a 0 to 1 value would have been better
			_nextTurnSlider.value = _placeTileOnGrid.Timer;
			_nextTurnSlider.maxValue = _placeTileOnGrid.MaxTimeTurn;

			float percent = _nextTurnSlider.value / _nextTurnSlider.maxValue;

			if (percent > 0.8f)
			{
				if (_nextTurnFillImage.color != _endSliderColor)
					_nextTurnFillImage.color = _endSliderColor;
			}
			else
			{
				if (_nextTurnFillImage.color != _startSliderColor)
					_nextTurnFillImage.color = _startSliderColor;
			}
		}

		private void InitTaunt()
		{
			_tauntAbility = Owner.GetStateComponent<TauntButtonAbility>();
			List<TauntScriptableObject> tauntList = _tauntAbility.Taunts;

			for (int i = 0; i < tauntList.Count; i++)
			{
				int index = i;
				Button tauntButton = Object.Instantiate(_tauntButtonPrefab, _tauntButtonParent);
				_tauntButtonsList.Add(tauntButton);
				TextMeshProUGUI tauntText = tauntButton.GetComponentInChildren<TextMeshProUGUI>();
				_tauntTMPList.Add(tauntText);
				tauntText.text = tauntList[i].Text;
				tauntButton.onClick.AddListener(() =>
				{
					_tauntAbility.CallEvent(_tauntAbility.Taunts[index]);
					OpenTauntMenu(false);
				});
			}

			_playerTaunt.HidePopUp();
			_enemyTaunt.HidePopUp();
		}

		#endregion

		#region Panels

		public void CloseLoadingScreen() => _loadingScreen.SetActive(false);

		public void OpenHud()
		{
			if (_isHudOpen)
				return;

			_isHudOpen = true;
			CloseAllScreens();
			_hudScreen.gameObject.SetActive(true);
			_hudScreen.alpha = 1f;
			// _lastHudTween.Kill();
			// _lastHudTween = DOTween.To(() => _hudScreen.alpha, x => _hudScreen.alpha = x, 1f, 0.5f).SetEase(Ease.InExpo);
		}

		public void CloseHud()
		{
			if (!_isHudOpen)
				return;

			_isHudOpen = false;
			_hudScreen.alpha = 0f;
			// _lastHudTween.Kill();
			// _lastHudTween = DOTween.To(() => _hudScreen.alpha, x => _hudScreen.alpha = x, 0f, 0.5f).OnComplete(CloseAllScreens);
		}

		public void OpenWin()
		{
			CloseAllScreens();
			_winScreen.SetActive(true);
			_winScore.text = $"Your score : {GameManager.Instance.PlayerScore}\n Their score : {GameManager.Instance.EnemyScore}";
		}

		public void OpenLoose()
		{
			CloseAllScreens();
			_looseScreen.SetActive(true);
			_looseScore.text = $"Your score : {GameManager.Instance.PlayerScore}\n Their score : {GameManager.Instance.EnemyScore}";
		}

		private void OpenLobby()
		{
			CloseAllScreens();

			GameManager.Instance.ResetManager();

			Time.timeScale = 1;

			Storage.Instance.GetElement<NetworkUI>().OpenMainMenu().Forget();

			SceneManager.UnloadSceneAsync(SceneName);
		}

		public void OpenPauseScreen()
		{
			CloseAllScreens();
			_pauseScreen.SetActive(true);
			_zoneHolder.HideMyHand(true);

			if (!_networkResource.IsNetActive())
				Time.timeScale = 0f;
		}

		public void ClosePauseScreen()
		{
			CloseAllScreens();
			OpenHud();
			_zoneHolder.HideMyHand(false);
			Time.timeScale = 1f;
		}

		private void CloseAllScreens()
		{
			_winScreen.SetActive(false);
			_looseScreen.SetActive(false);
			_hudScreen.gameObject.SetActive(false);
			_pauseScreen.gameObject.SetActive(false);
			//Temporairement retir� pcq il r�apparaissait pas
			//_waitingScreen.SetActive(false); 
			_scoringScreen.alpha = 0;
		}

		#endregion

		private void UpdateScore(int playerIndex, float score)
		{
			foreach (ScoreUI scoreUI in _scoreList)
			{
				if (scoreUI.PlayerIndex != playerIndex) continue;
				scoreUI.SetScore(score);
			}
		}

		public bool AmIClickingOnUI(Vector2 pos)
		{
			//taunt stuff
			if (_tauntOpen)
			{
				if (!RectTransformUtility.RectangleContainsScreenPoint(_tauntVisualContainer.GetComponent<RectTransform>(), pos, Camera.main))
				{
					OpenTauntMenu(false);
				}
			}

			//add exception for pause menu
			if (_pauseScreen.activeSelf == true)
				return true;

			if (RectTransformUtility.RectangleContainsScreenPoint(_nextTurnButton.GetComponent<RectTransform>(), pos, Camera.main))
				return true;
			else if (RectTransformUtility.RectangleContainsScreenPoint(_pauseButton.GetComponent<RectTransform>(), pos, Camera.main))
				return true;
			else if (RectTransformUtility.RectangleContainsScreenPoint(_tauntOpenMenu.GetComponent<RectTransform>(), pos, Camera.main))
				return true;

			foreach (Button button in _tauntButtonsList)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), pos, Camera.main))
					return true;
			}
			//ajouter autre element UI au besoin 
			//voili voilou

			return false;
		}

		public void SendTaunt(string tauntLine, bool self = true)
		{
			if (self) _playerTaunt.ShowPopUp(tauntLine).Forget();
			else _enemyTaunt.ShowPopUp(tauntLine).Forget();
		}

		public void SendTaunt(Sprite[] anim, float timer, bool self = true)
		{
			if (self) _playerTaunt.ShowPopUp(anim, timer).Forget();
			else _enemyTaunt.ShowPopUp(anim, timer).Forget();
		}

		private void OpenTauntMenu(bool open)
		{
			if (_tauntOpen == open)
				return;

			_tauntOpen = open;
			_tauntVisualContainer.DOKill();

			Vector3 posToGo = open ? _tauntInTransform.position : _tauntOutTransform.position;
			posToGo = _tauntVisualContainer.parent.InverseTransformPoint(posToGo);

			_tauntVisualContainer.DOLocalMove(posToGo, 0.5f).SetEase(Ease.InOutSine);
		}

		public void InitScores()
		{
			GameManager manager = GameManager.Instance;

			if (manager.IsNetCurrentlyActive())
			{
				Debug.Log($"we have {manager.OnlinePlayersID.Count} players");
				for (int i = 0; i < manager.OnlinePlayersID.Count; i++)
				{
					ScoreUI playerScore = Object.Instantiate(_scorePrefab, _scoreContainer);
					if (manager.OnlinePlayersID[i] == GameManager.Instance.OwnerClientId)
						playerScore.Setup(i, true);
					else
						playerScore.Setup(i, false);

					_scoreList.Add(playerScore);
				}
			}
			else
			{
				for (int i = 0; i < manager.SoloNames.Count; i++)
				{
					ScoreUI playerScore = Object.Instantiate(_scorePrefab, _scoreContainer);
					playerScore.Setup(i, i == 0);
					_scoreList.Add(playerScore);
				}
			}
		}

		public void UpdateTurnValue()
		{
			int round = (GameManager.Instance.LocalPlayerTurn - 1) / 3 + 1;
			_turnCounter.text = $"{round}/4";
		}

		public void ToggleNextTurnButton(bool toggle)
		{
			_nextTurnButton.gameObject.SetActive(toggle);
		}

		private void NextTurn()
		{
			_placeTileOnGrid.CallEndTurn();
		}

		public void UpdateFlag()
		{
			_firstCircle.enabled = false;
			_secondCircle.enabled = false;
			_flag.enabled = false;
			_actualArrow.enabled = true;

			int turn = GameManager.Instance.LocalPlayerTurn % 3;
			Vector3 posToGo = Vector3.zero;

			switch (turn)
			{
				case 0:
					posToGo = _flag.transform.position;
					break;
				case 1:
					posToGo = _firstCircle.transform.position;
					break;
				case 2:
					posToGo = _secondCircle.transform.position;
					break;
			}

			posToGo = _actualArrow.transform.parent.InverseTransformPoint(posToGo);

			_actualArrow.transform.DOLocalMove(posToGo, 0.5f).SetEase(Ease.InOutSine);
		}

		public void ChangeTurnFeedback(TurnState turnState)
		{
			_blurImage.DOKill();
			_scoringScreen.DOKill();
			_waitingScreen.DOKill();

			Color colorToGo = Color.white;
			CanvasGroup toLerp = null;

			switch (turnState)
			{
				case TurnState.Playing:
					colorToGo = _playerTurnColor;
					_scoreList.ForEach(x => x.IsMyTurn(true));
					break;
				case TurnState.Discard:
					colorToGo = _discardTurnColor;
					break;
				case TurnState.Scoring:
					colorToGo = _scoringTurnColor;
					toLerp = _scoringScreen;
					break;
				case TurnState.OtherPlayer:
					colorToGo = _otherTurnColor;
					toLerp = _waitingScreen;
					_scoreList.ForEach(x => x.IsMyTurn(false));
					break;
			}

			if (toLerp != _scoringScreen)
				_scoringScreen.DOFade(0, 0.1f);
			if (toLerp != _waitingScreen)
				_waitingScreen.DOFade(0, 0.1f);

			toLerp.DOFade(1, 0.5f);


			_blurImage.DOColor(colorToGo, 0.5f);
		}

		public enum TurnState
		{
			Playing,
			Discard,
			Scoring,
			OtherPlayer,
		}
	}
}