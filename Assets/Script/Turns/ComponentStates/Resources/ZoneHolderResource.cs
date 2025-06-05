using System.Collections.Generic;
using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
	public class ZoneHolderResource : Resource
	{
		[SerializeField]
		private RectTransform _handZone;

		[SerializeField]
		private List<GameObject> _tileInHand;

		public override void LateInit()
		{
			UpdatePlacementInHand();
		}

		public bool IsInHand(Vector2 position)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(_handZone, position);
		}

		public void GiveTileToHand(GameObject tile)
		{
			if (_tileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile already in hand");
				return;
			}

			_tileInHand.Add(tile);

			UpdatePlacementInHand();
		}

		public void RemoveTileFromHand(GameObject tile)
		{
			if (!_tileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile not in hand");
				return;
			}

			_tileInHand.Remove(tile);

			UpdatePlacementInHand();
		}

		public void UpdatePlacementInHand()
		{
			Vector3[] worldCorners = new Vector3[4];
			_handZone.GetWorldCorners(worldCorners);

			Vector3 pos1 = Camera.main.ScreenToWorldPoint(worldCorners[1]);
			Vector3 pos2 = Camera.main.ScreenToWorldPoint(worldCorners[2]);

			int count = _tileInHand.Count;

			for (int i = 0; i < count; i++)
			{
				float t = (float)i / (float)count;
				Vector3 pos = Vector3.Lerp(pos1, pos2, t);
				_tileInHand[i].transform.position = pos + (Camera.main.transform.forward * 2);
			}
		}
	}
}