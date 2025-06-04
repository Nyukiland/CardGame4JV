using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using Unity.Netcode;
using CardGame.Card;
using CardGame.Net;
using UnityEngine;
using CardGame.UI;
using System;

namespace CardGame.Turns
{
	public class NetworkResource : Resource
	{
		public NetCommunication NetCom { get; private set; }

		[SerializeField]
		private DrawPile _pioche;

		[SerializeField]
		private GridManager _grid;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			GetNetComForThisClientAsync().Forget();
		}

		public bool IsNetActive()
		{
			return NetCom != null;
		}

		private async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;

			await UniTask.WaitUntil(() =>
				NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom));

			NetCom = netCom;

			if (NetCom != null)
			{
				NetCom.TileMoved += UpdateTileInMovement;
				NetCom.TilePlaced += UpdateGridPlacement;
				NetCom.GridUpdated += UpdateGrid;
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();

			if (NetCom != null)
			{
				NetCom.TileMoved -= UpdateTileInMovement;
				NetCom.TilePlaced -= UpdateGridPlacement;
				NetCom.GridUpdated -= UpdateGrid;
			}
		}

		private void UpdateTileInMovement(DataToSend data)
		{
			TileData tileReceived = NetUtility.FromDataToTile(data, _pioche.AllTileSettings);
		}

		private void UpdateGridPlacement(DataToSend data)
		{
			TileData tileReceived = NetUtility.FromDataToTile(data, _pioche.AllTileSettings);

			//_grid.SetTile(tileReceived, data.Position.x, data.Position.y);
		}

		private void UpdateGrid(DataToSendList dataList)
		{
			foreach (DataToSend data in dataList.DataList)
			{
				TileData tile = NetUtility.FromDataToTile(data, _pioche.AllTileSettings);

				//if (_grid.GetTile(data.Position.x, data.Position.y) == tile)
				//	continue;

				//_grid.SetTile(tile, data.Position.x, data.Position.y);
			}
		}
	}
}