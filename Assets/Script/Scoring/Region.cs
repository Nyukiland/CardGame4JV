using CardGame.Card;
using CardGame.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CardGame
{
	public class Region
	{
		public int OpeningCount = 0;
		
		public HashSet<TileVisu> Tiles = new();

		public bool AlreadyScored = false;
        public Region(int openingCount)
        {
            OpeningCount = openingCount;
			//Debug.Log("Vous avez cr�� une r�gion");
        }

        public void Merge(Region otherRegion)
        {
			

			// Etape 1 : Pour chaque tuile de la region qu'on fusionne, il faut updater l'info de region contenu dans ZoneData :
			foreach (TileVisu Tile in otherRegion.Tiles)
			{
				for (int i = 0; i < Tile.TileData.Zones.Length; i++)
				{
					// Si la region actuelle de la zone est la region qu'on veut changer
					if (Tile.TileData.Zones[i].Region == otherRegion)
					{
						Tile.TileData.Zones[i].Region = this;
					}
				}
			}

			// Etape 2 : Fusionner les donn�es des R�gions
			Tiles.UnionWith(otherRegion.Tiles);
			OpeningCount += otherRegion.OpeningCount;
			//Debug.Log("FUUUUUUSION !!! " + Tiles.Count + " > " + OpeningCount);
			// - QUAND FERA LE SCORING FAUDRA PAS OUBLIER DE COPIER LES AUTRES DATAS DE LA REGION LORS DU MERGE
			if (OpeningCount == 0)
			{
				//Debug.Log("La r�gion est ferm�e");
				//Debug.Log("Il y a " + Tiles.Count + " dans la zone");
			}
		}
	}
}
