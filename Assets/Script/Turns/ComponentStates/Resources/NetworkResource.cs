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
		private TauntShakeTileAbility _tauntShakeTile;
		private GridManagerResource _grid;

		[SerializeField]
		private DrawPile _drawPile;


		public bool IsFinished
		{
			get;
			set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_createHand = owner.GetStateComponent<CreateHandAbility>();
			_tauntShakeTile = owner.GetStateComponent<TauntShakeTileAbility>();
			_grid = owner.GetStateComponent<GridManagerResource>();

			if (GameManager.Instance.IsNetCurrentlyActive())
				GetNetComForThisClientAsync().Forget();
		}

		public bool IsNetActive()
		{
			return NetCom != null;
		}

		private async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;

			int increment = 0;

			while (true)
			{
				NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom);

				if (netCom != null || increment >= 300) break;
				increment++;

				await UniTask.Yield();
			}

			NetCom = netCom;

			if (NetCom != null)
			{
				NetCom.TileMoved += UpdateTileInMovement;
				NetCom.TilePlaced += UpdateGridPlaced;
				NetCom.GridUpdated += UpdateGrid;
				NetCom.TileForHand += UpdateHand;
				NetCom.SendYourTurn += GoMyTurn;
				NetCom.SendTauntShake += ShakeTile;

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
				NetCom.TileForHand -= UpdateHand;
				NetCom.SendYourTurn -= GoMyTurn;
				NetCom.SendTauntShake -= ShakeTile;
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
			TileSettings tileSettings = _drawPile.GetTileFromID(ID);

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

		private void ShakeTile(Vector2 pos, bool special)
		{
			_tauntShakeTile.ShakeTileVisu(_grid.GetTile(Vector2Int.CeilToInt(pos)), special);
		}
	}
}