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

	[Disable]
	public NetworkVariable<int> Turns =
		new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<int> Scores =
		new(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<ulong> PlayersID
		= new(new List<ulong>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public NetworkList<FixedString64Bytes> PlayersNameIdentification
		= new(new List<FixedString64Bytes>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public int PlayerIndex
	{
		get;
		private set;
	} = -1;

	public int PlayerIndexTurn
	{
		get
		{
			if (PlayersID.Count == 0) return -1;
			return Turns.Value % PlayersID.Count;
		}
	}

	public string PlayerName
	{
		get
		{
			if (PlayerIndex == -1 || PlayersNameIdentification.Count == 0) return "none";
			return PlayersNameIdentification[PlayerIndex].ToString();
		}
	}

	public int GlobalTurn => Turns.Value;

	public int LocalPlayerTurn
	{
		get
		{
			if (PlayersID.Count == 0) return -1;
			return Mathf.CeilToInt((float)(Turns.Value + 1) / PlayersID.Count);
		}
	}

	public bool FlagTurn
	{
		get
		{
			if (PlayersID.Count == 0) return false;
			return Mathf.CeilToInt((float)(Turns.Value + 1) / PlayersID.Count) % 3 == 0;
		}
	}

	public int PlayerScore
	{
		get
		{
			if (Scores.Count == 0) return 0;
			return Scores[PlayerIndex];
		}
	}

	public void SetPlayerInfo(ulong ID, string name)
	{
		Scores.Add(0);
		PlayersID.Add(ID);
		PlayersNameIdentification.Add(name);
	}

	public void SetLocalPlayerInfo()
	{
		UnityEngine.Debug.Log("fuck");
		HoldForLocalChange(NetworkManager.Singleton.LocalClientId).Forget();
	}

	private async UniTask HoldForLocalChange(ulong owner)
	{
		await UniTask.WaitUntil(() => PlayersID.IndexOf(owner) != -1);
		PlayerIndex = PlayersID.IndexOf(owner);
	}

	private void Update()
	{
		UnityEngine.Debug.Log($"{nameof(GameManager)}: \n" +
			$"{nameof(Turns)}: {Turns.Value} \n" +
			$"{nameof(PlayerIndexTurn)}: {PlayerIndexTurn} \n \n" +
			$"{nameof(PlayerIndex)}: {PlayerIndex} \n" +
			$"{nameof(PlayerName)}: {PlayerName} \n" +
			$"{nameof(FlagTurn)}: {FlagTurn}"
			);
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		_instance = null;
	}

	public string GetInfo()
	{
		return $"{nameof(GameManager)}: \n" +
			$"{nameof(PlayerIndexTurn)}: {PlayerIndexTurn} \n \n" +
			$"{nameof(PlayerIndex)}: {PlayerIndex} \n" +
			$"{nameof(PlayerName)}: {PlayerName} \n" +
			$"{nameof(FlagTurn)}: {FlagTurn}";
	}
}