using CardGame.StateMachine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using DG.Tweening;
using CardGame.Card;
using CardGame.Utility;
using UnityEngine.SocialPlatforms.Impl;
using NUnit.Framework;
using System.Collections.Generic;

namespace CardGame.Turns
{
	public class ScoringAbility : Ability
	{
		private GridManagerResource _gridManager;

		public Vector2Int TilePlaced
		{
			get;
			private set;
		}

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
			TilePlaced = pos;
		}

		public void SetState(Type type)
		{
			NextState = type;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			TilePlaced = new(-100, -100);
			IsScoringFinished = false;
		}

		public void CallScoring()
		{
			TileData Tile = _gridManager.GetTile(TilePlaced.x, TilePlaced.y).TileData;
			foreach (ZoneData Zone in Tile.Zones)
			{
				if (
					Zone.Region.OpeningCount == 0 &&
					Zone.environment != ENVIRONEMENT_TYPE.Neutral &&
					Zone.Region.AlreadyScored == false)
				{
					Debug.Log("Score tile : " + TilePlaced.x + " - " + TilePlaced.y);
					ScoreClassicTiles(Zone.Region);
					ScoreFlagTiles(Zone.Region);
					Zone.Region.AlreadyScored = true;

				}
			}
			ScoringAsync().Forget();
		}

		private async UniTask ScoringAsync()
		{
			Owner.transform.DOShakePosition(0.1f);

			await UniTask.Yield();

			IsScoringFinished = true; //fin
		}

		private void ScoreClassicTiles(Region Region)
		{
			Dictionary<int, int> PlayersTileNumber = new();
			Debug.Log("Score tuiles classiques ? ");
			foreach (TileData Tile in Region.Tiles)
			{
				Debug.Log("foreach : Tile de la region ");
				// les tuiles avec flag ne sont pas comptabilisées
				if (Tile.HasFlag == true) continue;
				if (Tile.OwnerPlayerIndex == -1) continue; // -1 correspond à la tile centrale qui n'appartient à personne. Elle ne nous intéresse pas.
				if(PlayersTileNumber.ContainsKey(Tile.OwnerPlayerIndex) == false)
				{
					PlayersTileNumber.TryAdd(Tile.OwnerPlayerIndex, 0);
				}
				PlayersTileNumber.TryAdd(Tile.OwnerPlayerIndex, PlayersTileNumber[Tile.OwnerPlayerIndex]++);
			}

			foreach (int PlayerIndex in PlayersTileNumber.Keys)
			{
				Debug.Log("foreach : PlayerIndex : " + PlayerIndex);
				// Le joueur score le nombre de tuiles lui appartenant présentes dans la zone (hors tuile avec flag) :
				int PlayerScore = CalculateScore(PlayersTileNumber[PlayerIndex]);
				Debug.Log("Score tuiles classiques : " + PlayerScore);
				GameManager.Instance.AddScore(PlayerScore, PlayerIndex);
			}
			
		}

		private void ScoreFlagTiles(Region Region)
		{
			Dictionary<int, int> PlayersTileNumber = new();
			Debug.Log("Score tuiles Flag ? ");
			foreach (TileData Tile in Region.Tiles)
			{
				Debug.Log("foreach : Tile de la region ");
				// on ne veut comptabiliser que les tuiles avec un flag :
				if (Tile.HasFlag == false) continue;
				if (PlayersTileNumber.ContainsKey(Tile.OwnerPlayerIndex) == false)
				{
					PlayersTileNumber.TryAdd(Tile.OwnerPlayerIndex, 0);
				}
				PlayersTileNumber.Add(Tile.OwnerPlayerIndex, PlayersTileNumber[Tile.OwnerPlayerIndex]++);
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
			return TilesNumber * (TilesNumber + 1) / 2 ; 
		}
	}
}