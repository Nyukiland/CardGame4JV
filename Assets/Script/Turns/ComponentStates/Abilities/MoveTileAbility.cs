using CardGame.StateMachine;
using CardGame.Utility;
using CardGame.UI;
using UnityEngine;
using CardGame.Card;
using DG.Tweening;

namespace CardGame.Turns
{
	public class MoveTileAbility : Ability
	{
		[SerializeField]
		private LayerMask _layerTile;

		private Plane _planeForCast = new(Vector3.forward, new Vector3(0, 0, 0));

		private GridManagerResource _gridManager;
		private ZoneHolderResource _holder;
		private PlaceTileOnGridAbility _placeTileOnGrid;

		private Vector2Int _prevPos = new(-100, -100);
		private bool _prevActivity = false;

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
			_holder = owner.GetStateComponent<ZoneHolderResource>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
			_placeTileOnGrid = owner.GetStateComponent<PlaceTileOnGridAbility>();
		}

		public bool QuickCheckRay(Vector2 position)
		{
			return Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100, _layerTile);
		}

		public void PickCard(Vector2 position)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(position), out RaycastHit hit, 100, _layerTile))
			{
				if (hit.collider.GetComponentInParent<TileVisu>() is TileVisu visu)
				{
					bool needRemove = true;

					if (_placeTileOnGrid.TempPlacedTile != null && _placeTileOnGrid.TempPlacedTile != visu)
					{
						_holder.GiveTileToHand(_placeTileOnGrid.TempPlacedTile.gameObject);
						_placeTileOnGrid.TempPlacedTile = null;
					}
					else if (_placeTileOnGrid.TempPlacedTile == visu)
					{
						needRemove = false;
					}

					if (needRemove)
						_holder.RemoveTileFromHand(visu.gameObject);

					CurrentTile = visu;

					OnCardPicked?.Invoke();

					//Debug.Log($"{visu.TileData.TileSettings.name} : North {visu.TileData.Zones[0].environment}, Est {visu.TileData.Zones[1].environment} south {visu.TileData.Zones[2].environment} west {visu.TileData.Zones[3].environment}");

					_holder.HideMyHand(true);
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

			CurrentTile.transform.DOMove(pos, 0.1f);

			// Verification de sa validity
			if (!CanPlaceOnGrid || _holder.IsInHand(position)) //on fait rien quand c'est dans la main ou si on ne peut pas la placer
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
				if (_prevPos != new Vector2Int(-100, -100))
				{
					_gridManager.GetTile(_prevPos).gameObject.SetActive(_prevActivity);
				}

				_prevActivity = _gridManager.GetTile(gridPos).gameObject.activeSelf;
				_gridManager.GetTile(gridPos).gameObject.SetActive(false);
				_prevPos = gridPos;

				int connections = _gridManager.GetPlacementConnectionCount(CurrentTile.TileData, gridPos);
				int linkedNeighbor = _gridManager.CheckNeighborTileLinked(gridPos);
				//Debug.Log($"placement : {connections}, {linkedNeighbor}, {connections > 0 && linkedNeighbor > 0}");

				CurrentTile.ChangeValidityVisual(connections > 0 && linkedNeighbor > 0); // jaune si > 0, sinon noir
			}
		}

		public void StandardRelease()
		{
			if (_prevPos != new Vector2Int(-100, -100))
			{
				TileVisu visu = _gridManager.GetTile(_prevPos);

				visu.gameObject.SetActive(_prevActivity);

				_prevPos = new Vector2Int(-100, -100);
				_prevActivity = false;
			}

			if (_placeTileOnGrid.TempPlacedTile == CurrentTile)
			{
				_placeTileOnGrid.TempPlacedTile = null;
			}

			_holder.HideMyHand(false);
		}
	}
}