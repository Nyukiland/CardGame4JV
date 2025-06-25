using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class RotateTileAbility : Ability
	{
		[SerializeField]
		private LayerMask _layerTile;

		public void RotateCard(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100f, _layerTile))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					visu.TileData.RotateTile();
					visu.UpdateTile(visu.TileData);
				}
			}
		}
	}
}
