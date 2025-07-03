using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using CardGame.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CardGame.Turns
{
    public class HUDResource : Resource
	{
		[SerializeField] private string _sceneName;
		public GameObject WaitingScreen;
		[SerializeField] private Button _nextTurnButton;
		[Header("Hud")]
		[SerializeField] private GameObject _hudScreen;
		[Header("WinScreen")]
		[SerializeField] private GameObject _winScreen;
		[SerializeField] private Button _winContinueButton;
		[Header("LooseScreen")]
		[SerializeField] private GameObject _looseScreen;
		[SerializeField] private Button _looseContinueButton;
		[Header("Score")]
		[SerializeField] private Transform _scoreContainer;
		[SerializeField] private ScoreUI _scorePrefab;
		
		private PlaceTileOnGridAbility _placeTileOnGrid;

		private List<ScoreUI> _scoreList = new();

		public override void Init(Controller owner)
		{
			OpenHud();
			_placeTileOnGrid = owner.GetStateComponent<PlaceTileOnGridAbility>();
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
				Debug.Log($"we have {manager.SoloNames.Count} players");
				for (int i = 0; i < manager.SoloNames.Count; i++)
				{
					ScoreUI playerScore = Object.Instantiate(_scorePrefab, _scoreContainer);
					playerScore.Setup(i, manager.SoloNames[i]);
					_scoreList.Add(playerScore);
				}
			}
		}
		
		private void NextTurn()
		{
			Debug.Log($"Plop, going to next turn");
			_placeTileOnGrid.CallEndTurn();
		}
		
		#region Panels

		public void OpenHud()
		{
			CloseAllScreens();
			_hudScreen.SetActive(true);
		}
		
		public void OpenWin()
		{
			CloseAllScreens();
			_winScreen.SetActive(true);
		}

		public void OpenLoose()
		{
			CloseAllScreens();
			_looseScreen.SetActive(true);
		}

		private void OpenLobby()
		{
			CloseAllScreens();

			Storage.Instance.GetElement<NetworkUI>().OpenMainMenu();
				
			SceneManager.UnloadSceneAsync(_sceneName);
		}

		private void CloseAllScreens()
		{
			_winScreen.SetActive(false);
			_looseScreen.SetActive(false);
			_hudScreen.SetActive(false);
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
	}
}