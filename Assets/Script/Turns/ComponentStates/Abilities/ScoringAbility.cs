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
		[SerializeField]
		[LockUser]
		private MeshOverlapAlphaFeature _alphaFeature;

		private GridManagerResource _gridManager;
		private SoundResource _sound;

		public Vector2Int TilePlacedPosition
		{
			get;
			private set;
		} = new Vector2Int(-100, -100);

		private TileData _tilePlaced;

		private HashSet<Region> _closedRegionsInTurn = new();

		[Header("ClosedRegionFeedback")]
		[SerializeField] private float _interval = 2f;
		[SerializeField] private float _moveAmount = 1f;
		[SerializeField] private float _moveDuration = 1f;

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
			_sound = owner.GetStateComponent<SoundResource>();
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

			_alphaFeature.settings.TargetMeshes.Clear();
		}

		public void CallScoring()
		{
			if (TilePlacedPosition == new Vector2Int(-100, -100))
			{
				IsScoringFinished = true;
				return;
			}

			_tilePlaced = _gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).TileData;

			//EXEMPLE -------------------------

			//Add the mesh that need to be selected
			//_alphaFeature.settings.TargetMeshes.Add(_gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).VisuNorth);
			//_alphaFeature.settings.TargetMeshes.Add(_gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).VisuSouth);
			//_alphaFeature.settings.TargetMeshes.Add(_gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).VisuEast);
			//_alphaFeature.settings.TargetMeshes.Add(_gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).VisuWest);

			////Set the color of the visual
			//Shader.SetGlobalColor("_MainScoringColor", Color.red);

			//-------------------------

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
					//Debug.Log("-------------------------");
					zone.Region.AlreadyScored = true;

				}
			}

			// give colour to the closed region zones : 
			foreach (Region closedRegion in _closedRegionsInTurn)
			{
				foreach (TileVisu tileVisu in closedRegion.Tiles)
				{
					for (int i = 0; i <= 3; i++)
					{
						if (tileVisu.TileData.Zones[i].Region == closedRegion)
						{
							switch (i)
							{
								case 0:
									Debug.Log("Zone : Nord ");
									_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuNorth);
									break;
								case 1:
									Debug.Log("Zone : Est ");
									_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuEast);
									break;
								case 2:
									Debug.Log("Zone : Sud ");
									_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuSouth);
									break;
								case 3:
									Debug.Log("Zone : Ouest ");
									_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuWest);
									break;

							}
						}
					}
				}
			}
			Shader.SetGlobalColor("_MainScoringColor", Color.blue);
			VisualFeedbackAtScoringAsync().Forget();
		}


		private async UniTask VisualFeedbackAtScoringAsync()
		{
			await UniTask.WaitForSeconds(1f); //wait a little for tile placement feedback

			foreach (Region closedRegion in _closedRegionsInTurn)
			{
				foreach (TileVisu tileVisu in closedRegion.Tiles)
				{
					//Debug.Log("Shake tile : " + tileVisu.PositionOnGrid.x + " - " + tileVisu.PositionOnGrid.y);

					_sound.PlayScoring(tileVisu.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex);

					// Move up the closed regions tiles : 
					// if the tile is already moving (because it is the last placed tile for instance),
					// reset the movement before begining the new one - MARCHE PAS :'(
					if (tileVisu.transform.localPosition.z > 0) tileVisu.transform.DORestart();
					tileVisu.transform.DOLocalMoveZ(tileVisu.transform.localPosition.z - _moveAmount, _moveDuration)
						.SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutCubic);
				}
			}
			//Set the color of the visual
			IsScoringFinished = true; //fin
		}

		private void ScoreClassicTiles(Region Region)
		{
			Dictionary<int, int> playersTileNumber = new();
			//Debug.Log("Score tuiles classiques ? ");
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
				//Debug.Log("Score tuiles classiques : " + playerScore + " pour le joueur " + playerTileNumber.Key);
				GameManager.Instance.AddScore(playerScore, playerTileNumber.Key);
			}
		}

		private void ScoreFlagTiles(Region Region)
		{
			Dictionary<int, int> playersTileNumber = new();
			//Debug.Log("Score tuiles Flag ? ");
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
				//Debug.Log("foreach : PlayerIndex : " + playerVar.Key);

				// Le joueur score le nombre total de tuiles présentes dans la zone par case flag lui appartenant présente dans la zone :
				int playerScore = CalculateScore(Region.Tiles.Count) * playerVar.Value;

				//Debug.Log("Score tuiles Flag : " + playerScore);
				GameManager.Instance.AddScore(playerScore, playerVar.Key);
			}
		}

		private int CalculateScore(int TilesNumber)
		{
			return TilesNumber * (TilesNumber + 1) / 2;
		}
	}
}