using System.Collections.Generic;
using CardGame.StateMachine;
using DG.Tweening;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoneHolderResource : Resource
	{
		[SerializeField]
		private RectTransform _handZone;

		[SerializeField]
		private float _offsetTop;

		[SerializeField]
		private float _offsetSides;

		[Disable]
		public List<GameObject> TileInHand;

		public int TileInHandCount => TileInHand.Count;
		
		private HUDResource _hudResource;

		private float _tileSize;
		private float _defaultCamSize;

		public override void Init(Controller owner)
		{
			_hudResource = owner.GetStateComponent<HUDResource>();
			_defaultCamSize = Camera.main.orthographicSize;
			_tileSize = 1;
		}

		public override void LateInit()
		{
			UpdatePlacementInHand();
		}

		public bool IsInHand(Vector2 position)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(_handZone, position);
		}

		private int GetInsertionIndex(Vector3 droppedPosition)
		{
			for (int i = 0; i < TileInHand.Count; i++)
			{
				if (droppedPosition.x < TileInHand[i].transform.position.x)
					return i;
			}

			return TileInHand.Count; // add a la fin sinon
		}


		public void GiveTileToHand(GameObject tile)
		{
			if (TileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile already in hand");
				return;
			}

			int insertIndex = GetInsertionIndex(tile.transform.position);
			tile.transform.parent = Camera.main.transform;
			TileInHand.Insert(insertIndex, tile);

			UpdatePlacementInHand();
		}

		public void RemoveTileFromHand(GameObject tile)
		{
			if (!TileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile not in hand");
				return;
			}

			TileInHand.Remove(tile);
			tile.transform.parent = null;
			tile.transform.DORotate(new(0, 0, 0), 0.2f, RotateMode.Fast);
			tile.transform.DOScale(Vector3.one, 0.2f);

			UpdatePlacementInHand();
		}

		public void UpdatePlacementInHand(bool force = false)
		{
			Vector3[] worldCorners = new Vector3[4];
			_handZone.GetWorldCorners(worldCorners);

			Vector3 pos1 = Camera.main.ScreenToWorldPoint(worldCorners[1] + new Vector3(_offsetSides, _offsetTop, 0));
			Vector3 pos2 = Camera.main.ScreenToWorldPoint(worldCorners[2] + new Vector3(-_offsetSides, _offsetTop, 0));

			int count = TileInHand.Count;

			for (int i = 0; i < count; i++)
			{
				//float t = (float)i / (float)count;
				// Faut faire count -1 : si 4 cartes, on avait 0, .25 .50 .75 et jamais 1, donc la on a 0 .33 .66 1
				// On evite juste la division par 0 s'il n'y a qu'une carte
				float t = count > 1 ? (float)i / (count - 1) : 0.5f;
				Vector3 pos = Vector3.Lerp(pos1, pos2, t);

				if (!force)
				{
					TileInHand[i].transform.DOMove(pos + (Camera.main.transform.forward * 2), 0.2f);
					TileInHand[i].transform.DORotate(Camera.main.transform.eulerAngles, 0.2f, RotateMode.Fast);
					TileInHand[i].transform.DOScale(Vector3.one * _tileSize, 0.2f);
				}
				else
				{
					TileInHand[i].transform.position = pos + (Camera.main.transform.forward * 2);
					TileInHand[i].transform.eulerAngles = Camera.main.transform.eulerAngles;
					TileInHand[i].transform.localScale = Vector3.one * _tileSize;
				}
			}
		}

		public void HideMyHand(bool isHidden)
		{
			for (int i = 0; i < TileInHand.Count; i++)
			{
				TileInHand[i].SetActive(!isHidden);
			}

			if (isHidden)
				_hudResource.CloseHud();
			else
				_hudResource.OpenHud();
		}

		public void UpdateTileInHandSize()
		{
			_tileSize = Camera.main.orthographicSize / _defaultCamSize;
		}

		public void EndDestroyTile()
		{
			for (int i = TileInHand.Count-1; i >= 0; i--)
			{
				GameObject.Destroy(TileInHand[i]);
			}
		}
	}
}