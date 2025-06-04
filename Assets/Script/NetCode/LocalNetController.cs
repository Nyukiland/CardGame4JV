using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using System.Net;
using TMPro;

namespace CardGame.Net
{
	public class LocalNetController : MonoBehaviour
	{
		[SerializeField, Disable]
		private NetCommunication _netCommunication;

		[SerializeField, Disable]
		private LanSearchBeacon _lanSearch;

		[SerializeField]
		private TextMeshProUGUI _netComText, _receivedInfo;

		[SerializeField]
		private TMP_InputField _passwordField, _connectCode, _gameName;

		[SerializeField]
		private Toggle _toggleHost;

		[SerializeField]
		private VerticalLayoutGroup _publicSessionVerticalLayout;

		[SerializeField]
		private GameObject _displayPublic;

		private UnityTransport _transport;

		private string _joinPassword;
		private string _joinCode;

		private void Start()
		{
			_transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
			_lanSearch = GetComponent<LanSearchBeacon>();
			_lanSearch.OnHostsUpdated += UpdateConnectionList;
		}

		private void Update()
		{
			if (_netCommunication != null)
			{
				_netComText.text = _netCommunication.gameObject.name + " / \n Join Code: " + _joinCode;
			}
		}

		private void OnDestroy()
		{
			if (_netCommunication != null)
			{
				_netCommunication.ReceiveEventTest -= NetCommunication_ReceiveEvent;
			}

			if (_lanSearch != null)
			{
				_lanSearch.OnHostsUpdated -= UpdateConnectionList;
			}
		}

		private void NetCommunication_ReceiveEvent(string data)
		{
			_receivedInfo.text = data;
		}

		public void SendInfo()
		{
			_netCommunication.SubmitInfoTest(_netCommunication.name + " / \n password: " + _passwordField.text);
		}


		#region Connection

		private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			FastBufferReader reader = new(request.Payload, Allocator.None);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

			if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
			{
				response.Approved = false;
				response.Pending = false;
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetController)}] Connection rejected: game is full.", this);
				return;
			}

			if (receivedPassword.ToString() == _joinPassword)
			{
				response.Approved = true;
				response.CreatePlayerObject = true;
			}
			else
			{
				response.Approved = false;
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetController)}] Connection rejected: wrong password", this);
			}

			response.Pending = false;
		}

		public void StartHost()
		{
			if (string.IsNullOrEmpty(_gameName.text)) return;

			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
			_joinPassword = _passwordField.text;

			string localIP = GetLocalIPv4();
			ushort startingPort = 60121;
			ushort maxPort = 60200;
			ushort selectedPort = startingPort;

			// Find a free port
			while (selectedPort <= maxPort)
			{
				if (IsPortAvailable(localIP, selectedPort))
					break;

				selectedPort++;
			}

			if (selectedPort > maxPort)
			{
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetController)}] No available ports found in the specified range.", this);
				return;
			}

			_joinCode = NetUtility.GetJoinCode(localIP, selectedPort);

			_transport.SetConnectionData(localIP, selectedPort);
			NetworkManager.Singleton.StartHost();

			if (_toggleHost.isOn)
				_lanSearch.StartBroadcast(_gameName.text, selectedPort, _joinCode);

			GetNetComForThisClientAsync().Forget();
		}

		private void OnClientDisconnected(ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;

			UnityEngine.Debug.LogWarning($"[{nameof(LocalNetController)}] Client {clientId} disconnected.");
		}

		private bool IsPortAvailable(string ip, int port)
		{
			try
			{
				TcpListener listener = new TcpListener(IPAddress.Parse(ip), port);
				listener.Start();
				listener.Stop();
				return true;
			}
			catch (SocketException)
			{
				return false;
			}
		}

		public void JoinGame(string joinCode = default)
		{
			string password = _passwordField.text;
			if (string.IsNullOrEmpty(joinCode)) joinCode = _connectCode.text;

			//if still empty return
			if (string.IsNullOrEmpty(joinCode)) return;

			NetUtility.DecodeJoinCode(joinCode, out string ip, out ushort port);

			_transport.SetConnectionData(ip, port);

			FastBufferWriter writer = new(32, Allocator.Temp);
			writer.WriteValueSafe(new FixedString32Bytes(password));
			NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

			SafeConnectAsync().Forget();
		}

		public void StopHosting()
		{
			if (_netCommunication == null || !_netCommunication.IsHost) return;

			_lanSearch.StopBroadcast();
			
			DisconnectLogic();
		}

		public void DisconnectFromGame()
		{
			if (_netCommunication == null || !_netCommunication.IsClient) return;

			DisconnectLogic();
		}

		private void DisconnectLogic()
		{
			NetworkManager.Singleton.Shutdown();

			_netCommunication = null;
			_joinCode = null;

			_netComText.text = string.Empty;
			_receivedInfo.text = string.Empty;
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

		private async UniTask SafeConnectAsync()
		{
			bool connectionResultReceived = false;
			bool connectionSucceeded = false;
			float timeout = 2f;

			NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;

			NetworkManager.Singleton.StartClient();

			float elapsed = 0f;
			while (!connectionResultReceived && elapsed < timeout)
			{
				elapsed += Time.deltaTime;
				await UniTask.Yield(); // non-blocking
			}

			NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;

			if (connectionSucceeded)
			{
				TogglePublicSearch(false);
				await GetNetComForThisClientAsync();
			}
			else
			{
				NetworkManager.Singleton.Shutdown();
			}

			void OnConnected(ulong clientId)
			{
				if (clientId == NetworkManager.Singleton.LocalClientId)
				{
					connectionResultReceived = true;
					connectionSucceeded = true;
				}
			}

			void OnDisconnected(ulong clientId)
			{
				if (clientId == NetworkManager.Singleton.LocalClientId)
				{
					connectionResultReceived = true;
					connectionSucceeded = false;
				}
			}
		}

		#endregion

		#region public Connection

		private void UpdateConnectionList(List<LanSearchBeacon.BeaconDataWithIP> listData)
		{
			List<LanSearchBeacon.BeaconDataWithIP> listToUse = new(listData);
			List<string> gameNames = listToUse.Select(x => x.gameName).ToList();
			List<PublicSessionVisu> sessions = _publicSessionVerticalLayout.GetComponentsInChildren<PublicSessionVisu>().ToList();
			if (sessions.Count != 0)
			{
				for (int i = sessions.Count - 1; i >= 0; i--)
				{
					PublicSessionVisu visuSession = sessions[i];
					if (gameNames.Contains(visuSession.GameName))
					{
						int index = gameNames.IndexOf(visuSession.GameName);
						gameNames.RemoveAt(index);
						listToUse.RemoveAt(index);
					}
					else
					{
						sessions.RemoveAt(i);
						Destroy(visuSession.gameObject);
					}
				}
			}

			foreach (LanSearchBeacon.BeaconDataWithIP data in listToUse)
			{
				PublicSessionVisu session = Instantiate(_displayPublic, _publicSessionVerticalLayout.transform).GetComponent<PublicSessionVisu>();
				session.SetUpVisu(data.gameName, data.joinCode, this);
			}
		}

		public void TogglePublicSearch(bool isOn)
		{
			if (isOn)
				_lanSearch.StartListening();
			else
			{
				_lanSearch.StopListening();
				List<PublicSessionVisu> sessions = _publicSessionVerticalLayout.GetComponentsInChildren<PublicSessionVisu>().ToList();
				if (sessions.Count != 0)
				{
					for (int i = sessions.Count - 1; i >= 0; i--)
					{
						PublicSessionVisu visuSession = sessions[i];

						sessions.RemoveAt(i);
						Destroy(visuSession.gameObject);
					}
				}
			}
		}

		#endregion

		#region Useful

		private async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;

			await UniTask.WaitUntil(() =>
				NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom));

			_netCommunication = netCom;
			_netCommunication.ReceiveEventTest += NetCommunication_ReceiveEvent;
		}

		public void CopyJoinCode()
		{
			if (string.IsNullOrEmpty(_joinCode)) return;

			CopyHandler.CopyToClipboard(_joinCode);
		}

		#endregion
	}
}