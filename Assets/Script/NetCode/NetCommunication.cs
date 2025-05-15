using Unity.Netcode;
using UnityEngine;

namespace CardGame.Net
{
	public class NetCommunication : NetworkBehaviour
	{
		public static NetCommunication OwnedInstance { get; private set; }

		public delegate void ReceiveEventDelegate(DataNetcode data);
		public event ReceiveEventDelegate ReceiveEvent;

		public NetworkVariable<DataNetcode> SyncedData = new(
			new DataNetcode(),
			NetworkVariableReadPermission.Everyone,
			NetworkVariableWritePermission.Server);

		public override void OnNetworkSpawn()
		{
			if (IsClient)
			{
				OwnedInstance = this;
			}
			SyncedData.OnValueChanged += OnDataChanged;
		}

		public override void OnNetworkDespawn()
		{
			SyncedData.OnValueChanged -= OnDataChanged;
		}

		private void OnDataChanged(DataNetcode prev, DataNetcode current)
		{
			if (!IsServer)
			{
				Debug.Log($"[Client] Data changed: {current.Text}");
				ReceiveEvent?.Invoke(current);
			}
		}

		public void SubmitInfo(DataNetcode info)
		{
			if (IsOwner && IsClient)
			{
				SubmitInfoServerRpc(info);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SubmitInfoServerRpc(DataNetcode data)
		{
			Debug.Log($"[Server] Received from client: {data.Text}");
			SyncedData.Value = data;
		}
	}
}