using UnityEngine.SceneManagement;
using System.Collections.Generic;
using CardGame.Utility;
using CardGame.Turns;
using Unity.Netcode;
using CardGame.Card;
using UnityEngine;
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

		public delegate void TransmitValidation();
		public event TransmitValidation SendYourTurn;

		public Action OnDestroyEvent;
		public Action OnLaunchGameEvent;

		[SerializeField, Disable]
		private NetDataManager _dataManager;

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
			OnDestroyEvent?.Invoke();
			Instances.Remove(OwnerClientId);
		}

		public void SendPlacementTile(DataToSend data)
		{
			if (IsLocalPlayer)
				SendMoveTileServerRPC(data);
		}

		public void SendTilePlaced(DataToSend data)
		{
			if (IsLocalPlayer)
				SendTilePlacedServerRPC(data);
		}

		public void SendGrid(DataToSendList dataList)
		{
			if (IsLocalPlayer)
				SendGridServerRPC(dataList);
		}

		public void TurnFinished()
		{
			if (IsLocalPlayer)
				TurnCompletedServerRPC();
		}

		public void SetUp(int tileInHand)
		{
			if (IsHost)
				SetUpGameServerRPC(tileInHand);
		}

		public void LoadScene(string sceneName)
		{
			if (IsHost)
				LoadSceneServerRPC(sceneName);
		}

		#region Server

		[ServerRpc(RequireOwnership = false)]
		public void SendMoveTileServerRPC(DataToSend data, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			ForEachOtherClient(senderClientId, x => x.DistributeMovementTileClientRPC(data));
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendTilePlacedServerRPC(DataToSend data, ServerRpcParams rpcParams = default)
		{
			//not working for some reason
			//if (Storage.Instance.GetElement<GridManager>().GetTile(data.Position.x, data.Position.y).TileData != null)
			//	return;

			ulong senderClientId = rpcParams.Receive.SenderClientId;

			ForEachOtherClient(senderClientId, x => x.DistributeTilePlacedClientRPC(data));

            int tileId = Storage.Instance.GetElement<DrawPile>().GetTileIDFromDrawPile();
            if (tileId == -1) return;

            Instances.TryGetValue(senderClientId, out NetCommunication instance);
			instance.GiveNewTileInHandClientRPC(tileId);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendGridServerRPC(DataToSendList dataList, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			GridManagerResource grid = Storage.Instance.GetElement<GridManagerResource>();
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			bool isDifferent = false;

			foreach (DataToSend data in dataList.DataList)
			{
				TileData tile = NetUtility.FromDataToTile(data, drawPile.AllTileSettings);

				if (grid.GetTile(data.Position.x, data.Position.y).TileData != tile)
				{
					isDifferent = true;
					break;
				}
			}

			if (isDifferent)
			{
				Instances.TryGetValue(senderClientId, out NetCommunication instance);
				instance.DistributeGridClientRPC(dataList);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		public void TurnCompletedServerRPC()
		{
			_dataManager.PlayerTurn.Value++;
			if (_dataManager.PlayerTurn.Value >= _dataManager.PlayersID.Count) _dataManager.PlayerTurn.Value = 0;

			Instances[_dataManager.PlayersID[_dataManager.PlayerTurn.Value]].CallTurnClientRPC();
		}

		[ServerRpc(RequireOwnership = true)]
		public void SetUpGameServerRPC(int tileInHand)
		{
			_dataManager = NetDataManager.Instance;

			_dataManager.PlayerTurn.Value = 0;
			_dataManager.PlayersID.Clear();

			_dataManager.PlayersID.Add(OwnerClientId);

			//fill the list
			foreach (var instance in Instances)
			{
				for (int i = 0; i < tileInHand; i++)
				{
					instance.Value.GiveNewTileInHandClientRPC(Storage.Instance.GetElement<DrawPile>().GetTileIDFromDrawPile());
				}

				if (instance.Key == OwnerClientId)
					continue;

				instance.Value.SetUpOnClientRPC();
			}

			Instances[_dataManager.PlayersID[_dataManager.PlayerTurn.Value]].CallTurnClientRPC();
		}

		[ServerRpc(RequireOwnership = true)]
		public void LoadSceneServerRPC(string sceneName)
		{
			ForEachOtherClient(0, communication => communication.LoadSceneClientRPC(sceneName));
		}

		#endregion

		#region Client

		[ClientRpc(RequireOwnership = false)]
		public void DistributeMovementTileClientRPC(DataToSend data)
		{
			TileMoved?.Invoke(data);
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

		[ClientRpc(RequireOwnership = false)]
		public void DistributeGridClientRPC(DataToSendList dataList)
		{
			GridUpdated?.Invoke(dataList);
		}

		[ClientRpc(RequireOwnership = false)]
		public void CallTurnClientRPC()
		{
			SendYourTurn?.Invoke();
		}

		[ClientRpc(RequireOwnership = false)]
		public void LoadSceneClientRPC(string sceneName)
		{
			OnLaunchGameEvent?.Invoke();
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}

		[ClientRpc(RequireOwnership = false)]
		public void SetUpOnClientRPC()
		{
			_dataManager = NetDataManager.Instance;
			_dataManager.PlayersID.Add(OwnerClientId);
		}
		#endregion

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
	}
}