using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class GameManager : NetworkBehaviour, ISelectableInfo
{
	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameManager instance = FindFirstObjectByType<GameManager>();
				if (instance == null)
				{
					instance = new GameObject(typeof(GameManager).Name).AddComponent<GameManager>();
					instance.gameObject.AddComponent<NetworkObject>();
				}
				_instance = instance;
			}

			return _instance;

		}
		private set { _instance = value; }
	}

	#region Net

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		OnlineScores.OnListChanged += OnOnlineScoresChanged;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		OnlineScores.OnListChanged -= OnOnlineScoresChanged;
		_instance = null;
	}

	public NetworkVariable<int> OnlineTurns =
		new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<int> OnlineScores =
		new(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<ulong> OnlinePlayersID
		= new(new List<ulong>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<FixedString64Bytes> OnlinePlayersNameIdentification
		= new(new List<FixedString64Bytes>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public void SetLocalPlayerInfo()
	{
		HoldForLocalChange(NetworkManager.Singleton.LocalClientId).Forget();
	}

	private async UniTask HoldForLocalChange(ulong owner)
	{
		await UniTask.WaitUntil(() => OnlinePlayersID.IndexOf(owner) != -1);
		PlayerIndex = OnlinePlayersID.IndexOf(owner);
	}

	#endregion

	#region Solo

	public int SoloTurns = 1;
	public List<int> SoloScores = new();
	public List<string> SoloNames = new();

	#endregion

	#region Variables

	public int PlayerIndex
	{
		get;
		private set;
	} = -1;

	public int PlayerIndexTurn
	{
		get
		{
			if (OnlinePlayersID.Count == 0) return -1;
			return OnlineTurns.Value % OnlinePlayersID.Count;
		}
	}

	public string PlayerName
	{
		get
		{
			if (IsNetCurrentlyActive())
			{
				if (PlayerIndex == -1 || OnlinePlayersNameIdentification.Count == 0) return "none";
				return OnlinePlayersNameIdentification[PlayerIndex].ToString();
			}
			else
			{
				if (SoloNames.Count == 0)
					return "none";

				return SoloNames[0];
			}
		}
	}

	public int GlobalTurn
	{
		get
		{
			if (IsNetCurrentlyActive()) return OnlineTurns.Value;
			else return SoloTurns;
		}
	}

	public int LocalPlayerTurn
	{
		get
		{
			if (IsNetCurrentlyActive())
			{
				if (OnlinePlayersID.Count == 0) return -1;
				return Mathf.CeilToInt((float)(OnlineTurns.Value + 1) / OnlinePlayersID.Count);
			}
			else
			{
				if (SoloNames.Count == 0) return -1;
				return Mathf.CeilToInt((float)(SoloTurns + 1) / SoloNames.Count);
			}
		}
	}

	public bool FlagTurn
	{
		get
		{
			if (LocalPlayerTurn == -1) return false;
			return LocalPlayerTurn % 3 == 0;
		}
	}

	public int EnemyScore
	{
		// later will return an array of enemies scores instead of one int
		// didnt have time for the tests
		get
		{
			if (IsNetCurrentlyActive())
			{
				if (OnlineScores.Count == 0) return 0;
				for (int i = 0; i < OnlineScores.Count; i++)
				{
					if (i == PlayerIndex) continue;
					return OnlineScores[i];
				}

				return 0;
			}
			else
			{
				if (SoloScores.Count == 0) return 0;
				for (int i = 0; i < SoloScores.Count; i++)
				{
					if (i == 0) continue;
					return SoloScores[i];
				}

				return 0;
			}
		}
	}

	public int PlayerScore
	{
		get
		{
			if (IsNetCurrentlyActive())
			{
				if (OnlineScores.Count == 0) return 0;
				return OnlineScores[PlayerIndex];
			}
			else
			{
				if (SoloScores.Count == 0) return 0;
				return SoloScores[0];
			}
		}
	}

	public delegate void ScoreEventDelegate(int playerId, float floatEvent);
	public ScoreEventDelegate ScoreEvent;

	#endregion

	public bool GameIsFinished
	{
		get
		{
			if (IsNetCurrentlyActive())
			{
				if (OnlinePlayersID.Count <= 0) return false;
				return GlobalTurn >= 12 * OnlinePlayersID.Count;
			}
			else
			{
				if (SoloNames.Count <= 0) return false;
				return GlobalTurn >= 12 * SoloNames.Count;
			}
		}
	}

	public bool AmIWinning()
	{
		bool isHigher = true;

		if (IsNetCurrentlyActive())
		{
			foreach (int score in OnlineScores)
			{
				if (score > PlayerScore)
				{
					isHigher = false;
					break;
				}
			}
		}
		else
		{
			foreach (int score in SoloScores)
			{
				if (score > PlayerScore)
				{
					isHigher = false;
					break;
				}
			}
		}

		return isHigher;
	}

	#region Methods

	public void ResetManager()
	{
		if (IsNetCurrentlyActive())
		{
			PlayerIndex = -1;
			OnlineTurns.Value = 0;
			OnlineScores.Clear();
			OnlinePlayersID.Clear();
			OnlinePlayersNameIdentification.Clear();
		}
		else
		{
			PlayerIndex = 0;
			SoloTurns = 0;
			SoloScores.Clear();
			SoloNames.Clear();
		}
	}

	public void AddScore(int score, int index = -1)
	{
		if (IsNetCurrentlyActive())
		{
			if (!IsHost)
				return;

			if (index == -1)
				OnlineScores[PlayerIndex] += score;
			else
				OnlineScores[index] += score;
		}
		else
		{
			if (index == -1)
			{
				SoloScores[0] += score;
				ScoreEvent?.Invoke(0, SoloScores[0]);
			}
			else
			{
				SoloScores[index] += score;
				ScoreEvent?.Invoke(index, SoloScores[index]);
			}
		}
	}

	private void OnOnlineScoresChanged(NetworkListEvent<int> changeEvent)
	{
		if (changeEvent.Type == NetworkListEvent<int>.EventType.Value)
			ScoreEvent?.Invoke(changeEvent.Index, changeEvent.Value);
	}

	public void SetPlayerInfo(ulong ID, string name)
	{
		if (IsNetCurrentlyActive())
		{
			OnlineScores.Add(0);
			OnlinePlayersID.Add(ID);
			OnlinePlayersNameIdentification.Add(name);
		}
		else
		{
			SoloScores.Add(0);
			SoloNames.Add(name);
		}
	}

	public bool IsNetCurrentlyActive()
	{
		return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
	}

	public string GetInfo()
	{
		return $"{nameof(GameManager)}: \n" +
			$"{nameof(GlobalTurn)}: {GlobalTurn} \n" +
			$"{nameof(LocalPlayerTurn)}: {LocalPlayerTurn} \n" +
			$"{nameof(PlayerIndexTurn)}: {PlayerIndexTurn} \n \n" +
			$"{nameof(PlayerIndex)}: {PlayerIndex} \n" +
			$"{nameof(PlayerName)}: {PlayerName} \n" +
			$"{nameof(FlagTurn)}: {FlagTurn}";
	}

	#endregion
}