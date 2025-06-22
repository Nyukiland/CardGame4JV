using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
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
				instance ??= new GameObject(typeof(GameManager).Name).AddComponent<GameManager>();
				instance.gameObject.AddComponent<NetworkObject>();
				_instance = instance;
			}

			return _instance;

		}
		private set { _instance = value; }
	}

	#region

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		_instance = null;
	}

	public NetworkVariable<int> OnlineTurns =
		new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
			if (NetworkManager.Singleton != null)
			{
				if (PlayerIndex == -1 || OnlinePlayersNameIdentification.Count == 0) return "none";
				return OnlinePlayersNameIdentification[PlayerIndex].ToString();
			}
			else
			{
				if (SoloNames.Count == 0) return "none";
				else return SoloNames[0];
			}
		}
	}

	public int GlobalTurn
	{
		get
		{
			if (NetworkManager.Singleton != null) return OnlineTurns.Value;
			else return SoloTurns;
		}
	}

	public int LocalPlayerTurn
	{
		get
		{
			if (NetworkManager.Singleton != null)
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
			if (NetworkManager.Singleton != null)
			{
			if (OnlinePlayersID.Count == 0) return false;
			return Mathf.CeilToInt((float)(OnlineTurns.Value + 1) / OnlinePlayersID.Count) % 3 == 0;
			}
			else
			{
				if (SoloNames.Count == 0) return false;
				return Mathf.CeilToInt((float)(SoloTurns + 1) / SoloNames.Count) % 3 == 0;
			}
		}
	}

	public int PlayerScore
	{
		get
		{
			if (NetworkManager.Singleton != null)
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

	public void ResetManager()
	{
		if (NetworkManager.Singleton != null)
		{
			PlayerIndex = -1;
			OnlineTurns.Value = 0;
			OnlineScores.Clear();
			OnlinePlayersID.Clear();
			OnlinePlayersNameIdentification.Clear();
		}
		else
		{
			SoloTurns = 0;
			SoloScores.Clear();
			SoloNames.Clear();
		}
	}

	public void SetPlayerInfo(ulong ID, string name)
	{
		if (NetworkManager.Singleton != null)
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
}