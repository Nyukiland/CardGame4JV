using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Core;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using TMPro;

namespace CardGame.Net
{
	public class DistantNetController : MonoBehaviour
	{
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


		private string _lobbyId;
		private string _joinCode;
		private UnityTransport _transport;
		private NetCommunication _netCommunication;

		private void Start()
		{
			_transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			InitUnityServices().Forget();
		}

		private async UniTask InitUnityServices()
		{
			await UnityServices.InitializeAsync();
			if (!AuthenticationService.Instance.IsSignedIn)
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}

		private void Update()
		{
			if (_netCommunication != null)
			{
				_netComText.text = _netCommunication.gameObject.name + " / \n Join Code: " + _joinCode;
			}
		}

		public async void StartHost()
		{
			if (string.IsNullOrEmpty(_gameName.text)) return;

			var allocation = await RelayService.Instance.CreateAllocationAsync(1);
			_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			var createOptions = new CreateLobbyOptions
			{
				IsPrivate = !_toggleHost.isOn,
				Data = new Dictionary<string, DataObject>
				{
					{ "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, _joinCode) },
					{ "password", new DataObject(DataObject.VisibilityOptions.Private, _passwordField.text) }
				}
			};

			var lobby = await LobbyService.Instance.CreateLobbyAsync(_gameName.text, 2, createOptions);
			_lobbyId = lobby.Id;

			//TO FIX
			//_transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
			NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
			NetworkManager.Singleton.StartHost();

			await GetNetComForThisClientAsync();
			_ = HeartbeatLobby();
		}

		private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			var reader = new FastBufferReader(request.Payload, Unity.Collections.Allocator.None);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);
			response.Approved = receivedPassword.ToString() == _passwordField.text;
			response.CreatePlayerObject = response.Approved;
			response.Pending = false;
		}

		public async void JoinGame(string code = null)
		{
			if (string.IsNullOrEmpty(code)) code = _connectCode.text;
			if (string.IsNullOrEmpty(code)) return;

			var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
			_joinCode = lobby.Data["relayJoinCode"].Value;

			var allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);
			//TO FIX
			// _transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

			var writer = new FastBufferWriter(32, Unity.Collections.Allocator.Temp);
			writer.WriteValueSafe(new FixedString32Bytes(_passwordField.text));
			NetworkManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();

			SafeConnectAsync().Forget();
		}

		private async UniTask SafeConnectAsync()
		{
			bool connected = false;
			float timer = 0;
			float timeout = 3f;

			NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
			NetworkManager.Singleton.StartClient();

			while (!connected && timer < timeout)
			{
				timer += Time.deltaTime;
				await UniTask.Yield();
			}

			NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;

			if (connected)
				await GetNetComForThisClientAsync();
			else
				NetworkManager.Singleton.Shutdown();

			void OnConnected(ulong id) { if (id == NetworkManager.Singleton.LocalClientId) connected = true; }
			void OnDisconnected(ulong id) { if (id == NetworkManager.Singleton.LocalClientId) connected = false; }
		}

		private async UniTask HeartbeatLobby()
		{
			while (NetworkManager.Singleton.IsHost)
			{
				await LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
				await UniTask.Delay(15000);
			}
		}

		private async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;
			await UniTask.WaitUntil(() => NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom));
			_netCommunication = netCom;
			_netCommunication.ReceiveEventTest += NetCommunication_ReceiveEvent;
		}

		private void NetCommunication_ReceiveEvent(string data)
		{
			_receivedInfo.text = data;
		}

		public void SendInfo()
		{
			_netCommunication.SubmitInfoTest(_netCommunication.name + " / \\n password: " + _passwordField.text);
		}

		public void CopyJoinCode()
		{
			if (!string.IsNullOrEmpty(_joinCode))
				CopyHandler.CopyToClipboard(_joinCode);
		}

		public async void RefreshPublicLobbies()
		{
			var result = await LobbyService.Instance.QueryLobbiesAsync();
			var sessions = _publicSessionVerticalLayout.GetComponentsInChildren<PublicSessionVisu>().ToList();
			foreach (var s in sessions) Destroy(s.gameObject);

			foreach (var lobby in result.Results)
			{
				//TO FIX
				var visu = Instantiate(_displayPublic, _publicSessionVerticalLayout.transform).GetComponent<PublicSessionVisu>();
				//visu.SetUpVisu(lobby.Name, lobby.LobbyCode, this);
			}
		}
	}
}
