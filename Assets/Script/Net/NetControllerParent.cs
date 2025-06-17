using Unity.Netcode.Transports.UTP;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace CardGame.Net
{
	public abstract class NetControllerParent : MonoBehaviour
	{
		[SerializeField, Disable]
		protected NetCommunication _netCommunication;

		[SerializeField]
		protected TextMeshProUGUI _netComText, _receivedInfo;

		[SerializeField]
		protected TMP_InputField _passwordField, _connectCode, _gameName;

		[SerializeField]
		protected Toggle _toggleHost;

		[SerializeField]
		protected VerticalLayoutGroup _publicSessionVerticalLayout;

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
		public virtual void EndUseNet(bool shutdownNet = true)
		{
			if (shutdownNet) NetworkManager.Singleton.Shutdown();
		}

		#region TestStuff

		protected virtual void Update()
		{
			if (_netCommunication != null)
			{
				_netComText.text = _netCommunication.gameObject.name + " / \n Join Code: " + _joinCode;
			}
		}

		protected virtual void NetCommunication_ReceiveEvent(string data)
		{
			_receivedInfo.text = data;
		}

		public virtual void SendInfo()
		{
			_netCommunication.SubmitInfoTest(_netCommunication.name + " / \\n password: " + _passwordField.text);
		}

		protected virtual void OnDestroy()
		{
			if (_netCommunication != null)
			{
				_netCommunication.ReceiveEventTest -= NetCommunication_ReceiveEvent;
			}
		}

		#endregion

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
			_netCommunication.ReceiveEventTest += NetCommunication_ReceiveEvent;
		}

		public virtual void CopyJoinCode()
		{
			if (!string.IsNullOrEmpty(_joinCode))
				CopyHandler.CopyToClipboard(_joinCode);
		}

		#endregion
	}
}
