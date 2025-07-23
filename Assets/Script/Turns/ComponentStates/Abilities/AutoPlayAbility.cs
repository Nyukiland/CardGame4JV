using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.Card;
using UnityEngine;
using System;
using System.Linq;

namespace CardGame.Turns
{
	public class AutoPlayAbility : Ability
	{
		private readonly Vector2Int InvalidPosition = new(-100, -100);

		private GridManagerResource _grid;
		private ScoringAbility _scoring;
		private DrawPile _drawPile;

		[SerializeField]
		private float _waitSec = 2f;

		[SerializeField]
		private List<TileWithBestPlacement> _tilesInHand = new();

		private List<SurroundingAndCount> _surroundingTileDecomposed = new();

		public bool IsFinished { get; private set; }

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_grid = owner.GetStateComponent<GridManagerResource>();
			_scoring = owner.GetStateComponent<ScoringAbility>();
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

				TileWithBestPlacement tileBP = new TileWithBestPlacement()
				{
					Tile = tileData
				};

				if (_surroundingTileDecomposed.Count != 0)
				{
					foreach (SurroundingAndCount surrounding in _surroundingTileDecomposed)
					{
						UpdateTileInfo(surrounding, ref tileBP);
					}
				}

				_tilesInHand.Add(tileBP);
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
			await UniTask.WaitForEndOfFrame();

			if (_tilesInHand.Count == 0)
			{
				IsFinished = true;
				return;
			}

			UpdateSurroundingTileInfo();

			await UniTask.NextFrame();
			await UniTask.WaitForEndOfFrame();

			FindBestPlacement(out TileWithBestPlacement tileToPlay, out Vector2Int pos, out int rotation, out int connection);


			if (pos == InvalidPosition)
			{
				UnityEngine.Debug.LogWarning($"[{nameof(AutoPlayAbility)}] Failed to place tile due to no valid placement");
			}
			else
			{
				tileToPlay.Tile.HasFlag = GameManager.Instance.FlagTurn;

				string text = "Tile \n";
				text += $"\t rot: {rotation} \n";

				foreach (ZoneData data in tileToPlay.Tile.Zones)
				{
					text += $"Zone: {data.environment} \n";
				}
				text += "\n";

				UnityEngine.Debug.Log(text);

				for (int i = 0; i < rotation; i++)
					tileToPlay.Tile.RotateTile();

				_grid.SetTile(tileToPlay.Tile, pos);
				_tilesInHand.Remove(tileToPlay);
				_scoring.SetScoringPos(pos);
				GenerateTheoreticalHand(connection);
			}

			GameManager.Instance.SoloTurns++;
			IsFinished = true;

			await UniTask.NextFrame();
			await UniTask.WaitForEndOfFrame();

			UpdateSurroundingTileInfo();
		}

		private void UpdateSurroundingTileInfo()
		{
			for (int i = _surroundingTileDecomposed.Count - 1; i >= 0; i--)
			{
				SurroundingAndCount surrounding = _surroundingTileDecomposed[i];

				if (!_grid.SurroundingTilePos.Contains(surrounding.Pos)
					|| _grid.LastSurroundingUpdated.Contains(surrounding.Pos))
				{
					_surroundingTileDecomposed.Remove(surrounding);
				}
			}

			foreach (Vector2Int pos in _grid.LastSurroundingUpdated)
			{
				SurroundingAndCount surrounding = new();
				surrounding.Pos = pos;

				GetInfoFromSurroundingTile(ref surrounding);

				for (int i = 0; i < _tilesInHand.Count; i++)
				{
					TileWithBestPlacement tileBP = _tilesInHand[i];
					UpdateTileInfo(surrounding, ref tileBP);
					_tilesInHand[i] = tileBP;
				}

				_surroundingTileDecomposed.Add(surrounding);
			}
		}

		private void UpdateTileInfo(SurroundingAndCount surrounding, ref TileWithBestPlacement tileBP)
		{
			if (TileMatches(surrounding.Environments, tileBP.Tile.Zones, out int rotation))
			{
				if (tileBP.BestScore < surrounding.PotentialScore)
				{
					tileBP.BestScore = surrounding.PotentialScore;
					tileBP.BestPositionForScore = surrounding.Pos;
					tileBP.BestRotationForScore = rotation;
				}

				if (tileBP.BestConnection < surrounding.SurroundingCount)
				{
					tileBP.BestConnection = surrounding.SurroundingCount;
					tileBP.BestPositionForConnection = surrounding.Pos;
					tileBP.BestRotationForConnection = rotation;
				}
			}

			bool TileMatches(ENVIRONEMENT_TYPE[] boardHint, ZoneData[] tileEdges, out int r)
			{
				for (r = 0; r < 4; r++)
				{
					bool valid = true;
					for (int i = 0; i < 4; i++)
					{
						ENVIRONEMENT_TYPE boardColor = boardHint[i];
						ENVIRONEMENT_TYPE tileColor = tileEdges[(i + r) % 4].environment;

						if (boardColor != ENVIRONEMENT_TYPE.None && boardColor != tileColor)
						{
							valid = false;
							break;
						}
					}

					if (valid)
						return true; // Match found with rotation r
				}
				return false;
			}

		}

		private void GetInfoFromSurroundingTile(ref SurroundingAndCount surrounding)
		{
			int x = surrounding.Pos.x;
			int y = surrounding.Pos.y;

			if (x + 1 <= _grid.Width - 1) ModifySurroundingData(x + 1, y, 1, ref surrounding);
			if (x - 1 >= 0) ModifySurroundingData(x - 1, y, 3, ref surrounding);
			if (y + 1 <= _grid.Height - 1) ModifySurroundingData(x, y + 1, 0, ref surrounding);
			if (y - 1 >= 0) ModifySurroundingData(x, y - 1, 2, ref surrounding);

			void ModifySurroundingData(int x, int y, int index, ref SurroundingAndCount surrounding)
			{
				TileData tile = _grid.GetTile(x, y).TileData;

				if (tile == null)
				{
					surrounding.Environments[index] = ENVIRONEMENT_TYPE.None;
					return;
				}

				int oppositeIndex = index + 2;
				if (oppositeIndex > 3) oppositeIndex -= 4;

				surrounding.Environments[index] = tile.Zones[oppositeIndex].environment;
				surrounding.PotentialScore += tile.Zones[oppositeIndex].Region.Tiles.Count;
				surrounding.SurroundingCount++;
			}
		}

		private void FindBestPlacement(out TileWithBestPlacement tileBP, out Vector2Int pos, out int rotation, out int connection)
		{
			tileBP = _tilesInHand[0];
			pos = InvalidPosition;
			rotation = 0;
			connection = 0;

			if (GameManager.Instance.FlagTurn)
			{
				_tilesInHand = _tilesInHand
					.OrderByDescending(x => x.BestScore)
					.ToList();

				for (int i = 0; i < _tilesInHand.Count; i++)
				{
					if (_grid.SurroundingTilePos.Contains(_tilesInHand[i].BestPositionForScore))
					{
						tileBP = _tilesInHand[i];
						pos = _tilesInHand[i].BestPositionForScore;
						rotation = _tilesInHand[i].BestRotationForScore;
						connection = _surroundingTileDecomposed
							.FirstOrDefault(x => x.Pos == _tilesInHand[i].BestPositionForScore)
							.SurroundingCount;
						break;
					}
					else
					{
						foreach (SurroundingAndCount surrounding in _surroundingTileDecomposed)
						{
							TileWithBestPlacement tile = _tilesInHand[i];
							UpdateTileInfo(surrounding, ref tile);
							_tilesInHand[i] = tile;
						}
					}
				}
			}
			else
			{
				_tilesInHand = _tilesInHand
					.OrderByDescending(x => x.BestConnection)
					.ToList();

				for (int i = 0; i < _tilesInHand.Count; i++)
				{
					if (_grid.SurroundingTilePos.Contains(_tilesInHand[i].BestPositionForConnection))
					{
						tileBP = _tilesInHand[i];
						pos = _tilesInHand[i].BestPositionForConnection;
						rotation = _tilesInHand[i].BestRotationForConnection;
						connection = _surroundingTileDecomposed
							.FirstOrDefault(x => x.Pos == _tilesInHand[i].BestPositionForConnection)
							.SurroundingCount;
						break;
					}
					else
					{
						foreach (SurroundingAndCount surrounding in _surroundingTileDecomposed)
						{
							TileWithBestPlacement tile = _tilesInHand[i];
							UpdateTileInfo(surrounding, ref tile);
							_tilesInHand[i] = tile;
						}
					}
				}
			}
		}

		public override string DisplayInfo()
		{
			string text = "";

			text += "---Surrounding---\n";

			for (int i = 0; i < _surroundingTileDecomposed.Count; i++)
			{
				text += "Surrounding " + i;
				text += $"\t count: {_surroundingTileDecomposed[i].SurroundingCount} \n" +
					$"\t score: {_surroundingTileDecomposed[i].PotentialScore} \n";

				foreach (ENVIRONEMENT_TYPE type in _surroundingTileDecomposed[i].Environments)
				{
					text += $"\t zone: {type}\n";
				}
				text += "\n";
			}

			text += "\n---Tile---\n";

			for (int i = 0; i < _tilesInHand.Count; i++)
			{
				text += "Tile " + i + "\n";
				text += $"\t count: {_tilesInHand[i].BestConnection} \n" +
					$"\t score: {_tilesInHand[i].BestScore} \n";

				foreach (ZoneData data in _tilesInHand[i].Tile.Zones)
				{
					text += $"Zone: {data.environment} \n";
				}
				text += "\n";
			}

			return text;
		}

		public class TileWithBestPlacement
		{
			public TileData Tile;
			public int BestRotationForScore;
			public Vector2Int BestPositionForScore;
			public int BestScore;

			public int BestRotationForConnection;
			public Vector2Int BestPositionForConnection;
			public int BestConnection;
		}

		public class SurroundingAndCount
		{
			public Vector2Int Pos;
			public int SurroundingCount;
			public int PotentialScore;
			public ENVIRONEMENT_TYPE[] Environments = new ENVIRONEMENT_TYPE[4];
		}
	}
}