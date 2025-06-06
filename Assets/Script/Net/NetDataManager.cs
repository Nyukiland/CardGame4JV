using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetDataManager : NetworkBehaviour
{

	private static NetDataManager _instance;
	public static NetDataManager Instance
	{
		get
		{
			if (_instance == null)
			{
				NetDataManager instance = FindFirstObjectByType<NetDataManager>();
				instance ??= new GameObject(typeof(NetDataManager).Name).AddComponent<NetDataManager>();
				_instance = instance;
			}

			return _instance;

		}
		private set { _instance = value; }
	}

	public NetworkVariable<int> PlayerTurn = 
		new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public NetworkList<ulong> PlayersID 
		= new(new List<ulong>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		_instance = null;
	}

}