using CardGame.Turns;
using CardGame.Utility;
using System;
using System.Collections.Generic;
using Unity.Services.Relay.Models;

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

		/// <summary> Définit la région de scoring à laquelle appartient la zone de même index (0 = nord, 1 = est, 2 = sud, 3 = ouest) </summary>
		//public Region[] Regions { get; private set; }

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

		/*
		 
		 */
		//class Connection
		//{

		//}

		//enum Direction
		//{
		//    North,
		//    South,
		//    East,    
		//    West,
		//}

		//Dictionary<Direction, Connection> _connections = new Dictionary<Direction, Connection>();

		private TileData DetermineNeighborTile(int DirectionIndex, int x, int y)
		{
			GridManagerResource grid = Storage.Instance.GetElement<GridManagerResource>();
			switch (DirectionIndex)
			{
				case 0:
					return grid.GetTile(x, y + 1).TileData;
				case 1:
					return grid.GetTile(x + 1, y).TileData;
				case 2:
					return grid.GetTile(x, y - 1).TileData;
				case 3:
					return grid.GetTile(x - 1, y).TileData;
				default:
					return null;
			}
		}

		private int? DetermineCorrespondingZoneIndex(int ZoneIndex)
		{
			switch (ZoneIndex)
			{
				case 0:
					return 2;
				case 1:
					return 3;
				case 2:
					return 0;
				case 3:
					return 1;
				default:
					return null;
			}
		}

		public void DetermineTileRegions(int x, int y)
		{
			//Debug.Log("TILE : " + x + " - " + y);
		
			// Etape 1 : Check pour chaque zone, s'il y a une tuile à côté.
			for (int i = 0; i <= 3; i++)
			{
				//Debug.Log("Direction : " + i);
				TileData NeighborTile = DetermineNeighborTile(i, x, y);

				// si une tuile jouxte la zone :
				if (NeighborTile != null)
				{
					//Debug.Log("Il y a une tuile à côté");
					int? CorrespondingZoneIndex = DetermineCorrespondingZoneIndex(i);
					if (CorrespondingZoneIndex == null) Debug.LogWarning($"[{nameof(TileData)}] > [{nameof(DetermineCorrespondingZoneIndex)}] > renvoie null");

					// Si oui, copie cette région comme étant celle de la zone
					Zones[i].Region = NeighborTile.Zones[i].Region;
					Zones[i].Region.Tiles.Add(this);
					// baisse le compteur de la zone en question de 1
					Zones[i].Region.OpeningCount--;
					if (Zones[i].Region.OpeningCount == 0)
					{
						Debug.Log("La région dans la direction " + i + " est fermée");
						Debug.Log("Il y a " + Zones[i].Region.Tiles.Count + " dans la zone");
					}
				}
				else
				{
					//Debug.Log("Il n'y a pas une tuile à côté");
					// Sinon Crée une nouvelle zone. 
					Zones[i].Region = new Region(1);
				}
				Zones[i].Region.Tiles.Add(this);
			}

			// Etape 2 : check pour chaque zone si elle est connectée à une autre zone de la tuile
			for (int i = 0; i <= 3; i++)
			{
				//Debug.Log("Direction I : " + i);
				if (Zones[i].isOpen == false) continue;
				for (int j = i+1; j <= 3; j++)
				{
					//Debug.Log("Direction I : " + i + " + J : " + j);

					if (Zones[i].environment != Zones[j].environment) continue;
					if (Zones[i].Region == Zones[j].Region) continue;
					// On merge :
					Zones[i].Region.Merge(Zones[j].Region);
				}
			}

		}

	}
}