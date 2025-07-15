using System;
using CardGame.UI;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace CardGame.Net
{
	public abstract class NetControllerParent : MonoBehaviour
	{
		[SerializeField, Disable] protected NetCommunication _netCommunication;

		[SerializeField] protected NetworkUI _networkUI;
		[SerializeField] protected GameObject _displayPublicPrefab;
		[SerializeField, LockUser] protected int _maxPlayer = 4;
		
		protected bool _isDistant;
		protected UnityTransport _transport;

		protected virtual void Start()
		{
			_transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		}

		private void OnDestroy()
		{
			if (_netCommunication != null)
				_netCommunication.OnLaunchGameEvent -= _networkUI.CloseMenu;
		}

		protected virtual void Launch() { }

		//call when you stop using this mode
		protected virtual void StopHosting()
		{
			if (_netCommunication == null || !_netCommunication.IsHost) return;

			NetworkManager.Singleton.Shutdown();
		}

		protected virtual void DisconnectFromGame()
		{
			if (_netCommunication == null || !_netCommunication.IsClient) return;

			NetworkManager.Singleton.Shutdown();
		}

		#region Connection

		protected virtual void StartHost() { }
		public virtual void JoinGame(string code) { }

		protected virtual void TogglePublicSearch(bool isOn) { }

		protected virtual void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
			{
				response.Approved = true;
				response.CreatePlayerObject = true;
				response.Pending = false;
				return;
			}

			using FastBufferReader reader = new(request.Payload, Allocator.Temp);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

			if (NetworkManager.Singleton.ConnectedClients.Count > 4)
			{
				response.Approved = false;
				response.Pending = false;
				Debug.LogWarning($"[{nameof(NetControllerParent)}] Connection rejected: game is full.", this);
				return;
			}

			if (_networkUI.Password == NetworkUI.NONE_BASE_VALUE || receivedPassword.ToString() == _networkUI.Password)
			{
				response.Approved = true;
				response.CreatePlayerObject = true;
			}
			else
			{
				response.Approved = false;
				Debug.LogWarning($"[{nameof(NetControllerParent)}] Connection rejected: wrong password", this);
			}

			response.Pending = false;
		}

		protected virtual async UniTask SafeConnectAsync()
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

			_networkUI.ToggleInputBlock(false);
			
			if (connected)
			{
				await GetNetComForThisClientAsync();
			}
			else
			{
				NetworkManager.Singleton.Shutdown();
			}

			//------------------------------
			void OnConnected(ulong id) 
			{ 
				if (id == NetworkManager.Singleton.LocalClientId) 
					connected = true; 
			}

			void OnDisconnected(ulong id) 
			{ 
				if (id == NetworkManager.Singleton.LocalClientId) 
					connected = false; 
			}
		}

		#endregion

		#region Useful

		protected virtual async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;

			await UniTask.WaitUntil(() =>
				NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom));

			_netCommunication = netCom;
			
			_netCommunication.OnLaunchGameEvent += _networkUI.CloseMenu;
			
			if (_netCommunication == null) return;
			
			_netCommunication.OnLaunchGameEvent += ()=>
			{
				_networkUI.CloseMenu();
				_netCommunication.OnLaunchGameEvent -= _networkUI.CloseMenu;
			};

			if (_netCommunication.IsHost) return;
			
			_netCommunication.OnDestroyEvent += ()=>
			{
				_networkUI.QuitClientGame();
				_networkUI.SpawnPopUp("You have been disconnected because the host closed the game", 2f).Forget();
				_netCommunication.OnDestroyEvent -= _networkUI.QuitClientGame;
			};
		}

		protected virtual void CopyJoinCode(string code)
		{
			if (!string.IsNullOrEmpty(code))
				CopyHandler.CopyToClipboard(code);
		}

		protected virtual void ToggleDistant(bool isDistant)
		{
			_isDistant = isDistant;
		}

		#endregion

		protected virtual void LaunchGame() { }
	}
}
