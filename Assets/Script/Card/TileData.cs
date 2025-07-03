using UnityEngine;

namespace CardGame.Card
{
    public class TileData
    {
        public TileSettings TileSettings { get; set; }

        /// <summary> Sens des aiguilles d'une montre </summary>
        public int TileRotationCount { get; private set; } = 0;

        /// <summary> Sens des aiguilles d'une montre : 0 = nord, 1 = est, 2 = sud, 3 = ouest </summary>
        public ZoneData[] Zones { get; private set; }

        public int OwnerPlayerIndex { get; set; } = -1; // C'est l'index du joueur dans la liste de OnlinePlayersID 
        public bool HasFlag { get; set; } = false;

        // Bonus tile 
        public int MultiplicativeBonus = 1;
        public int AdditiveBonus = 0;

		public event System.Action OnTileRotated;

		public void InitTile(TileSettings tileSettingsRef)
        {
            TileSettings = tileSettingsRef;

            Zones = new ZoneData[4];
            Zones[0] = TileSettings.NorthZone;
            Zones[1] = TileSettings.EastZone;
            Zones[2] = TileSettings.SouthZone;
            Zones[3] = TileSettings.WestZone;

            OwnerPlayerIndex = -1;
            HasFlag = false;
        }

        public void RotateTile()
        {
            ZoneData copiedZone = Zones[3]; // rotation aiguille d'une montre

            for (int i = 3; i > 0; i--)
            {
                Zones[i] = Zones[i - 1];
            }
            Zones[0] = copiedZone;

            TileRotationCount += 1;
			OnTileRotated?.Invoke(); // Tile visu va rotate son visuel

		}

		public ZoneData[] GetUnrotatedZones()
		{
			var zones = new ZoneData[4];
			zones[0] = TileSettings.NorthZone;
			zones[1] = TileSettings.EastZone;
			zones[2] = TileSettings.SouthZone;
			zones[3] = TileSettings.WestZone;
			return zones;
		}
	}
}