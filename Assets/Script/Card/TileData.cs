using CardGame.Turns;
using CardGame.Utility;
using System;
using System.Collections.Generic;
using Unity.Services.Relay.Models;
using UnityEditor.Experimental.GraphView;
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
		public Region[] Regions { get; private set; }

		public int OwnerPlayerIndex { get; set; } = -1; // C'est l'index du joueur dans la liste de OnlinePlayersID 
		public bool HasFlag { get; set; } = false;

		// Bonus tile 
		public int MultiplicativeBonus = 1;
		public int AdditiveBonus = 0;
		//public int PoolIndex = 0; // 1 is small pool and 2 is big pool, c'est dans tile settings

		public void InitTile(TileSettings tileSettingsRef)
		{
			TileSettings = tileSettingsRef;

			Zones = new ZoneData[4];
			Zones[0] = TileSettings.NorthZone;
			Zones[1] = TileSettings.EastZone;
			Zones[2] = TileSettings.SouthZone;
			Zones[3] = TileSettings.WestZone;

			Regions = new Region[4];
			Regions[0] = null;
			Regions[1] = null;
			Regions[2] = null;
			Regions[3] = null;

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

		public void DetermineTileRegions(int x, int y)
		{
			Debug.Log("TILE : " + x + " - " + y);
			//for (int i = 0; i < 4; i++)
			//{
			//    _connections.Add((Direction)i, new Connection());
			//}

			//c

			//// Explore les différentes zones de la tile : 
			//for (int i = 0; i <= 3; i++)
			//{
			//    // Détermine la tuile voisine adjacente à la zone considérée :
			//    TileData NeighborTile;
			//    switch (i)
			//    {
			//        case 0:
			//            NeighborTile = grid.GetTile(x, y + 1).TileData;
			//            break;
			//        case 1:
			//            NeighborTile = grid.GetTile(x + 1, y).TileData;
			//            break;
			//        case 2:
			//            NeighborTile = grid.GetTile(x, y - 1).TileData;
			//            break;
			//        case 3:
			//            NeighborTile = grid.GetTile(x - 1, y).TileData;
			//            break;
			//        default:
			//            NeighborTile = null;
			//            break;
			//    }

			//    // si il n'y a pas de voisin
			//    if (NeighborTile == null)
			//    {
			//        int ZoneOpenings = 1;
			//        // si la zone est connectée à une autre zone de la même tuile
			//        if (Zones[i].isOpen == true)
			//        {
			//            // trouver laquelle/lesquelles et vérifier si elle n'a pas déjà une région affectée
			//            for (int j = 0; j <= 3; j++)
			//            {
			//                if (j == i) continue;
			//                if (Zones[i].environment != Zones[j].environment) continue;

			//                // Si la zone j est de même type que i :
			//                ZoneOpenings++;
			//                // Si la zone a connectée a déjà 
			//                if (Regions[j] != null)
			//                {
			//                    //todo check de merge :
			//                    Regions[i] = Regions[j];
			//                }

			//            }
			//        }
			//        //si region est toujours nulle, en créer une
			//        if (Regions[i] == null) Regions[i] = new Region(ZoneOpenings);

			//        //Refacto :
			//        //Connection connection = _connections[(Direction)i];
			//        //if (connection.Region == null) connection.ConnectTo(new Region(ZoneOpenings));
			//    }
			//    // Compte le nombre d'ouverture de la zone de la tuile et la stocke dans le compteur de RegionDatas
			//    // si il y a un voisin 
			//    // copie la référence à la région dans le tableau RegionDatas
			//    // modifie le compteur d'ouverture de la région :
			//    // NbreOuvertureActuelleDeLaRegion += NbreOuvertureDeLaTuilePourLeMemeEnvironment(si connecté) - 2
			//    // if(NbreOuvertureActuelleDeLaRegion == 0) ScoreRegion




			// Nouvelle version : 
			// Etape 1 : Check pour chaque zone, s'il y a une tuile à côté.
			for (int i = 0; i <= 3; i++)
			{
				Debug.Log("Direction : " + i);
				TileData NeighborTile = DetermineNeighborTile(i, x, y);
				if (NeighborTile != null)
				{
					Debug.Log("Il y a une tuile à côté");
					// Si oui, copie cette région comme étant celle de la zone
					Regions[i] = NeighborTile.Regions[i];
					// baisse le compteur de la zone en question de 1
					Regions[i].OpeningCount--;
					Regions[i].Tiles.Add(this);
					if (Regions[i].OpeningCount == 0)
					{
						Debug.Log("La région dans la direction " + i + " est fermée");
						Debug.Log("Il y a " + Regions[i].Tiles.Count + " dans la zone");
					}
				}
				else
				{
					Debug.Log("Il n'y a pas une tuile à côté");
					// Sinon Crée une nouvelle zone. 
					Regions[i] = new Region(1);
				}
				Regions[i].Tiles.Add(this);
			}

			// Etape 2 : check pour chaque zone si elle est connectée à une autre zone de la tuile
			for (int i = 0; i <= 3; i++)
			{
				Debug.Log("Direction I : " + i);
				if (Zones[i].isOpen == false) continue;
				for (int j = i+1; j <= 3; j++)
				{
					Debug.Log("Direction I : " + i + " + J : " + j);

					if (Zones[i].environment != Zones[j].environment) continue;
					if (Regions[i] == Regions[j]) continue;
					// on merge :
					Regions[i].Merge(Regions[j]);
					Regions[j] = Regions[i];
				}
			}

		}

	}
}