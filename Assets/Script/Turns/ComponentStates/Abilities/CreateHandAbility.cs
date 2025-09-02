using CardGame.StateMachine;
using CardGame.Card;
using CardGame.UI;
using UnityEngine;
using System.Collections.Generic;

namespace CardGame.Turns
{
	public class CreateHandAbility : Ability
	{
		[SerializeField]
		List<TileSettings> _tileSettings = new();

		[SerializeField]
		private int _countCard;

		[SerializeField]
		private GameObject _prefab;

		[SerializeField]
		private DrawPile _pile;

		private ZoneHolderResource _holdHand;

		public int CountCard => _countCard;

		public override void Init(Controller owner)
		{
			base.Init(owner);
			_holdHand = owner.GetStateComponent<ZoneHolderResource>();
		}

		public override void OnEnable()
		{
			base.OnEnable();
		}

        public void GenerateTiles(int count, bool first = false)
        {
			if (first)
			{
				foreach (TileSettings tile in _tileSettings)
				{
					CreateTile(tile);
				}

				return;
			}

            if (_pile.AllTileSettings == null || _pile.AllTileSettings.Count == 0)
            {
                Debug.LogError("DrawPile.AllTileSettings is empty or null");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                CreateTile(_pile.GetTileFromDrawPile());
            }
        }


        public void CreateTile(TileSettings settings)
		{
			GameObject temp = GameObject.Instantiate(_prefab);
			temp.transform.position = new(100, 100, 0);
			TileData data = new();
			data.InitTile(settings);
			temp.GetComponent<TileVisu>().UpdateTile(data);
			temp.GetComponent<TileVisu>().SetTileLayerGrid(LayerTile.InHand);

			_holdHand.GiveTileToHand(temp);
		}
	}
}