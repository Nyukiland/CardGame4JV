using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.UI;
using UnityEngine;
using CardGame.Card;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		[SerializeField]
		private LayerMask _layerTile;

		private Plane _planeForCast = new(Vector3.forward, new Vector3(0, 0, -0.15f));

		private GridManagerResource _gridManager;
		private SendInfoAbility _sender;
		private ZoneHolderResource _handResource;
        public TileVisu CurrentTile
		{
			get;
			set;
		}

        public event System.Action OnCardPicked; //Pour la preview d'ou on peut poser la tile de maniere valide

        public bool CanPlaceOnGrid { get; set; } = false;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_handResource = owner.GetStateComponent<ZoneHolderResource>();
			_sender = owner.GetStateComponent<SendInfoAbility>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
		}

		public void PickCard(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100, _layerTile))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					_handResource.RemoveTileFromHand(visu.gameObject);
					CurrentTile = visu;

                    OnCardPicked?.Invoke();
                }
			}
		}

		public void MoveCard(Vector2 position)
		{
			if (CurrentTile == null)
				return;

			// Deplacement de la tuile
			Ray ray = Camera.main.ScreenPointToRay(position);
			_planeForCast.Raycast(ray, out float dist);
			Vector3 pos = ray.GetPoint(dist);
			pos = Vector3Int.FloorToInt(pos);
			CurrentTile.transform.position = pos;

			// Verification de sa validity
			if (!CanPlaceOnGrid || _handResource.IsInHand(position)) //on fait rien quand c'est dans la main ou si on ne peut pas la placer
			{
				CurrentTile.ResetValidityVisual();
				return;
			}

			Vector2Int gridPos = Vector2Int.FloorToInt(pos);
			TileVisu target = _gridManager.GetTile(gridPos);

            if (target == null || target.TileData != null)
            {
                CurrentTile.ChangeValidityVisual(false); // noir
            }
            else
            {
                int connections = _gridManager.GetPlacementConnectionCount(CurrentTile.TileData, gridPos);
                CurrentTile.ChangeValidityVisual(connections > 0); // jaune si > 0, sinon noir
            }
        }


    }
}