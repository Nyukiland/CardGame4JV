using CardGame.Card;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.UI
{
	public class TileVisu : MonoBehaviour
	{
        [SerializeField]
        private MeshRenderer _visuValidity;
        [SerializeField]
		private MeshRenderer _visuNorth;
		[SerializeField]
		private MeshRenderer _visuSouth;
		[SerializeField]
		private MeshRenderer _visuEast;
		[SerializeField]
		private MeshRenderer _visuWest;
		[SerializeField]
		private MeshRenderer _visuCenter;

		[Space(10)]

		[SerializeField]
		private Material _red;
		[SerializeField]
		private Material _green;
		[SerializeField]
		private Material _blue;
		[SerializeField]
		private Material _white;

		public TileData TileData { get; set; }

        private void Start()
        {
			UpdateTile(TileData);
        }

        public void UpdateTile(TileData data)
		{
			TileData = data;
			UpdateVisu();
		}

		public void ChangeParent(Transform parent)
		{
			gameObject.transform.SetParent(parent);
		}

		private void UpdateVisu()
		{
			List<ZoneData> zones = new();

			if (TileData == null)
			{
                _visuNorth.enabled = false;
                _visuSouth.enabled = false;
                _visuEast.enabled = false;
                _visuWest.enabled = false;
                _visuCenter.enabled = false;

				return;
            }

            _visuNorth.enabled = true;
            _visuSouth.enabled = true;
            _visuEast.enabled = true;
            _visuWest.enabled = true;
            _visuCenter.enabled = true;

            ZoneData[] zonesRotated = TileData.Zones;

            zones.Add(zonesRotated[0]); // North
            _visuNorth.material = GetMaterialForType(zonesRotated[0].environment);

            zones.Add(zonesRotated[1]); // East
            _visuEast.material = GetMaterialForType(zonesRotated[1].environment);

            zones.Add(zonesRotated[2]); // South
            _visuSouth.material = GetMaterialForType(zonesRotated[2].environment);

            zones.Add(zonesRotated[3]); // West
            _visuWest.material = GetMaterialForType(zonesRotated[3].environment);


            //temp
            _visuCenter.enabled = false;

			for (int i = 0; i < zones.Count - 1; i++)
			{
				if (!zones[i].isOpen)
					continue;

				for (int j = i + 1; j < zones.Count; j++)
				{
					if (!zones[j].isOpen)
						continue;

					if (zones[i].environment == zones[j].environment)
					{
						_visuCenter.enabled = true;
						_visuCenter.material = GetMaterialForType(zones[i].environment);
						return;
					}
				}
			}
		}

		public void ChangeValidityVisual(bool isValid)
		{
			_visuValidity.material = isValid ? _green : _red;
        }

		private Material GetMaterialForType(ENVIRONEMENT_TYPE type)
		{
			switch (type)
			{
				case ENVIRONEMENT_TYPE.Forest:
					return _green;
				case ENVIRONEMENT_TYPE.Snow:
					return _white;
				case ENVIRONEMENT_TYPE.Lava:
					return _red;
				case ENVIRONEMENT_TYPE.River:
					return _blue;
			}

			return null;
		}
	}
}