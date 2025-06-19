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
    public class LocalNetController : NetControllerParent
    {
        [SerializeField, Disable] private LanSearchBeacon _lanSearch;

        [SerializeField] private NetworkUI _networkUI;
        
        [SerializeField] private string _sceneName;
                
        #region Unity Methods

        protected override void Start()
        {
			base.Start();
            _networkUI.TogglePublicEvent += TogglePublicSearch;
            _networkUI.StartHostEvent += StartHost;
            _networkUI.JoinGameEvent += ()=>JoinGame();
            _networkUI.UnhostEvent += StopHosting;
            _networkUI.CopyCodeEvent += CopyJoinCode;
            _networkUI.QuitGameEvent += DisconnectFromGame;
			_networkUI.PlayGameEvent += LaunchGame;

			//should be moved 
			Launch();
		}

		public override void Launch()
		{
			base.Launch();
			_lanSearch = GetComponent<LanSearchBeacon>();
			_lanSearch.OnHostsUpdated += UpdateConnectionList;
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

		public override void StartHost()
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

		public override void JoinGame(string joinCode = default)
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

		public override void StopHosting()
		{
			if (_netCommunication == null || !_netCommunication.IsHost) return;

			NetworkManager.Singleton.OnConnectionEvent -= CallOnConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback -= CallOnDisconnect;

			NetworkManager.Singleton.Shutdown();
			
			_lanSearch.StopBroadcast();
			
			DisconnectLogic();
		}

		public override void DisconnectFromGame()
		{
			base.DisconnectFromGame();

			DisconnectLogic();
		}

		private void DisconnectLogic()
		{
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

		protected override async UniTask SafeConnectAsync()
		{
			await base.SafeConnectAsync();
			if (!NetworkManager.Singleton.ShutdownInProgress) TogglePublicSearch(false);
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
				session.SetUpVisu(data.gameName, data.joinCode, this);
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

		protected override async UniTask GetNetComForThisClientAsync()
		{
			await base.GetNetComForThisClientAsync();

			if (_netCommunication == null) return;

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