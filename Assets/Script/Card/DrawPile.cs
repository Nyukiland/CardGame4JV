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

		public TileSettings GetTileFromDrawPile()
		{
			if (_tileInDrawPile.Count == 0) return null;

			int index = Random.Range(0, _tileInDrawPile.Count);
			TileSettings settings = _tileInDrawPile[index];
			_tileInDrawPile.RemoveAt(index);

			return settings;
		}

		public int GetTileIDFromDrawPile()
		{
			if (_tileInDrawPile.Count == 0) return -1;
            int index = Random.Range(0, _tileInDrawPile.Count);
            TileSettings settings = _tileInDrawPile[index];
			_tileInDrawPile.RemoveAt(index);

			return settings.IdCode;
		}	

		public TileSettings GetTileFromID(int settingsID)
		{
			foreach (TileSettings setting in AllTileSettings)
			{
				if (setting.IdCode == settingsID)
				{
					return setting;
				}
			}

			return null;
		}

		public void DiscardTile(int settingsID) 
		{
			_tileInDrawPile.Add(GetTileFromID(settingsID));
		}
    }
}