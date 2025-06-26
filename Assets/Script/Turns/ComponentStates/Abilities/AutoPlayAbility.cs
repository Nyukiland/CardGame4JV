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
		private readonly Vector2Int InvalidPosition = new (-100, -100);

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

			if (_tilesInHand.Count == 0)
			{
				GameManager.Instance.SoloTurns++;
				IsFinished = true;
				return;
			}

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

		private async UniTask AutoPlay()
		{
			await UniTask.WaitForSeconds(_waitSec);

			(TileData tileData, Vector2Int tilePlaced, int connection) = await FindTilePlacement();

			await PlayTile(tileData, tilePlaced, connection);
			await EndPlay();
		}

		private async UniTask<(TileData tileData, Vector2Int tilePlaced, int connection)> FindTilePlacement()
		{
			await UniTask.Yield();

			Vector2Int bestPosition = InvalidPosition;
			TileData bestTile = null;
			int bestConnection = 0;
			int bestRotation;

			foreach (TileData tile in _tilesInHand)
			{
				bestRotation = tile.TileRotationCount;

				foreach (Vector2Int pos in _grid.SurroundingTilePos)
				{
					for (int i = 0; i < 4; i++)
					{
						int currentConnection = _grid.GetPlacementConnectionCount(tile, pos);

						if (currentConnection > bestConnection)
						{
							bestConnection = currentConnection;
							bestTile = tile;
							bestPosition = pos;

							bestRotation = tile.TileRotationCount;
						}

						tile.RotateTile(); 
					}
				}

				if (bestTile == tile)
					while (tile.TileRotationCount != bestRotation) tile.RotateTile();
			}

			return (bestTile, bestPosition, bestConnection);
		}


		private async UniTask PlayTile(TileData tileData, Vector2Int tilePlaced, int connection)
		{
			await UniTask.Yield();

			if (tilePlaced == InvalidPosition)
			{
				Debug.LogWarning($"[{nameof(AutoPlayAbility)}] Failed to place tile due to no valid placement");
				return;
			}

			tileData.HasFlag = GameManager.Instance.FlagTurn;
			_grid.SetTile(tileData, tilePlaced);
			_tilesInHand.Remove(tileData);

			GenerateTheoreticalHand(connection);
		}

		private async UniTask EndPlay()
		{
			await UniTask.Yield();

			GameManager.Instance.SoloTurns++;
			IsFinished = true;
		}

		public override string DisplayInfo()
		{
			return $"Tile in hand: {_tilesInHand.Count} \n";
		}
	}
}