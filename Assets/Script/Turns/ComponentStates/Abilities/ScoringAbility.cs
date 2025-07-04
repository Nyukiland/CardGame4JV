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
		} = new Vector2Int(-100, -100);

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
			foreach (ZoneData zone in _tilePlaced.Zones)
			{
				if (
					zone.Region.OpeningCount == 0 &&
					zone.environment != ENVIRONEMENT_TYPE.Neutral &&
					zone.Region.AlreadyScored == false)
				{
					_closedRegionsInTurn.Add(zone.Region);
					//Debug.Log("Score tile : " + TilePlacedPosition.x + " - " + TilePlacedPosition.y);
					ScoreClassicTiles(zone.Region);
					ScoreFlagTiles(zone.Region);
					Debug.Log("-------------------------");
					zone.Region.AlreadyScored = true;

				}
			}
			VisualFeedbackAtScoringAsync().Forget();
		}


		private async UniTask VisualFeedbackAtScoringAsync()
		{

			foreach (Region closedRegion in _closedRegionsInTurn)
			{
				foreach (TileVisu tileVisu in closedRegion.Tiles)
				{
					//Debug.Log("Shake tile : " + tileVisu.PositionOnGrid.x + " - " + tileVisu.PositionOnGrid.y);

					tileVisu.transform.DOShakePosition(0.1f);
					await UniTask.WaitForSeconds(0.5f);
				}
			}
			//await UniTask.WaitForSeconds(3.0f);
			IsScoringFinished = true; //fin
		}

		private void ScoreClassicTiles(Region Region)
		{
			Dictionary<int, int> playersTileNumber = new();
			Debug.Log("Score tuiles classiques ? ");
			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// les tuiles avec flag ou sans player défini (-1) ne sont pas comptabilisées :
				if (tile.HasFlag == true) continue;
				if (tile.OwnerPlayerIndex == -1) continue;

				if (!playersTileNumber.ContainsKey(tile.OwnerPlayerIndex))
				{
					playersTileNumber.Add(tile.OwnerPlayerIndex, 0);
				}
				playersTileNumber[tile.OwnerPlayerIndex] = playersTileNumber[tile.OwnerPlayerIndex] + 1;
			}

			foreach (var playerTileNumber in playersTileNumber)
			{
				// Le joueur score le nombre de tuiles lui appartenant présentes dans la zone (hors tuile avec flag) :
				int playerScore = CalculateScore(playerTileNumber.Value);
				Debug.Log("Score tuiles classiques : " + playerScore + " pour le joueur " + playerTileNumber.Key);
				GameManager.Instance.AddScore(playerScore, playerTileNumber.Key);
			}
		}

		private void ScoreFlagTiles(Region Region)
		{
			Dictionary<int, int> playersTileNumber = new();
			Debug.Log("Score tuiles Flag ? ");
			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// on ne veut comptabiliser que les tuiles avec un flag :
				if (tile.HasFlag == false) continue;
				if (playersTileNumber.ContainsKey(tile.OwnerPlayerIndex) == false)
				{
					playersTileNumber.TryAdd(tile.OwnerPlayerIndex, 0);
				}
				playersTileNumber[tile.OwnerPlayerIndex] = playersTileNumber[tile.OwnerPlayerIndex] + 1;
			}

			foreach (var playerVar in playersTileNumber)
			{
				Debug.Log("foreach : PlayerIndex : " + playerVar.Key);

				// Le joueur score le nombre total de tuiles présentes dans la zone par case flag lui appartenant présente dans la zone :
				int playerScore = CalculateScore(Region.Tiles.Count) * playerVar.Value;

				Debug.Log("Score tuiles Flag : " + playerScore);
				GameManager.Instance.AddScore(playerScore, playerVar.Key);
			}
		}

		private int CalculateScore(int TilesNumber)
		{
			return TilesNumber * (TilesNumber + 1) / 2;
		}
	}
}