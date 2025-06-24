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

		[SerializeField]
		private float _waitSec = 2f;

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

		private async UniTask AutoPlay()
		{
			await UniTask.WaitForSeconds(_waitSec);
			DrawPile drawPile = Storage.Instance.GetElement<DrawPile>();

			TileSettings tileSettings = drawPile.GetTileFromDrawPile();

			if (tileSettings == null)
			{
				Owner.SetState<PlaceTileCombinedState>();
				return;
			}

			TileData tileData = new();
			tileData.InitTile(tileSettings);
			tileData.OwnerPlayerIndex = 1;
			tileData.HasFlag = GameManager.Instance.FlagTurn;

			Vector2Int tilePlaced = new(-100, -100);
			foreach (Vector2Int pos in _grid.SurroundingTilePos)
			{
				for (int i = 0; i < 4; i++)
				{
					if (_grid.GetPlacementConnectionCount(tileData, pos) != 0)
					{
						tilePlaced = pos;
						break;
					}
					else
					{
						tileData.RotateTile();
					}
				}

				if (tilePlaced != new Vector2Int(-100, -100)) break;
			}

			if (tilePlaced == new Vector2Int(-100, -100))
			{
				UnityEngine.Debug.LogWarning($"[{nameof(AutoPlayAbility)}] Failed to place tile due to no valid placement");
			}
			else
			{
				_grid.SetTile(tileData, tilePlaced);
			}

			GameManager.Instance.SoloTurns++;
			IsFinished = true;
		}
	}
}