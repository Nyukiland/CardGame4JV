namespace CardGame.Card
{
    public class TileData
    {
        public TileSettings TileSettings { get; set; }

        /// <summary> Sens des aiguilles d'une montre </summary>
        public int TileRotationCount { get; private set; } = 0;

        /// <summary> Sens des aiguilles d'une montre : 0 = nord, 1 = est, 2 = sud, 3 = ouest </summary>
        public ZoneData[] Zones { get; private set; }


        public void InitTile(TileSettings tileSettingsRef)
        {
            TileSettings = tileSettingsRef;

            Zones = new ZoneData[4];
            Zones[0] = TileSettings.NorthZone;
            Zones[1] = TileSettings.EastZone;
            Zones[2] = TileSettings.SouthZone;
            Zones[3] = TileSettings.WestZone;

        }

        public void RotateTile()
        {
            TileRotationCount += 1;

            ZoneData copiedZone = Zones[0];
            for (int i = 0; i < 3; i++)
            { 
                Zones[i] = Zones[i+1];
            }
            Zones[3] = copiedZone;

        }
    }
}