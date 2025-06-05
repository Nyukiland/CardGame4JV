using CardGame.Card;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.UI
{
	public class TileVisu : MonoBehaviour
	{
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

			ZoneData zone = TileData.TileSettings.NorthZone;
			zones.Add(zone);
			_visuNorth.material = GetMaterialForType(zone.environment);

			zone = TileData.TileSettings.SouthZone;
			zones.Add(zone);
			_visuSouth.material = GetMaterialForType(zone.environment);

			zone = TileData.TileSettings.EastZone;
			zones.Add(zone);
			_visuEast.material = GetMaterialForType(zone.environment);

			zone = TileData.TileSettings.WestZone;
			zones.Add(zone);
			_visuWest.material = GetMaterialForType(zone.environment);

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