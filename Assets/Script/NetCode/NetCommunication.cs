using Unity.Netcode;
using UnityEngine;

namespace CardGame.Net
{
	public class NetCommunication : NetworkBehaviour
	{
		public static NetCommunication OwnedInstance { get; private set; }

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
			Debug.Log($"[Server] Received from {senderId}:{info.Text}");

			BroadcastInfoClientRpc(info);
		}

		[ClientRpc]
		public void BroadcastInfoClientRpc(DataNetcode info)
		{
			Debug.Log($"[Client] Broadcasted: {info.Text}");
		}
	}
}