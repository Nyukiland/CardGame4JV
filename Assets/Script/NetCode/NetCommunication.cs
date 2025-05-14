using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CardGame.Net
{
	public delegate void ReceiveEventDelegate(DataNetcode data);
	public class NetCommunication : NetworkBehaviour
	{
		public static NetCommunication OwnedInstance { get; private set; }

		public event ReceiveEventDelegate ReceiveEvent;

		public DataNetcode DataNetcode { get; private set; } = new();

		public TestLinkUI link;

		void Start()
		{
			if (IsOwner)
			{
				gameObject.name = "netcom" + NetworkObjectId;
				OwnedInstance = this;
			}
		}

		[ServerRpc(RequireOwnership = false)]
		public void SubmitInfoToServerRpc(DataNetcode info, ServerRpcParams rpcParams = default)
		{
			ulong senderId = rpcParams.Receive.SenderClientId;
			Debug.Log($"[Server] Received from {senderId}: {info.Text}");

			// Find the other client
			ulong targetClientId = NetworkManager.Singleton.ConnectedClientsIds
				.FirstOrDefault(id => id != senderId);

			Debug.Log($"[Server] Will send to: {targetClientId}");

			var clientParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new[] { targetClientId }
				}
			};

			SendInfoToClientRpc(info, clientParams);
		}

		[ClientRpc]
		public void SendInfoToClientRpc(DataNetcode info, ClientRpcParams rpcParams = default)
		{
			Debug.Log($"[Client] Received info: {info.Text}");
			ReceiveEvent?.Invoke(info);
			DataNetcode = info;

			link.Receive(info);
		}
	}
}