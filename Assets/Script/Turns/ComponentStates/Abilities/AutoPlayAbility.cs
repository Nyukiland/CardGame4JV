using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.Card;
using UnityEngine;

namespace CardGame.Turns
{
	public class AutoPlayAbility : Ability
	{
		private GridManagerResource _grid;
		private DrawPile _drawPile;

		[SerializeField]
		private float _waitSec = 2f;

		[SerializeField]
		private List<TileData> _tilesInHand = new();

		public bool IsFinished
		{
			get;
			private set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_grid = Owner.GetStateComponent<GridManagerResource>();
		}

		public override void LateInit()
		{
			base.LateInit();
			_drawPile = Storage.Instance.GetElement<DrawPile>();
		}

		public override void OnEnable()
		{
			base.OnEnable();
			GameManager.Instance.SoloTurns++;
			AutoPlay().Forget();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			IsFinished = false;
		}

		public void GenerateTheoreticalHand(int count)
		{
			for (int i = 0; i < count; i++)
			{
				TileSettings tileSettings = _drawPile.GetTileFromDrawPile();

				if (tileSettings == null) return;

				TileData tileData = new();
				tileData.InitTile(tileSettings);
				tileData.OwnerPlayerIndex = 1;

				_tilesInHand.Add(tileData);
			}
		}

		//need to split that in multiple unitask and await them
		private async UniTask AutoPlay()
		{
			await UniTask.WaitForSeconds(_waitSec);

			if (_tilesInHand.Count == 0)
			{
				Owner.SetState<PlaceTileCombinedState>();
				return;
			}

			//----------------------------------------
			//find card placement
			//fun triple loop
			Vector2Int tilePlaced = new(-100, -100);
			TileData tileData = null;
			int connection = 0;
			foreach (TileData tile in _tilesInHand)
			{
				foreach (Vector2Int pos in _grid.SurroundingTilePos)
				{
					for (int i = 0; i < 4; i++)
					{
						connection = _grid.GetPlacementConnectionCount(tile, pos);
						if (connection != 0)
						{
							tileData = tile;
							tilePlaced = pos;
							break;
						}
						else
						{
							tile.RotateTile();
						}
					}

					if (tilePlaced != new Vector2Int(-100, -100)) break;
				}

				if (tilePlaced != new Vector2Int(-100, -100)) break;
			}

			//-------------------------
			//actually play the tile
			if (tilePlaced == new Vector2Int(-100, -100))
			{
				UnityEngine.Debug.LogWarning($"[{nameof(AutoPlayAbility)}] Failed to place tile due to no valid placement");
			}
			else
			{
				tileData.HasFlag = GameManager.Instance.FlagTurn;
				_grid.SetTile(tileData, tilePlaced);
				_tilesInHand.Remove(tileData);
				GenerateTheoreticalHand(connection);
			}

			GameManager.Instance.SoloTurns++;
			IsFinished = true;
		}

		public override string DisplayInfo()
		{
			return $"Tile in hand: {_tilesInHand.Count} \n";
		}
	}
}