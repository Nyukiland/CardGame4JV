using CardGame.StateMachine;
using CardGame.Card;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
	public class CreateHandAbility : Ability
	{
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

        public void GenerateTiles(int count)
        {
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
			TileData data = new();
			data.InitTile(settings);
			temp.GetComponent<TileVisu>().UpdateTile(data);
			temp.GetComponent<TileVisu>().SetTileLayerGrid(false);

			_holdHand.GiveTileToHand(temp);
		}
	}
}