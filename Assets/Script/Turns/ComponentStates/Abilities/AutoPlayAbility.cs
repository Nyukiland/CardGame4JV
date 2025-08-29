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
		private readonly Vector2Int InvalidPosition = new(-100, -100);

		private GridManagerResource _grid;
		private ScoringAbility _scoring;
		private DrawPile _drawPile;
		private SoundResource _sound;

		[SerializeField]
		private float _waitSec = 2f;

		[SerializeField]
		private List<TileData> _tilesInHand = new();

		public bool IsFinished { get; private set; }

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_grid = owner.GetStateComponent<GridManagerResource>();
			_scoring = owner.GetStateComponent<ScoringAbility>();
			_sound = owner.GetStateComponent<SoundResource>();
		}

		public override void LateInit()
		{
			base.LateInit();
			_drawPile = Storage.Instance.GetElement<DrawPile>();
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

		public void CallBotTurn()
		{
			GameManager.Instance.SoloTurns++;
			AutoPlay().Forget();
		}

		private async UniTask AutoPlay()
		{
			await UniTask.WaitForSeconds(_waitSec);

			if (_tilesInHand.Count == 0)
			{
				IsFinished = true;
				return;
			}

			(TileData tile, Vector2Int pos, int connection) = FindBestPlacement();

			if (pos == InvalidPosition)
			{
				UnityEngine.Debug.LogWarning($"[{nameof(AutoPlayAbility)}] Failed to place tile due to no valid placement");
			}
			else
			{
				tile.HasFlag = GameManager.Instance.FlagTurn;
				_grid.SetTile(tile, pos);
				_sound.PlayTilePlaced(false);
				_tilesInHand.Remove(tile);
				_scoring.SetScoringPos(pos);
				GenerateTheoreticalHand(connection);
			}

			GameManager.Instance.SoloTurns++;
			IsFinished = true;
		}

		private (TileData tile, Vector2Int pos, int connection) FindBestPlacement()
		{
			TileData bestTile = null;
			Vector2Int bestPos = InvalidPosition;
			int bestConnection = 0;

			foreach (TileData tile in _tilesInHand)
			{
				foreach (Vector2Int pos in _grid.SurroundingTilePos)
				{
					int connection = _grid.GetPlacementConnectionCount(tile, pos);
					if (connection > bestConnection)
					{
						bestTile = tile;
						bestPos = pos;
						bestConnection = connection;
					}
				}
			}

			return (bestTile, bestPos, bestConnection);
		}

		public override string DisplayInfo()
		{
			return $"Tile in hand: {_tilesInHand.Count} \n";
		}
	}
}