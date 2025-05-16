using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Net;
using TMPro;

namespace CardGame.Net
{
	public class TestLinkUI : MonoBehaviour
	{
		[SerializeField, Disable]
		private NetCommunication _netCommunication;

		[SerializeField]
		private TextMeshProUGUI _netComText, _receivedInfo;

		[SerializeField]
		private TMP_InputField _passwordField, _connectCode;

		[SerializeField]
		private UnityTransport _transport;

		private string _joinPassword;
		private string _joinCode;

		private void Update()
		{
			if (_netCommunication != null)
			{
				_netComText.text = _netCommunication.gameObject.name + " / \n Join Code: " + _joinCode;
				_receivedInfo.text = _netCommunication.SyncedData.Value.Text;
			}
		}

		private void OnDestroy()
		{
			if (_netCommunication != null)
			{
				_netCommunication.ReceiveEvent -= NetCommunication_ReceiveEvent;
			}
		}

		private void NetCommunication_ReceiveEvent(DataNetcode data)
		{
			_receivedInfo.text = _netCommunication.SyncedData.Value.Text;
		}

		private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			FastBufferReader reader = new(request.Payload, Allocator.None);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

			if (receivedPassword.ToString() == _joinPassword)
			{
				response.Approved = true;
				response.CreatePlayerObject = true;
			}
			else
			{
				response.Approved = false;
				UnityEngine.Debug.LogWarning($"[{nameof(TestLinkUI)}] Client rejected: wrong password");
			}

			response.Pending = false;
		}

		public void StartHost()
		{
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;

			_joinPassword = _passwordField.text;

			string localIP = GetLocalIPv4();
			ushort port = 7777;

			_joinCode = CodeNetUtility.GetJoinCode(localIP, port);

			_transport.SetConnectionData(localIP, port);
			NetworkManager.Singleton.StartHost();
			StartCoroutine(GetNetComForThisClient());
		}

		public void JoinGame()
		{
			string password = _passwordField.text;
			string joinCode = _connectCode.text;

			_joinCode = "joined";

			if (string.IsNullOrEmpty(joinCode)) return;

			CodeNetUtility.DecodeJoinCode(joinCode, out string ip, out ushort port);
			_transport.SetConnectionData(ip, port);

			FastBufferWriter writer = new(32, Allocator.Temp);
			writer.WriteValueSafe(new FixedString32Bytes(password));
			NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

			NetworkManager.Singleton.StartClient();
			StartCoroutine(GetNetComForThisClient());
		}

		private string GetLocalIPv4()
		{
			foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
					return ip.ToString();
			}
			return "127.0.0.1";
		}

		private IEnumerator GetNetComForThisClient()
		{
			NetCommunication netCom = null;
			while(!NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom))
			{
				yield return null;
			}

			_netCommunication = netCom;
			_netCommunication.ReceiveEvent += NetCommunication_ReceiveEvent;
		}

		public void SendInfo()
		{
			DataNetcode info = new DataNetcode(_netCommunication.name + " / \n password: " + _passwordField.text);

			_netCommunication.SubmitInfo(info);
		}

		public void CopyJoinCode()
		{
			if (string.IsNullOrEmpty(_joinCode)) return;

			CopyHandler.CopyToClipboard(_joinCode);
		}
	}
}