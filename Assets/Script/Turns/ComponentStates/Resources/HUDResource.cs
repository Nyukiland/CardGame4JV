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
		[SerializeField] private string _sceneName;
		[SerializeField] private GameObject _waitingScreen;
		[SerializeField] private GameObject _scoringScreen;
		[Header("Hud")]
		[SerializeField] private CanvasGroup _hudScreen;
		[Header("WinScreen")]
		[SerializeField] private GameObject _winScreen;
		[SerializeField] private Button _winContinueButton;
		[SerializeField] private TextMeshProUGUI _winScore;
		[Header("LooseScreen")]
		[SerializeField] private GameObject _looseScreen;
		[SerializeField] private Button _looseContinueButton;
		[SerializeField] private TextMeshProUGUI _looseScore;
		[Header("Score")]
		[SerializeField] private Transform _scoreContainer;
		[SerializeField] private ScoreUI _scorePrefab;
		[Header("FlagCounter")]
		[SerializeField] private Image _firstCircle;
		[SerializeField] private Image _secondCircle;
		[SerializeField] private Image _flag;
		[Header("Next Turn Button")]
		[SerializeField] private TextMeshProUGUI _turnCounter;
		[SerializeField] private Slider _nextTurnSlider;
		[SerializeField] private Button _nextTurnButton;
		[SerializeField] private Image _nextTurnFillImage;
		[SerializeField] private Image _greyFilter;
		[SerializeField] private Color _startSliderColor;
		[SerializeField] private Color _endSliderColor;

		private PlaceTileOnGridAbility _placeTileOnGrid;

		private readonly List<ScoreUI> _scoreList = new();

		private bool _isHudOpen;
		private Tween _lastHudTween;

		#region Unity Methods

		public override void Init(Controller owner)
		{
			OpenHud();
			_placeTileOnGrid = owner.GetStateComponent<PlaceTileOnGridAbility>();
			_nextTurnSlider.maxValue = _placeTileOnGrid.MaxTimeTurn;
			_greyFilter.gameObject.SetActive(false);
		}

		public override void OnEnable()
		{
			_winContinueButton.onClick.AddListener(OpenLobby);
			_looseContinueButton.onClick.AddListener(OpenLobby);
			_nextTurnButton.onClick.AddListener(NextTurn);
			GameManager.Instance.ScoreEvent += UpdateScore;
		}
		public override void OnDisable()
		{
			_winContinueButton.onClick.RemoveListener(OpenLobby);
			_looseContinueButton.onClick.RemoveListener(OpenLobby);
			_nextTurnButton.onClick.RemoveListener(NextTurn);
			GameManager.Instance.ScoreEvent -= UpdateScore;
		}

		public override void Update(float deltaTime)
		{
			_nextTurnSlider.value = _placeTileOnGrid.Timer;

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

		#endregion

		public void InitScores()
		{
			GameManager manager = GameManager.Instance;

			if (manager.IsNetCurrentlyActive())
			{
				Debug.Log($"we have {manager.OnlinePlayersID.Count} players");
				for (int i = 0; i < manager.OnlinePlayersID.Count; i++)
				{
					ScoreUI playerScore = Object.Instantiate(_scorePrefab, _scoreContainer);
					playerScore.Setup(i, manager.OnlinePlayersID[i].ToString());
					_scoreList.Add(playerScore);
				}
			}
			else
			{
				for (int i = 0; i < manager.SoloNames.Count; i++)
				{
					ScoreUI playerScore = Object.Instantiate(_scorePrefab, _scoreContainer);
					playerScore.Setup(i, manager.SoloNames[i]);
					_scoreList.Add(playerScore);
				}
			}
		}

		public void UpdateTurnValue()
		{
			_turnCounter.text = $"Turn : {GameManager.Instance.GlobalTurn.ToString()}";
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

			int turn = GameManager.Instance.LocalPlayerTurn % 3;

			switch (turn)
			{
				case 0:
					_flag.enabled = true;
					_flag.transform.DOMoveX(_secondCircle.transform.position.x, 1f).From();
					break;
				case -1:
				case 1:
					_firstCircle.enabled = true;
					_firstCircle.transform.DOMoveX(_flag.transform.position.x, 1f).From();
					break;
				default:
					_secondCircle.enabled = true;
					_secondCircle.transform.DOMoveX(_firstCircle.transform.position.x, 1f).From();
					break;
			}
		}

		#region Panels

		public void OpenHud()
		{
			if (_isHudOpen)
				return;

			_isHudOpen = true;
			CloseAllScreens();
			_hudScreen.gameObject.SetActive(true);
			_hudScreen.alpha = 0f;
			_lastHudTween.Kill();
			_lastHudTween = DOTween.To(() => _hudScreen.alpha, x => _hudScreen.alpha = x, 1f, 0.5f).SetEase(Ease.InExpo);
		}

		public void CloseHud()
		{
			if (!_isHudOpen)
				return;

			_isHudOpen = false;
			_hudScreen.alpha = 1f;
			_lastHudTween.Kill();
			_lastHudTween = DOTween.To(() => _hudScreen.alpha, x => _hudScreen.alpha = x, 0f, 0.5f).OnComplete(CloseAllScreens);
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

			Storage.Instance.GetElement<NetworkUI>().OpenMainMenu().Forget();

			SceneManager.UnloadSceneAsync(_sceneName);
		}

		public void OpenWaitingScreen() => _waitingScreen.SetActive(true);
		public void CloseWaitingScreen() => _waitingScreen.SetActive(false);

		public void OpenScoringScreen() => _scoringScreen.SetActive(true);
		public void CloseScoringScreen() => _scoringScreen.SetActive(false);

		private void CloseAllScreens()
		{
			_winScreen.SetActive(false);
			_looseScreen.SetActive(false);
			_hudScreen.gameObject.SetActive(false);
			//Temporairement retiré pcq il réapparaissait pas
			//_waitingScreen.SetActive(false); 
			_scoringScreen.SetActive(false);
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
			if (RectTransformUtility.RectangleContainsScreenPoint(_nextTurnButton.GetComponent<RectTransform>(), pos)) 
				return true;
			//ajouter autre element UI au besoin 
			//voili voilou

			return false;
		}
	}
}