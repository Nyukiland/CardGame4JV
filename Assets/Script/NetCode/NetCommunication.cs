using System.Collections.Generic;
using CardGame.Utility;
using Unity.Netcode;
using CardGame.Card;
using CardGame.UI;
using System;

namespace CardGame.Net
{
	public class NetCommunication : NetworkBehaviour
	{
		public static Dictionary<ulong, NetCommunication> Instances = new();

		public delegate void SendTileInfoEvent(DataToSend data);
		public event SendTileInfoEvent TileMoved;
		public event SendTileInfoEvent TilePlaced;

		public delegate void SendGridEvent(DataToSendList data);
		public event SendGridEvent GridUpdated;

		public delegate void SendTileForHandEvent(int tileId);
		public event SendTileForHandEvent TileForHand;

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
				SendMoveTileServerRPC(data);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendMoveTileServerRPC(DataToSend data, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			ForEachOtherClient(senderClientId, x => x.DistributeMovementTileClientRPC(data));
		}

		[ClientRpc(RequireOwnership = false)]
		public void DistributeMovementTileClientRPC(DataToSend data)
		{
			TileMoved?.Invoke(data);
		}

		public void SendTilePlaced(DataToSend data)
		{
			if (IsLocalPlayer)
				SendTilePlacedServerRPC(data);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendTilePlacedServerRPC(DataToSend data, ServerRpcParams rpcParams = default)
		{
			if (Storage.Instance.GetElement<GridManager>().GetTile(data.Position.x, data.Position.y) != null)
				return;

			ulong senderClientId = rpcParams.Receive.SenderClientId;

			int tileId = Storage.Instance.GetElement<DrawPile>().GetTileIDFromDrawPile();

			ForEachOtherClient(senderClientId, x => x.DistributeTilePlacedClientRPC(data));

			Instances.TryGetValue(senderClientId, out NetCommunication instance);
			instance.GiveNewTileInHandClientRPC(tileId);
		}

		[ClientRpc(RequireOwnership = false)]
		public void DistributeTilePlacedClientRPC(DataToSend data)
		{
			TilePlaced?.Invoke(data);
		}

		[ClientRpc(RequireOwnership = false)]
		public void GiveNewTileInHandClientRPC(int ID)
		{
			TileForHand?.Invoke(ID);
		}

		public void SendGrid(DataToSendList dataList)
		{
			if (IsLocalPlayer)
				SendGridServerRPC(dataList);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendGridServerRPC(DataToSendList dataList, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			GridManager grid = Storage.Instance.GetElement<GridManager>();
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			bool isDifferent = false;

			foreach (DataToSend data in dataList.DataList)
			{
				TileData tile = NetUtility.FromDataToTile(data, drawPile.AllTileSettings);

				//if (grid.GetTile(data.Position.x, data.Position.y) != tile)
				//{
				//	isDifferent = true;
				//	break;
				//}
			}

			if (isDifferent)
			{
				Instances.TryGetValue(senderClientId, out NetCommunication instance);
				instance.DistributeGridClientRPC(dataList);
			}
		}

		[ClientRpc(RequireOwnership = false)]
		public void DistributeGridClientRPC(DataToSendList dataList)
		{
			GridUpdated?.Invoke(dataList);
		}

		private void ForEachOtherClient(ulong senderClientId, Action<NetCommunication> action)
		{
			foreach (var kvp in Instances)
			{
				ulong targetClientId = kvp.Key;
				NetCommunication instance = kvp.Value;

				if (targetClientId != senderClientId)
					action(instance);
			}
		}

		public static TileData FromDataToSend(DataToSend data, List<TileSettings> allTileSettings)
		{
			// Find TileSettings by IdCode
			TileSettings matchingSettings = allTileSettings.Find(ts => ts.IdCode == data.TileSettingsId);
			if (matchingSettings == null)
			{
				UnityEngine.Debug.LogError($"[{nameof(NetCommunication)}] No TileSettings found with IdCode {data.TileSettingsId}");
				return null;
			}

			// Create and initialize TileData
			TileData tile = new TileData();
			tile.InitTile(matchingSettings);

			// Rotate to match received rotation count
			for (int i = 0; i < data.TileRotationCount; i++)
			{
				tile.RotateTile();
			}

			// Override zones in case of changes during gameplay
			for (int i = 0; i < 4; i++)
			{
				tile.Zones[i] = data.Zones[i];
			}

			return tile;
		}

		#region Test

		public delegate void ReceiveEventTestDelegate(string data);
		public event ReceiveEventTestDelegate ReceiveEventTest;

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

			ForEachOtherClient(senderClientId, x => x.OnDataChangedTestClientRpc(data));
		}

		[ClientRpc(RequireOwnership = false)]
		public void OnDataChangedTestClientRpc(string current)
		{
			ReceiveEventTest?.Invoke(current);
		}
		#endregion
	}
}