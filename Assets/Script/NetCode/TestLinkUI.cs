using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Net;
using TMPro;

public class TestLinkUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _netComText;

	[SerializeField]
	private TMP_InputField _passwordField, _connectCode;

	[SerializeField]
	private UnityTransport _transport;

	private string _joinPassword;
	private string _joinCode;

	private void Start()
	{
		// Assign event of connection (host only)
		NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
	}

	private void Update()
	{
		if (NetCommunication.OwnedInstance != null)
			_netComText.text = NetCommunication.OwnedInstance.gameObject.name + " / \n Join Code: " + _joinCode;
	}

	private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		var reader = new FastBufferReader(request.Payload, Allocator.None);
		reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

		if (receivedPassword.ToString() == _joinPassword)
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

	public void StartHost()
	{
		_joinPassword =  _passwordField.text;

		string localIP = GetLocalIPv4();
		ushort port = 7777;

		_joinCode = CodeNetUtility.GetJoinCode(localIP, port);

		_transport.SetConnectionData(localIP, port);
		NetworkManager.Singleton.StartHost();
		Debug.Log($"[Host] Started on {localIP}:{port} with password '{_joinPassword}'");
	}

	public void JoinGame()
	{
		string password = _passwordField.text;
		string joinCode = _connectCode.text;

		CodeNetUtility.DecodeJoinCode(joinCode, out string ip, out ushort port);
		_transport.SetConnectionData(ip, port);

		var writer = new FastBufferWriter(32, Allocator.Temp);
		writer.WriteValueSafe(new FixedString32Bytes(password));
		NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

		NetworkManager.Singleton.StartClient();
	}

	private string GetLocalIPv4()
	{
		foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
				return ip.ToString();
		}
		return "127.0.0.1";
	}
}