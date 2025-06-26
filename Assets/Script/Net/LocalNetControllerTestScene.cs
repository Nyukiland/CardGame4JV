using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using System.Net;

namespace CardGame.Net
{
	public class LocalNetControllerTestScene : NetControllerParentTest
	{
		[SerializeField, Disable]
		private LanSearchBeacon _lanSearch;

		protected override void Start()
		{
			base.Start();

			//should be called by a button later
			Launch();
		}

		public override void Launch()
		{
			base.Launch();
			_lanSearch = GetComponent<LanSearchBeacon>();
			_lanSearch.OnHostsUpdated += UpdateConnectionList;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (_lanSearch != null)
			{
				_lanSearch.OnHostsUpdated -= UpdateConnectionList;
			}
		}

		#region Connection

		public override void StartHost()
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
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] No available ports found in the specified range.", this);
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

			UnityEngine.Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Client {clientId} disconnected.");
		}

		private bool IsPortAvailable(string ip, int port)
		{
			try
			{
				TcpListener listener = new (IPAddress.Parse(ip), port);
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

		public override void StopHosting()
		{
			base.StopHosting();
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
			base.DisconnectFromGame();
			
			TogglePublicSearch(false);

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

		public override void TogglePublicSearch(bool isOn)
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
	}
}