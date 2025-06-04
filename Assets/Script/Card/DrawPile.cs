using System.Collections.Generic;
using CardGame.Utility;
using CardGame.Card;
using UnityEngine;

namespace CardGame
{
    public class DrawPile : MonoBehaviour
    {
		public List<TileSettings> AllTileSettings = new();

		private List<TileSettings> _tileInDrawPile = new();

		private void Start()
		{
			foreach (TileSettings item in AllTileSettings)
			{
				_tileInDrawPile.Add(item);
			}
		}

		private void OnEnable()
		{
			Storage.Instance.Register(this);
		}

		private void OnDisable()
		{
			Storage.Instance.Delete(this);
		}

		public int GetTileIDFromDrawPile()
		{
			int index = Random.Range(0, _tileInDrawPile.Count - 1);
			TileSettings settings = _tileInDrawPile[index];
			_tileInDrawPile.RemoveAt(index);

			return settings.IdCode;
		}	
    }
}