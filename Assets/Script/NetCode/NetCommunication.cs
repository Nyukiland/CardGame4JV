using System.Collections.Generic;
using Unity.Netcode;

namespace CardGame.Net
{
	public class NetCommunication : NetworkBehaviour
	{
		public static Dictionary<ulong, NetCommunication> Instances = new();

		public delegate void ReceiveEventDelegate(DataNetcode data);
		public event ReceiveEventDelegate ReceiveEvent;

		public NetworkVariable<DataNetcode> SyncedData = new(
			new DataNetcode(),
			NetworkVariableReadPermission.Everyone,
			NetworkVariableWritePermission.Server);

		public override void OnNetworkSpawn()
		{
			if (!Instances.ContainsKey(OwnerClientId))
			{
				Instances.Add(OwnerClientId, this);
			}

			gameObject.name = "netcom" + NetworkObjectId;

			SyncedData.OnValueChanged += OnDataChanged;
		}

		public override void OnNetworkDespawn()
		{
			Instances.Remove(OwnerClientId);
			SyncedData.OnValueChanged -= OnDataChanged;
		}

		private void OnDataChanged(DataNetcode prev, DataNetcode current)
		{
			ReceiveEvent?.Invoke(current);
		}

		public void SubmitInfo(DataNetcode info)
		{
			if (IsServer || IsOwner)
			{
				SubmitInfoServerRpc(info);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SubmitInfoServerRpc(DataNetcode data, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			foreach (var kvp in Instances)
			{
				ulong targetClientId = kvp.Key;
				NetCommunication instance = kvp.Value;

				if (targetClientId != senderClientId)
				{
					instance.SyncedData.Value = data;
				}
			}
		}
	}
}