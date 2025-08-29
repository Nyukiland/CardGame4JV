using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class RotateTileAbility : Ability
	{
		[SerializeField]
		private LayerMask _layerTile;

		private PlaceTileOnGridAbility _tileOnGrid;
		private GridManagerResource _gridManager;
		private SoundResource _sound;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			
			_tileOnGrid = owner.GetStateComponent<PlaceTileOnGridAbility>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
			_sound = owner.GetStateComponent<SoundResource>();
		}

		public void RotateCard(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100f, _layerTile))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					visu.TileData.RotateTile();
					visu.UpdateTile(visu.TileData);
					_sound.PlayTileRotate();

					if (_tileOnGrid.TempPlacedTile == visu)
					{
						int connections = _gridManager.GetPlacementConnectionCount(visu.TileData, _tileOnGrid.TempPos);
						visu.SetWrongRotationFeedbackActive(connections == 0);
					}
				}
			}
		}
	}
}
