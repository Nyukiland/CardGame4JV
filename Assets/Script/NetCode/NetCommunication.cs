using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Net;

public class NetCommunication : NetworkBehaviour
{
	[SerializeField]
	private UnityTransport transport;

	private string joinPassword;

	void Start()
	{
		// Assign connection approval logic (host only)
		if (NetworkManager.Singleton != null)
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
	}

	// --- CONNECTION SETUP ---

	public static string GetLocalIPv4()
	{
		foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
				return ip.ToString();
		}
		return "127.0.0.1";
	}

	public void StartHost()
	{
		string localIP = GetLocalIPv4();
		ushort port = 7777;
		transport.SetConnectionData(localIP, port);
		NetworkManager.Singleton.StartHost();
		Debug.Log($"[Host] Started on {localIP}:{port} with password '{joinPassword}'");
	}

	public void JoinGame(string joinCode, string password)
	{
		string[] parts = joinCode.Split(':');
		if (parts.Length != 2 || !ushort.TryParse(parts[1], out ushort port))
		{
			Debug.LogError("Invalid join code. Format: IP:PORT");
			return;
		}

		string ip = parts[0];
		transport.SetConnectionData(ip, port);

		var writer = new FastBufferWriter(32, Allocator.Temp);
		writer.WriteValueSafe(new FixedString32Bytes(password));
		NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

		NetworkManager.Singleton.StartClient();
	}

	private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		var reader = new FastBufferReader(request.Payload, Allocator.None);
		reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

		if (receivedPassword.ToString() == joinPassword)
		{
			response.Approved = true;
			response.CreatePlayerObject = true;
		}
		else
		{
			response.Approved = false;
			Debug.LogWarning("Client rejected: wrong password");
		}

		response.Pending = false;
	}

	// --- PLAYER INFO DATA SYNC ---

	//public void SendMyPlayerInfo()
	//{
	//	if (!IsClient) return;

	//	//PlayerInfo info = new PlayerInfo
	//	//{
	//	//	health = Random.Range(10, 100),
	//	//	position = transform.position
	//	//};

	//	//SubmitInfoToServerRpc(info);
	//}

	//[ServerRpc(RequireOwnership = false)]
	//void SubmitInfoToServerRpc(PlayerInfo info, ServerRpcParams rpcParams = default)
	//{
	//	ulong senderId = rpcParams.Receive.SenderClientId;
	//	//Debug.Log($"[Server] Received from {senderId}: Health={info.health}, Pos={info.position}");

	//	BroadcastInfoClientRpc(info);
	//}

	//[ClientRpc]
	//void BroadcastInfoClientRpc(PlayerInfo info)
	//{
	//	//Debug.Log($"[Client] Broadcasted: Health={info.health}, Pos={info.position}");
	//}
}
