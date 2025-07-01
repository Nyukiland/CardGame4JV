using System.Collections.Generic;
using CardGame.Utility;
using CardGame.Card;
using CardGame.Managers;
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
		public List<TileSettings> _bonusTileList {get; private set; } = new();

		private static HashSet<TileSettings> _hashSet;

        public event System.Action OnTilesLoaded; // Pour call la grid generation

        private void Awake()
		{
			AsyncAwake().Forget();
		}

        private async UniTask AsyncAwake()
        {
            await LoadTiles();

            _tileInDrawPile = new List<TileSettings>();

			for (int i = 0; i < AllTileSettings.Count; i++) 
			{
				if (AllTileSettings[i].PoolIndex == 0)
				{
					_tileInDrawPile.Add(AllTileSettings[i]); // On ajoute a la draw pile
				}
				else
				{
					_bonusTileList.Add(AllTileSettings[i]); // On ajoute a la bonus tile pile
				}
			}

            for (int i = 0; i < 2; i++) //On le fait en double pour avoir + de tiles (4 fois +)
            {
                _tileInDrawPile.AddRange(_tileInDrawPile);
            }

            OnTilesLoaded?.Invoke(); // start la grid
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
			
			IList<TileSettings> handle = await AddressableManager.LoadLabel<TileSettings>("TileSetting");

			_hashSet = new();
			foreach (var tile in handle)
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



		// 1 ou 2, c'est call par gridmanager
        public List<TileData> GetBonusTileFromPoolIndex(int poolIndex)
        {
            List<TileData> result = new();

            foreach (var tile in _bonusTileList)
            {
                if (tile.PoolIndex == poolIndex)
                {
					TileData data = new();
					data.InitTile(tile);
                    result.Add(data);
                }
            }

            return result;
        }

    }
}