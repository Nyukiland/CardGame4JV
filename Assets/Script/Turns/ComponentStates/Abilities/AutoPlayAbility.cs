using CardGame.Card;
using CardGame.StateMachine;
using CardGame.Utility;
using Cysharp.Threading.Tasks;
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

			bool hasValidPosition = false;
			while (hasValidPosition == false)
			{
				int xPosition = Random.Range(0, _grid.Width - 1);
				int yPosition = Random.Range(0, _grid.Height - 1);

				if (_grid.GetTile(xPosition, yPosition).TileData != null) continue;

				TileData tileData = new();
				tileData.InitTile(tileSettings);
				tileData.OwnerPlayerIndex = 1;
				tileData.HasFlag = GameManager.Instance.FlagTurn;
				_grid.SetTile(tileData, xPosition, yPosition);
				hasValidPosition = true;
			}


			GameManager.Instance.SoloTurns++;
			IsFinished = true;
		}
	}
}