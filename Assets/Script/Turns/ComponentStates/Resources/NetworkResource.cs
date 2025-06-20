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

		private CreateHandAbility _createHand;

		[SerializeField]
		private DrawPile _drawPile;

		private GridManagerResource _grid;

		public bool IsFinished
		{
			get; 
			set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_createHand = owner.GetStateComponent<CreateHandAbility>();
            _grid = owner.GetStateComponent<GridManagerResource>();

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
				NetCom.TilePlaced += UpdateGridPlaced;
				NetCom.GridUpdated += UpdateGrid;
				NetCom.TileForHand += UpdateHand;
				NetCom.SendYourTurn += GoMyTurn;
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();

			if (NetCom != null)
			{
				NetCom.TileMoved -= UpdateTileInMovement;
				NetCom.TilePlaced -= UpdateGridPlaced;
				NetCom.GridUpdated -= UpdateGrid;
			}
		}

		private void UpdateTileInMovement(DataToSend data)
		{
			TileData tileReceived = NetUtility.FromDataToTile(data, _drawPile.AllTileSettings);
		}

		private void UpdateGridPlaced(DataToSend data)
		{
			TileData tileReceived = NetUtility.FromDataToTile(data, _drawPile.AllTileSettings);
			_grid.SetTile(tileReceived, data.Position.x, data.Position.y);
		}

		private void UpdateHand(int ID)
		{
			TileSettings tileSettings = null;

			foreach (TileSettings setting in _drawPile.AllTileSettings)
			{
				if (setting.IdCode == ID)
				{
					tileSettings = setting;
					break;
				}
			}

			_createHand.CreateTile(tileSettings);
		}

		private void UpdateGrid(DataToSendList dataList)
		{
			foreach (DataToSend data in dataList.DataList)
			{
				TileData tile = NetUtility.FromDataToTile(data, _drawPile.AllTileSettings);

				if (_grid.GetTile(data.Position.x, data.Position.y).TileData == tile)
					continue;

				_grid.SetTile(tile, data.Position.x, data.Position.y);
			}
		}

		private void GoMyTurn()
		{
			IsFinished = true;
		}
	}
}