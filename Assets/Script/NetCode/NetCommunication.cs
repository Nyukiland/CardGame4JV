using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Net;

public class NetCommunication : NetworkBehaviour
{
	public static NetCommunication OwnedInstance { get; private set; }

	void Start()
	{
		if (IsOwner)
		{
			gameObject.name = "netcom" + NetworkObjectId;
			OwnedInstance = this;
		}
	}

	// --- PLAYER INFO DATA SYNC ---

	//public void SendMyPlayerInfo()
	//{
	//	if (!IsClient) return;

	//	DataNetcode info = new DataNetcode($"test text from");

	//	SubmitInfoToServerRpc(info);
	//}

	//[ServerRpc(RequireOwnership = false)]
	//void SubmitInfoToServerRpc(DataNetcode info, ServerRpcParams rpcParams = default)
	//{
	//	ulong senderId = rpcParams.Receive.SenderClientId;
	//	Debug.Log($"[Server] Received from {senderId}:{info.Text}");

	//	BroadcastInfoClientRpc(info);
	//}

	//[ClientRpc]
	//void BroadcastInfoClientRpc(DataNetcode info)
	//{
	//	Debug.Log($"[Client] Broadcasted: {info.Text}");
	//}
}
