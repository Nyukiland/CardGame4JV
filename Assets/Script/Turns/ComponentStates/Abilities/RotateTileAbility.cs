using CardGame.Card;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
    public class RotateTileAbility : Ability
    {
        public void RotateCard(Vector2 position)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100f))
            {
                if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
                {
                    Debug.Log($"Before Rotate - North: {visu.TileData.Zones[0].environment}");

                    visu.TileData.RotateTile();
                    visu.UpdateTile(visu.TileData);

                    Debug.Log($"After Rotate - North: {visu.TileData.Zones[0].environment}");
                    Debug.Log("Rotated Tile");
                }
            }
        }
    }
}
