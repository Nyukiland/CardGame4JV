using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CardGame.UI;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardGame.Net
{
    public class LocalNetController : MonoBehaviour
    {
        [SerializeField, Disable] private NetCommunication _netCommunication;

        [SerializeField, Disable] private LanSearchBeacon _lanSearch;

        [SerializeField] private NetworkUI _networkUI;
        
        [SerializeField] private PublicSessionVisu _displayPublic;
        [SerializeField] private string _sceneName;
        
        private UnityTransport _transport;
        
        #region Unity Methods

        private void Start()
        {
            _transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            _lanSearch = GetComponent<LanSearchBeacon>();
            _lanSearch.OnHostsUpdated += UpdateConnectionList;

            _networkUI.TogglePublicEvent += TogglePublicSearch;
            _networkUI.StartHostEvent += StartHost;
            _networkUI.JoinGameEvent += ()=>JoinGame();
            _networkUI.UnhostEvent += StopHosting;
            _networkUI.CopyCodeEvent += CopyJoinCode;
            _networkUI.QuitGameEvent += DisconnectFromGame;
			_networkUI.PlayGameEvent += LaunchGame;

		}
        
        private void OnDestroy()
        {
            if (_lanSearch != null)
            {
                _lanSearch.OnHostsUpdated -= UpdateConnectionList;
            }
            
            _networkUI.TogglePublicEvent -= TogglePublicSearch;
            _networkUI.StartHostEvent -= StartHost;
            _networkUI.JoinGameEvent -= ()=>JoinGame();
            _networkUI.UnhostEvent -= StopHosting;
            _networkUI.CopyCodeEvent -= CopyJoinCode;
            _networkUI.QuitGameEvent -= DisconnectFromGame;
			_networkUI.PlayGameEvent -= LaunchGame;
        }
        
        #endregion
        
        #region Connection

		private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			FastBufferReader reader = new(request.Payload, Allocator.None);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

			if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
			{
				response.Approved = false;
				response.Pending = false;
				Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Connection rejected: game is full.", this);
				return;
			}

			if (receivedPassword == _networkUI.Code)
			{
				response.Approved = true;
				response.CreatePlayerObject = true;
			}
			else
			{
				response.Approved = false;
				Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Connection rejected: wrong password", this);
			}

			response.Pending = false;
		}

		public void StartHost()
		{
			if (string.IsNullOrEmpty(_networkUI.SessionName)) return;

			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

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
				Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] No available ports found in the specified range.", this);
				return;
			}

			_networkUI.Code = NetUtility.GetJoinCode(localIP, selectedPort);

			_transport.SetConnectionData(localIP, selectedPort);
			NetworkManager.Singleton.StartHost();

			if (_networkUI.IsPublicShown)
				_lanSearch.StartBroadcast(_networkUI.SessionName, selectedPort, _networkUI.Code);

			NetworkManager.Singleton.OnConnectionEvent += CallOnConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback += CallOnDisconnect;
			GetNetComForThisClientAsync().Forget();
		}

		private void CallOnConnect(NetworkManager arg1, ConnectionEventData arg2)
		{
			_networkUI.UpdateAfterHost();
		}

		private void CallOnDisconnect(ulong obj)
		{
			_networkUI.UpdateAfterHost();
		}

		private void OnClientDisconnected(ulong clientId)
		{
			if (clientId == NetworkManager.Singleton.LocalClientId) return;

			Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Client {clientId} disconnected.");
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
			if (string.IsNullOrEmpty(joinCode)) joinCode = _networkUI.Code;

			//if still empty return
			if (string.IsNullOrEmpty(joinCode)) return;

			_networkUI.ToggleInputBlock(true);

			NetUtility.DecodeJoinCode(joinCode, out string ip, out ushort port);

			_transport.SetConnectionData(ip, port);

			FastBufferWriter writer = new(32, Allocator.Temp);
			writer.WriteValueSafe(new FixedString32Bytes(_networkUI.Password));
			NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

			SafeConnectAsync().Forget();
		}

		public void StopHosting()
		{
			if (_netCommunication == null || !_netCommunication.IsHost) return;

			NetworkManager.Singleton.OnConnectionEvent -= CallOnConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback -= CallOnDisconnect;
			
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
			_networkUI.Code = string.Empty;
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

				_networkUI.OpenAfterClient();
			}
			else
			{
				NetworkManager.Singleton.Shutdown();
			}
			
			_networkUI.ToggleInputBlock(false);

			return;

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
			List<PublicSessionVisu> sessions = _networkUI.PublicHostsContainer.GetComponentsInChildren<PublicSessionVisu>().ToList();
			Debug.Log(sessions.Count);
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
				PublicSessionVisu session = Instantiate(_displayPublic, _networkUI.PublicHostsContainer.transform).GetComponent<PublicSessionVisu>();
				//session.SetUpVisu(data.gameName, data.joinCode, this);
			}
		}

		private void TogglePublicSearch(bool isOn)
		{
			if (isOn)
				_lanSearch.StartListening();
			else
			{
				_lanSearch.StopListening();
				List<PublicSessionVisu> sessions = _networkUI.PublicHostsContainer.GetComponentsInChildren<PublicSessionVisu>().ToList();
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

			_netCommunication.OnDestroyEvent += ()=>
			{
				_networkUI.QuitClientGame();
				_netCommunication.OnDestroyEvent -= _networkUI.QuitClientGame;
			};
			
			_netCommunication.OnLaunchGameEvent += ()=>
			{
				_networkUI.CloseMenu();
				_netCommunication.OnLaunchGameEvent -= _networkUI.CloseMenu;
			};
		}

		private void CopyJoinCode(string code)
		{
			if (string.IsNullOrEmpty(code)) return;

			CopyHandler.CopyToClipboard(code);
		}

		#endregion

		#region Solo

		private void LaunchGame()
		{
			if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
			{
				SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
			}
			else
			{
				_netCommunication.LoadScene(_sceneName);
			}
		}

		#endregion
	}
}