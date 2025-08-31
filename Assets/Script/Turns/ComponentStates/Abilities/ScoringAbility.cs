using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardGame.Turns
{
	public class ScoringAbility : Ability
	{
		[SerializeField]
		[LockUser]
		private MeshOverlapAlphaFeature _alphaFeature;

		[SerializeField]
		private Color _terrainColor;

		[SerializeField]
		private Color _fieldColor;

		[SerializeField]
		private Color _waterColor;

		[SerializeField]
		private Color _forestColor;

		private GridManagerResource _gridManager;
		private SoundResource _sound;
		private HUDResource _hud;

		public Vector2Int TilePlacedPosition
		{
			get;
			private set;
		} = new Vector2Int(-100, -100);

		private TileData _tilePlaced;

		//private HashSet<Region> _closedRegionsInTurn = new();
		List<KeyValuePair<Region, int>> _closedRegionsInTurn = new();

		[Header("ClosedRegionFeedback")]
		[SerializeField] private float _moveAmount = 1f;
		[SerializeField] private float _moveDuration = 1f;

		private List<TileVisu> _playerClassicTiles = new();
		private List<TileVisu> _playerFlagTiles = new();

		private int _cumulativeScoring;

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
			_hud = owner.GetStateComponent<HUDResource>();
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
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ACTION", GameManager.Instance.AmIWinning() ? 1 : 0);
		}

		public async void CallScoring()
		{
			if (TilePlacedPosition == new Vector2Int(-100, -100))
			{
				IsScoringFinished = true;
				return;
			}

			_tilePlaced = _gridManager.GetTile(TilePlacedPosition.x, TilePlacedPosition.y).TileData;

			Vector3 posCam = new(0, 10, 0);

			foreach (ZoneData zone in _tilePlaced.Zones)
			{
				if (
					zone.Region.OpeningCount == 0 &&
					zone.environment != ENVIRONEMENT_TYPE.Neutral &&
					zone.Region.AlreadyScored == false)
				{
					_closedRegionsInTurn.Add(new KeyValuePair<Region, int>(zone.Region, zone.Region.Tiles.Count));

					//Debug.Log("Score tile : " + TilePlacedPosition.x + " - " + TilePlacedPosition.y);

					// TODO : A virer quand aura le nouveau système avec feedback
					//ScoreClassicTiles(zone.Region);
					//ScoreFlagTiles(zone.Region);

					//Debug.Log("-------------------------");
					zone.Region.AlreadyScored = true;
				}
			}

			if (_closedRegionsInTurn.Count == 0)
			{
				IsScoringFinished = true;
				return;
			}

			_hud.ChangeTurnFeedback(HUDResource.TurnState.Scoring);

			// classe les régions de la plus petite à la plus grande
			_closedRegionsInTurn.Sort((a, b) => a.Value.CompareTo(b.Value));

			await VisualFeedbackAllRegionsScoredAsync();

			int currentPlayerTurn = _tilePlaced.OwnerPlayerIndex;

			int playersNumber = 0;
			GameManager manager = GameManager.Instance;
			if (manager.IsNetCurrentlyActive())
			{
				playersNumber = manager.OnlinePlayersID.Count;
			}
			else
			{
				playersNumber = manager.SoloNames.Count;
			}

			await DoScoringForPlayer(currentPlayerTurn);

			// for each player :
			for (int i = 0; i < playersNumber; i++)
			{
				if (i == currentPlayerTurn)
					continue;

				await DoScoringForPlayer(i);
			}

			await EndScoringFeedbackAsync();
		}

		private async UniTask DoScoringForPlayer(int playerIndex)
		{
			_cumulativeScoring = 0;

			//Debug.Log("-----------------> Player : " + i);
			foreach (KeyValuePair<Region, int> closedRegion in _closedRegionsInTurn)
			{
				_alphaFeature.settings.TargetMeshes.Clear();

				ColorRegion(closedRegion.Key);

				// Classic Tiles Scoring Phase :
				SortClassicTilesByPlayer(playerIndex, closedRegion.Key);
				//Debug.Log("_playersClassicTiles: " + _playerClassicTiles.Count);

				if (_playerClassicTiles.Count > 0)
				{
					//Debug.Log("Phase classique tiles");
					await VisualFeedbackClassicScoringAsync(playerIndex);
				}

				// Flag Tiles Scoring Phase :
				SortFlagTilesByPlayer(playerIndex, closedRegion.Key);
				//Debug.Log("_playersFlagTiles: " + _playerFlagTiles.Count);

				if (_playerFlagTiles.Count > 0)
				{
					//Debug.Log("Phase flag tiles");
					await VisualFeedbackFlagScoringAsync(closedRegion.Key, playerIndex);
				}
			}

			_hud.ScoreList[playerIndex].SetCummulativeScore(0);
			GameManager.Instance.AddScore(_cumulativeScoring, playerIndex);
		}

		private void ColorRegion(Region region)
		{
			ENVIRONEMENT_TYPE environment = ENVIRONEMENT_TYPE.None;

			foreach (TileVisu tileVisu in region.Tiles)
			{
				for (int i = 0; i <= 3; i++)
				{
					if (tileVisu.TileData.Zones[i].Region != region)
						continue;

					switch (i)
					{
						case 0:
							_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuNorth);
							break;
						case 1:
							_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuEast);
							break;
						case 2:
							_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuSouth);
							break;
						case 3:
							_alphaFeature.settings.TargetMeshes.Add(tileVisu.VisuWest);
							break;

					}

					if (environment == ENVIRONEMENT_TYPE.None)
						environment = tileVisu.TileData.Zones[i].environment;
				}
			}

			Debug.Log(environment);

			Color color = Color.black;

			switch (environment)
			{
				case ENVIRONEMENT_TYPE.None:
					color = Color.white;
					break;
				case ENVIRONEMENT_TYPE.Neutral:
					color = Color.white;
					break;
				case ENVIRONEMENT_TYPE.Terrain:
					color = _terrainColor;
					break;
				case ENVIRONEMENT_TYPE.Grass:
					color = _forestColor;
					break;
				case ENVIRONEMENT_TYPE.Fields:
					color = _fieldColor;
					break;
				case ENVIRONEMENT_TYPE.Water:
					color = _waterColor;
					break;
			}

			_sound.PlayZoneAmbiance(environment);
			Shader.SetGlobalColor("_MainScoringColor", color);
		}


		private async UniTask VisualFeedbackAllRegionsScoredAsync()
		{
			await UniTask.WaitForSeconds(1f); //wait a little for tile placement feedback

			//foreach (Region closedRegion in _closedRegionsInTurn)
			foreach (KeyValuePair<Region, int> closedRegion in _closedRegionsInTurn)
			{
				// Feedback by colouring the closed region zones : 
				ColorRegion(closedRegion.Key);

				foreach (TileVisu tileVisu in closedRegion.Key.Tiles)
				{
					// Move up the closed regions tiles : 
					// if the tile is already moving (because it is the last placed tile for instance),
					// reset the movement before begining the new one 
					tileVisu.transform.DOKill();
					tileVisu.transform.DOLocalMoveZ(tileVisu.transform.localPosition.z - _moveAmount, _moveDuration)
						.SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutCubic);
				}
			}

			await UniTask.WaitForSeconds(_moveDuration * 2); // for the end of the animation
		}

		private async UniTask VisualFeedbackClassicScoringAsync(int currentPhaseScoringPlayer)
		{
			await UniTask.WaitForSeconds(0.2f);
			bool firstScoreToDisplay = true;
			int currentTilePhaseScoreToDisplay = 0;
			int tileScoreAmount = 1;

			_playerClassicTiles
				.OrderBy(x => Vector3.Distance(new(TilePlacedPosition.x, TilePlacedPosition.y, 0), x.transform.position))
				.ToList();

			foreach (TileVisu classicTile in _playerClassicTiles)
			{
				classicTile.transform.DOShakePosition(0.1f, 0.2f, 5).OnComplete(() =>
				{
					classicTile.transform.position = classicTile.PositionOnGrid;
				});

				if (classicTile.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex)
					FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Self_Score_Info", tileScoreAmount > 3 ? 1 : 0); //1 = big score, 0 = small score
				else
					FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Ennemy_Score_Info", tileScoreAmount > 3 ? 1 : 0); //1 = big score, 0 = small score

				_sound.PlayScoring(classicTile.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex);


				classicTile.VisualScoringTileFeedback(tileScoreAmount);
				await UniTask.WaitForSeconds(0.2f); // for the end of the animation

				if (firstScoreToDisplay)
				{
					_hud.ActivatePhaseScoreDisplay();
					firstScoreToDisplay = false;
					_hud.SetupPhaseScore(currentPhaseScoringPlayer, false);
				}
				currentTilePhaseScoreToDisplay += tileScoreAmount;
				_hud.SetPhaseScore(currentTilePhaseScoreToDisplay);

				_cumulativeScoring += tileScoreAmount;
				_hud.ScoreList[currentPhaseScoringPlayer].SetCummulativeScore(_cumulativeScoring);

				tileScoreAmount++;
			}
			await UniTask.WaitForSeconds(1f);
			_hud.DeactivatePhaseScoreDisplay();
		}

		private async UniTask VisualFeedbackFlagScoringAsync(Region Region, int currentPhaseScoringPlayer)
		{
			await UniTask.WaitForSeconds(0.2f); //wait a little for tile placement feedback
			bool firstScoreToDisplay = true;
			int currentTilePhaseScoreToDisplay = 0;
			int tileScoreAmount = 1;

			// show the flag of the current player
			//foreach (KeyValuePair<int, TileVisu> playerFlagTiles in _playersFlagTiles)
			int playerFlagInRegion = 0;
			foreach (TileVisu playerFlagTile in _playerFlagTiles)
			{
				//playerFlagTile.transform.DOLocalMoveZ(playerFlagTile.transform.localPosition.z - _moveAmount, _moveDuration)
				//		.SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutCubic);
				playerFlagInRegion++;
			}
			//await UniTask.WaitForSeconds(_moveDuration * 2); // for the end of the animation

			List<TileVisu> reorderedTiles = Region.Tiles
				.OrderBy(x => Vector3.Distance(new(TilePlacedPosition.x, TilePlacedPosition.y, 0), x.transform.position))
				.ToList();

			// count for each tile of the 
			foreach (TileVisu tileVisu in reorderedTiles)
			{
				tileVisu.transform.DOShakePosition(0.1f, 0.2f, 5).OnComplete(() =>
				{
					tileVisu.transform.position = tileVisu.PositionOnGrid;
				});

				if (tileVisu.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex)
					FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Self_Score_Info", tileScoreAmount > 6 ? 1 : 0); //1 = big score, 0 = small score
				else
					FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Ennemy_Score_Info", tileScoreAmount > 6 ? 1 : 0); //1 = big score, 0 = small score

				_sound.PlayScoring(tileVisu.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex);

				await UniTask.WaitForSeconds(0.2f); // for the end of the animation

				if (firstScoreToDisplay)
				{
					_hud.ActivatePhaseScoreDisplay();
					firstScoreToDisplay = false;
					_hud.SetupPhaseScore(currentPhaseScoringPlayer, true);
				}
				currentTilePhaseScoreToDisplay += tileScoreAmount;
				_hud.SetPhaseScore(currentTilePhaseScoreToDisplay * playerFlagInRegion);

				_cumulativeScoring += tileScoreAmount;
				_hud.ScoreList[currentPhaseScoringPlayer].SetCummulativeScore(_cumulativeScoring);

				tileScoreAmount++;
			}

			await UniTask.WaitForSeconds(1f); // Delay until next player scoring and/or next player turn
			_hud.DeactivatePhaseScoreDisplay();
		}

		private async UniTask EndScoringFeedbackAsync()
		{
			await UniTask.WaitForSeconds(0.1f);

			IsScoringFinished = true; //fin
		}

		private void SortClassicTilesByPlayer(int playerId, Region Region)
		{
			_playerClassicTiles = new();

			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// les tuiles avec flag ou sans player défini (-1) ne sont pas ajouté à la liste :
				if (tile.HasFlag == true) continue;
				//if (tile.OwnerPlayerIndex == -1) continue;
				if (tile.OwnerPlayerIndex != playerId) continue;

				//_playersClassicTiles.Add(new KeyValuePair<int, TileVisu>(tile.OwnerPlayerIndex, tileVisu));
				_playerClassicTiles.Add(tileVisu);
			}
		}

		private void SortFlagTilesByPlayer(int playerId, Region Region)
		{
			_playerFlagTiles = new();

			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// on ne veut ajouter à la liste que les tuiles avec un flag :
				if (tile.HasFlag == false) continue;
				if (tile.OwnerPlayerIndex != playerId) continue;
				//_playerFlag.Add(tile.OwnerPlayerIndex, true);

				_playerFlagTiles.Add(tileVisu);
			}
		}

		#region Old
		//Vieux système : à virer quand à fini refonte avec feedback
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

		//Vieux système : à virer quand finit système de scoring avec feedback
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
		#endregion
	}
}