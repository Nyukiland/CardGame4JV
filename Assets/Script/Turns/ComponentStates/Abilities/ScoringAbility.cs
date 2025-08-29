using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
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

		//private HashSet<Region> _closedRegionsInTurn = new();
		List<KeyValuePair<Region, int>> _closedRegionsInTurn = new();

		[Header("ClosedRegionFeedback")]
		[SerializeField] private float _interval = 2f;
		[SerializeField] private float _moveAmount = 1f;
		[SerializeField] private float _moveDuration = 1f;

		//private Dictionary<Region, List<KeyValuePair<int, TileVisu>>> _playersClassicTilesByRegion = new();
		private List<KeyValuePair<int, TileVisu>> _playersClassicTiles = new();
		private List<KeyValuePair<int, TileVisu>> _playersFlagTiles = new();

		private int _currentScoringPlayer = -1;

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

		public async void CallScoring()
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
					_closedRegionsInTurn.Add(new KeyValuePair<Region, int>(zone.Region, zone.Region.Tiles.Count));

					//Debug.Log("Score tile : " + TilePlacedPosition.x + " - " + TilePlacedPosition.y);

					// TODO : A virer quand aura le nouveau système avec feedback
					ScoreClassicTiles(zone.Region);
					ScoreFlagTiles(zone.Region);

					//Debug.Log("-------------------------");
					zone.Region.AlreadyScored = true;
				}
			}
			// classe les régions de la plus petite à la plus grande
			_closedRegionsInTurn.Sort((a, b) => a.Value.CompareTo(b.Value));

			await VisualFeedbackAllRegionsScoredAsync();

			_currentScoringPlayer = _tilePlaced.OwnerPlayerIndex;

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

			// for each player :
			for (int i = 0; i < playersNumber; i++)
			{
				foreach (KeyValuePair<Region, int> closedRegion in _closedRegionsInTurn)
				{
					ColorRegion(closedRegion.Key);

					SortClassicTilesByPlayer(closedRegion.Key);
					await VisualFeedbackClassicScoringAsync();
					_playersClassicTiles = new();

					SortFlagTilesByPlayer(closedRegion.Key);
					if (_playersFlagTiles.Count > 0) await VisualFeedbackFlagScoringAsync(closedRegion.Key);
					_playersFlagTiles = new();
				}

				// recommence la boucle de scoring si le joueur suivant :
				_currentScoringPlayer = (_currentScoringPlayer + 1) % 2;
			}

			await EndSoringFeedbackAsync();
		}

		private void ColorRegion(Region region)
		{
			foreach (TileVisu tileVisu in region.Tiles)
			{
				for (int i = 0; i <= 3; i++)
				{
					if (tileVisu.TileData.Zones[i].Region == region)
					{
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
					}
				}
			}

			Shader.SetGlobalColor("_MainScoringColor", Color.blue);
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
					//Debug.Log("Shake tile : " + tileVisu.PositionOnGrid.x + " - " + tileVisu.PositionOnGrid.y);

					// TODO : remettre au bon endroit
					_sound.PlayScoring(tileVisu.TileData.OwnerPlayerIndex == GameManager.Instance.PlayerIndex);

					// Move up the closed regions tiles : 
					// if the tile is already moving (because it is the last placed tile for instance),
					// reset the movement before begining the new one 
					tileVisu.transform.DOKill();
					tileVisu.transform.DOLocalMoveZ(tileVisu.transform.localPosition.z - _moveAmount, _moveDuration)
						.SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutCubic);
				}
			}

			await UniTask.WaitForSeconds(2f); // for the end of the animation

		}

		private async UniTask VisualFeedbackClassicScoringAsync()
		{
			await UniTask.WaitForSeconds(0.2f); //wait a little for tile placement feedback

			foreach (KeyValuePair<int, TileVisu> playerClassicTiles in _playersClassicTiles)
			{
				if (playerClassicTiles.Key != _currentScoringPlayer) continue;

				TileVisu tileVisu = playerClassicTiles.Value;

				tileVisu.transform.DOShakePosition(0.1f, 0.2f, 5);
				await UniTask.WaitForSeconds(0.2f); // for the end of the animation
			}


		}

		private async UniTask VisualFeedbackFlagScoringAsync(Region Region)
		{
			await UniTask.WaitForSeconds(0.2f); //wait a little for tile placement feedback

			// show the flag of the current player
			foreach (KeyValuePair<int, TileVisu> playerFlagTiles in _playersFlagTiles)
			{
				if (playerFlagTiles.Key != _currentScoringPlayer) continue;

				TileVisu tileVisu = playerFlagTiles.Value;
				tileVisu.transform.DOLocalMoveZ(tileVisu.transform.localPosition.z - _moveAmount, _moveDuration)
						.SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutCubic);
			}
			await UniTask.WaitForSeconds(_moveDuration * 2); // for the end of the animation

			// count for each tile of the 
			foreach (TileVisu tileVisu in Region.Tiles)
			{
				tileVisu.transform.DOShakePosition(0.1f, 0.2f, 5);
				await UniTask.WaitForSeconds(0.3f); // for the end of the animation
			}

			await UniTask.WaitForSeconds(1f); // Delay until next player scoring and/or next player turn
		}

		private async UniTask EndSoringFeedbackAsync()
		{
			await UniTask.WaitForSeconds(0.1f);

			IsScoringFinished = true; //fin
		}

		private void SortClassicTilesByPlayer(Region Region)
		{
			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// les tuiles avec flag ou sans player défini (-1) ne sont pas ajouté à la liste :
				if (tile.HasFlag == true) continue;
				if (tile.OwnerPlayerIndex == -1) continue;

				_playersClassicTiles.Add(new KeyValuePair<int, TileVisu>(tile.OwnerPlayerIndex, tileVisu));
			}
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

		private void SortFlagTilesByPlayer(Region Region)
		{
			foreach (TileVisu tileVisu in Region.Tiles)
			{
				TileData tile = tileVisu.TileData;

				// on ne veut ajouter à la liste que les tuiles avec un flag :
				if (tile.HasFlag == false) continue;

				_playersFlagTiles.Add(new KeyValuePair<int, TileVisu>(tile.OwnerPlayerIndex, tileVisu));
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