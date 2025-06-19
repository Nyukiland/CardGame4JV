using Unity.Netcode.Transports.UTP;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CardGame.Net
{
	public abstract class NetControllerParent : MonoBehaviour
	{
		[SerializeField, Disable]
		protected NetCommunication _netCommunication;

		[SerializeField]
		protected GameObject _displayPublic;

		[SerializeField, LockUser]
		protected int _maxPlayer = 4;

		protected string _joinPassword;
		protected string _joinCode;

		protected UnityTransport _transport;

		protected virtual void Start()
		{
			_transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		}

		public virtual void Launch()
		{

		}

		//call when you stop using this mode
		public virtual void StopHosting()
		{
			if (_netCommunication == null || !_netCommunication.IsHost) return;

			NetworkManager.Singleton.Shutdown();
		}

		public virtual void DisconnectFromGame()
		{
			if (_netCommunication == null || !_netCommunication.IsClient) return;

			NetworkManager.Singleton.Shutdown();
		}

		#region Connection

		public virtual void StartHost() { }
		public virtual void JoinGame(string code = null) { }

		protected virtual void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
		{
			FastBufferReader reader = new(request.Payload, Allocator.None);
			reader.ReadValueSafe(out FixedString32Bytes receivedPassword);

			if (NetworkManager.Singleton.ConnectedClients.Count > 4)
			{
				response.Approved = false;
				response.Pending = false;
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Connection rejected: game is full.", this);
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
				UnityEngine.Debug.LogWarning($"[{nameof(LocalNetControllerTestScene)}] Connection rejected: wrong password", this);
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

			if (connected)
				await GetNetComForThisClientAsync();
			else
				NetworkManager.Singleton.Shutdown();

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
		}

		public virtual void CopyJoinCode()
		{
			if (!string.IsNullOrEmpty(_joinCode))
				CopyHandler.CopyToClipboard(_joinCode);
		}

		#endregion
	}
}
