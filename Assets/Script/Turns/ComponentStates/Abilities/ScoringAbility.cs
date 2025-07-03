using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringAbility : Ability
	{
		private GridManagerResource _gridManager;

		public Vector2Int TilePlacedPosition
		{
			get;
			private set;
		}

		private TileData _tilePlaced;

		private HashSet<Region> _closedRegionsInTurn = new();

		public Type NextState
		{
			get;
			private set;
		}

		public bool IsScoringFinished
		{
			get;
			private set;
		}

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_gridManager = owner.GetStateComponent<GridManagerResource>();
		}

		public void SetScoringPos(Vector2Int pos)
		{
			TilePlacedPosition = pos;
		}

		public void SetState(Type type)
		{
			NextState = type;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			TilePlacedPosition = new(-100, -100);
			IsScoringFinished = false;
			_tilePlaced = null;
			_closedRegionsInTurn = new();

		}

		public void CallScoring()
		{
			_tilePlaced = _gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).TileData;
			foreach (ZoneData Zone in _tilePlaced.Zones)
			{
				if (
					Zone.Region.OpeningCount == 0 &&
					Zone.environment != ENVIRONEMENT_TYPE.Neutral &&
					Zone.Region.AlreadyScored == false)
				{
					_closedRegionsInTurn.Add(Zone.Region);
					Debug.Log("Score tile : " + TilePlacedPosition.x + " - " + TilePlacedPosition.y);
					ScoreClassicTiles(Zone.Region);
					ScoreFlagTiles(Zone.Region);
					Zone.Region.AlreadyScored = true;

				}
			}
			VisualFeedbackAtScoringAsync().Forget();
		}


		private async UniTask VisualFeedbackAtScoringAsync()
		{

			foreach (var ClosedRegion in _closedRegionsInTurn)
			{
				foreach (var TileVisu in ClosedRegion.Tiles)
				{
					Debug.Log("Shake tile : " + TileVisu.PositionOnGrid.x + " - " + TileVisu.PositionOnGrid.y);

					TileVisu.transform.DOShakePosition(0.1f);
					await UniTask.WaitForSeconds(0.5f);
				}
			}
			await UniTask.WaitForSeconds(3.0f);
			IsScoringFinished = true; //fin
		}

		private void ScoreClassicTiles(Region Region)
		{
			Dictionary<int, int> PlayersTileNumber = new();
			Debug.Log("Score tuiles classiques ? ");
			foreach (TileVisu TileVisu in Region.Tiles)
			{
				TileData Tile = TileVisu.TileData;

				// les tuiles avec flag ou sans player défini (-1) ne sont pas comptabilisées :
				if (Tile.HasFlag == true) continue;
				if (Tile.OwnerPlayerIndex == -1) continue;

				if (PlayersTileNumber.ContainsKey(Tile.OwnerPlayerIndex) == false)
				{
					PlayersTileNumber.Add(Tile.OwnerPlayerIndex, 0);
				}
				PlayersTileNumber[Tile.OwnerPlayerIndex] = PlayersTileNumber[Tile.OwnerPlayerIndex] + 1;
			}

			foreach (var PlayerTileNumber in PlayersTileNumber)
			{
				Debug.Log("foreach : PlayerIndex : " + PlayerTileNumber.Key);
				// Le joueur score le nombre de tuiles lui appartenant présentes dans la zone (hors tuile avec flag) :
				int PlayerScore = CalculateScore(PlayerTileNumber.Value);
				Debug.Log("Score tuiles classiques : " + PlayerScore);
				GameManager.Instance.AddScore(PlayerScore, PlayerTileNumber.Key);
			}
		}

		private void ScoreFlagTiles(Region Region)
		{
			Dictionary<int, int> PlayersTileNumber = new();
			Debug.Log("Score tuiles Flag ? ");
			foreach (TileVisu TileVisu in Region.Tiles)
			{
				TileData Tile = TileVisu.TileData;

				// on ne veut comptabiliser que les tuiles avec un flag :
				if (Tile.HasFlag == false) continue;
				if (PlayersTileNumber.ContainsKey(Tile.OwnerPlayerIndex) == false)
				{
					PlayersTileNumber.TryAdd(Tile.OwnerPlayerIndex, 0);
				}
				PlayersTileNumber[Tile.OwnerPlayerIndex] = PlayersTileNumber[Tile.OwnerPlayerIndex] + 1;
			}

			foreach (int PlayerIndex in PlayersTileNumber.Keys)
			{
				Debug.Log("foreach : PlayerIndex : " + PlayerIndex);

				// Le joueur score le nombre total de tuiles présentes dans la zone par case flag lui appartenant présente dans la zone :
				int PlayerScore = CalculateScore(Region.Tiles.Count) * PlayersTileNumber[PlayerIndex];

				Debug.Log("Score tuiles Flag : " + PlayerScore);
				GameManager.Instance.AddScore(PlayerScore, PlayerIndex);
			}
		}

		private int CalculateScore(int TilesNumber)
		{
			return TilesNumber * (TilesNumber + 1) / 2;
		}
	}
}