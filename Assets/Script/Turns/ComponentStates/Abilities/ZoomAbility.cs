using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoomAbility : Ability
	{

		[SerializeField] private float _minZoom = 2f;
		private float _maxZoom = 10f;

		private float _startdist = 0;
		private float _startZoom = 0;

		private bool _inZoom;

		private Camera _cam;
		private ZoneHolderResource _zoneHolder;
		private GridManagerResource _gridManager;


		public bool InZoom => _inZoom;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_cam = Camera.main;

			_zoneHolder = owner.GetStateComponent<ZoneHolderResource>();
			_gridManager = owner.GetStateComponent<GridManagerResource>();
		}

		public void StartZoom(Vector2 posTouch1, Vector2 posTouch2)
		{
			_inZoom = true;
			_startdist = Vector2.Distance(posTouch1, posTouch2);
			_startZoom = _cam.orthographicSize;

			Vector3 bottomLeft = new(100, 100, 0);
			Vector3 topRight = new(0, 0, 0);

			for (int i = 0; i < _gridManager.Width; i++)
			{
				for (int j = 0; j < _gridManager.Height; j++)
				{
					TileVisu tile = _gridManager.GetTile(i, j);

					if (tile.TileData == null) continue;

					Vector3 tilePos = tile.transform.position;

					bottomLeft.x = Mathf.Min(bottomLeft.x, tilePos.x);
					bottomLeft.y = Mathf.Min(bottomLeft.y, tilePos.y);
					topRight.x = Mathf.Max(topRight.x, tilePos.x);
					topRight.y = Mathf.Max(topRight.y, tilePos.y);
				}
			}

			float width = topRight.x - bottomLeft.x;
			float height = topRight.y - bottomLeft.y;
			_maxZoom = Mathf.Max(width, height) * 0.5f + 1f; // +1f c'est la marge
			_zoneHolder.HideMyHand(true);
		}

		public void ZoomInProcess(Vector2 posTouch1, Vector2 posTouch2)
		{
			if (!_inZoom) return;

			float currentDist = Vector2.Distance(posTouch1, posTouch2);
			float zoomFactor = _startdist / currentDist;

			_cam.orthographicSize = Mathf.Clamp(_startZoom * zoomFactor, _minZoom, _maxZoom); // Half size, d'ou le *.5 sur maxzoom

			_zoneHolder.UpdatePlacementInHand(true);
		}

		public void StopZoom()
		{
			_inZoom = false;
			_startdist = 0;
			_startZoom = 0;

			_zoneHolder.UpdateTileInHandSize();
			_zoneHolder.UpdatePlacementInHand(true);
			_zoneHolder.HideMyHand(false);
		}
	}
}