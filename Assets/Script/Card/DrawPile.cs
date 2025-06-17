using System.Collections.Generic;
using CardGame.Utility;
using CardGame.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame
{
    public class DrawPile : MonoBehaviour
    {
	    // Use label "TileSetting" to load tile
		[Disable] public List<TileSettings> AllTileSettings = new();

		private List<TileSettings> _tileInDrawPile = new();
		private static HashSet<TileSettings> _hashSet;

		private void Awake()
		{
			AsyncAwake().Forget();
		}

        private async UniTask AsyncAwake()
        {
            await LoadTiles();


            _tileInDrawPile = new List<TileSettings>();

            for (int i = 0; i < 3; i++)
            {
                _tileInDrawPile.AddRange(AllTileSettings);
            }

            Shuffle(_tileInDrawPile);
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }


        private void OnEnable()
		{
			Storage.Instance.Register(this);
		}

		private void OnDisable()
		{
			if (Storage.CheckInstance()) Storage.Instance.Delete(this);
		}
		
		private async UniTask LoadTiles()
		{
			AllTileSettings.Clear();
			
			AsyncOperationHandle<IList<TileSettings>> handle = await AddressableManager.LoadLabel<TileSettings>("TileSetting");

			_hashSet = new();
			foreach (var tile in handle.Result)
			{
				if (tile == null) continue;

				if (!_hashSet.Add(tile)) return;

				AllTileSettings.Add(tile);
			}
		}

		public int GetTileIDFromDrawPile()
		{
			if (_tileInDrawPile.Count == 0) return -1;
            int index = Random.Range(0, _tileInDrawPile.Count);
            TileSettings settings = _tileInDrawPile[index];
			_tileInDrawPile.RemoveAt(index);

			return settings.IdCode;
		}	
    }
}