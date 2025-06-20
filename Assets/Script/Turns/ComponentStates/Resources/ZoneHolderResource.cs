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

		public int TileInHandCount => _tileInHand.Count;

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
            for (int i = 0; i < _tileInHand.Count; i++)
            {
                if (droppedPosition.x < _tileInHand[i].transform.position.x)
                    return i;
            }

            return _tileInHand.Count; // add a la fin sinon
        }


        public void GiveTileToHand(GameObject tile)
		{
			if (_tileInHand.Contains(tile))
			{
				UnityEngine.Debug.LogError($"[{nameof(ZoneHolderResource)}] Tile already in hand");
				return;
			}

            int insertIndex = GetInsertionIndex(tile.transform.position);
            _tileInHand.Insert(insertIndex, tile);

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
                //float t = (float)i / (float)count;
				// Faut faire count -1 : si 4 cartes, on avait 0, .25 .50 .75 et jamais 1, donc la on a 0 .33 .66 1
				// On evite juste la division par 0 s'il n'y a qu'une carte
                float t = count > 1 ? (float)i / (count - 1) : 0.5f; 
                Vector3 pos = Vector3.Lerp(pos1, pos2, t);
				_tileInHand[i].transform.position = pos + (Camera.main.transform.forward * 2);
			}
		}
	}
}