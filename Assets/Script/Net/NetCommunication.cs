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
		public event SendTileInfoEvent TilePlaced;

		public delegate void SendGridEvent(DataToSendList data);
		public event SendGridEvent GridUpdated;

		public delegate void SendTileForHandEvent(int tileId);
		public event SendTileForHandEvent TileForHand;

		public delegate void SendTauntShakeEvent(Vector2 pos, bool special);
		public event SendTauntShakeEvent SendTauntShake;

		public delegate void TransmitValidation();
		public event TransmitValidation SendYourTurn;

		public Action OnDestroyEvent;
		public Action OnLaunchGameEvent;

		[SerializeField, Disable]
		private GameManager _manager;

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
			if (_manager.OnlinePlayersID.Contains(OwnerClientId) && _manager.OnlineTurns.Value != -1)
				_manager.OnlineTurns.Value = 100;

			Instances.Remove(OwnerClientId);
		}

		public void SendTilePlaced(DataToSend data)
		{
			if (IsLocalPlayer)
				SendTilePlacedServerRPC(data);
		}

		public void SendDiscard(int ID)
		{
			if (IsLocalPlayer)
				SendDiscardTileServerRPC(ID);
		}

		public void GenerateFirstGrid()
		{
			if (IsHost)
			{
				Storage.Instance.GetElement<GridManagerResource>().GenerateBonusTiles();
				SendGridServerRPC();
			}
		}

		public void SendTauntShakeNet(Vector2 pos, bool special)
		{
			if (IsLocalPlayer)
				SendTauntShakeServerRPC(pos, special);
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
		public void SendTilePlacedServerRPC(DataToSend data, ServerRpcParams rpcParams = default)
		{
			//not working for some reason
			//if (Storage.Instance.GetElement<GridManager>().GetTile(data.Position.x, data.Position.y).TileData != null)
			//	return;

			ulong senderClientId = rpcParams.Receive.SenderClientId;

			ForEachOtherClient(senderClientId, x => x.DistributeTilePlacedClientRPC(data));

			TileData tileData = NetUtility.FromDataToTile(data, Storage.Instance.GetElement<DrawPile>().AllTileSettings);

			int connectionCount = Storage.Instance.GetElement<GridManagerResource>()
				.GetPlacementConnectionCount(tileData, data.Position);

			Instances.TryGetValue(senderClientId, out NetCommunication instance);

			for (int i = 0; i < connectionCount; i++)
			{
				int tileId = Storage.Instance.GetElement<DrawPile>().GetTileIDFromDrawPile();
				if (tileId == -1)
				{
					UnityEngine.Debug.LogError($"[{nameof(NetCommunication)}] no Tile to draw");
					return;
				}
				instance.GiveNewTileInHandClientRPC(tileId);
			}
		}

		[ServerRpc (RequireOwnership = false)]
		public void SendDiscardTileServerRPC(int ID)
		{
			Storage.Instance.GetElement<DrawPile>().DiscardTile(ID);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendGridServerRPC(ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			GridManagerResource grid = Storage.Instance.GetElement<GridManagerResource>();
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			DataToSendList list = grid.GetListOfPlacedTile();

			ForEachOtherClient(senderClientId, x => x.DistributeGridClientRPC(list));
		}

		[ServerRpc(RequireOwnership = false)]
		public void TurnCompletedServerRPC()
		{
			_manager.OnlineTurns.Value++;

			if (_manager.GameIsFinished)
				return;

			Instances[_manager.OnlinePlayersID[_manager.PlayerIndexTurn]].CallTurnClientRPC();
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendTauntShakeServerRPC(Vector2 pos, bool special, ServerRpcParams rpcParams = default)
		{
			ulong senderClientId = rpcParams.Receive.SenderClientId;

			ForEachOtherClient(senderClientId, x => x.CallTauntShakeClientRPC(pos, special));
		}

		[ServerRpc(RequireOwnership = true)]
		public void SetUpGameServerRPC(int tileInHand)
		{
			_manager = GameManager.Instance;

			_manager.ResetManager();

			_manager.SetPlayerInfo(OwnerClientId, "Host");
			_manager.SetLocalPlayerInfo();

			//fill the list
			foreach (var instance in Instances)
			{
				for (int i = 0; i < tileInHand; i++)
				{
					instance.Value.GiveNewTileInHandClientRPC(Storage.Instance.GetElement<DrawPile>().GetTileIDFromDrawPile());
				}

				if (instance.Key == OwnerClientId)
					continue;

				_manager.SetPlayerInfo(instance.Value.OwnerClientId, "Client" + _manager.OnlinePlayersID.Count);

				instance.Value.SetUpOnClientRPC(instance.Value.OwnerClientId);
			}

			Instances[_manager.OnlinePlayersID[_manager.PlayerIndexTurn]].CallTurnClientRPC();
		}

		[ServerRpc(RequireOwnership = true)]
		public void LoadSceneServerRPC(string sceneName)
		{
			ForEachOtherClient(0, communication => communication.LoadSceneClientRPC(sceneName));
		}

		#endregion

		#region Client

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
		public void CallTauntShakeClientRPC(Vector2 pos, bool special)
		{
			SendTauntShake?.Invoke(pos, special);
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
		public void SetUpOnClientRPC(ulong owner)
		{
			_manager = GameManager.Instance;
			_manager.SetLocalPlayerInfo();
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