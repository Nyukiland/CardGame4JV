using System.Collections.Generic;
using Unity.Netcode;

namespace CardGame.Net
{
	public class NetCommunication : NetworkBehaviour
	{
		public static Dictionary<ulong, NetCommunication> Instances = new();

		public delegate void ReceiveEventDelegate(string data);
		public event ReceiveEventDelegate ReceiveEvent;

		public override void OnNetworkSpawn()
		{
			if (!Instances.ContainsKey(OwnerClientId))
			{
				Instances.Add(OwnerClientId, this);
			}

			gameObject.name = "netcom" + NetworkObjectId;
		}

		public override void OnNetworkDespawn()
		{
			Instances.Remove(OwnerClientId);
		}

		public void SendPlacementTile(DataToSend data)
		{
			if (IsLocalPlayer)
				DistributePlacementTileClientRPC(data);

		}

		[ServerRpc(RequireOwnership = false)]
		public void DistributePlacementTileServerRPC(DataToSend data)
		{

		}

		[ClientRpc(RequireOwnership = false)]
		public void DistributePlacementTileClientRPC(DataToSend data)
		{

		}

		#region Test
		public void SubmitInfoTest(string info)
		{
			if (IsLocalPlayer)
			{
				SubmitInfoTestServerRpc(info);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SubmitInfoTestServerRpc(string data, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			foreach (var kvp in Instances)
			{
				ulong targetClientId = kvp.Key;
				NetCommunication instance = kvp.Value;

				if (targetClientId != senderClientId)
				{
					instance.OnDataChangedTestClientRpc(data);
				}
			}
		}

		[ClientRpc(RequireOwnership = false)]
		public void OnDataChangedTestClientRpc(string current)
		{
			ReceiveEvent?.Invoke(current);
		}
		#endregion
	}
}