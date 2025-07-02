using CardGame.Card;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CardGame
{
	public class Region
	{
		public int OpeningCount = 0;
		
		public HashSet<TileData> Tiles = new();



        public Region(int openingCount)
        {
            OpeningCount = openingCount;
			//Debug.Log("Vous avez créé une région");
        }

        public void Merge(Region OtherRegion)
        {

			// Etape 1 : Pour chaque tuile de la région qu'on fusionne, il faut updater l'info de region contenu dans ZoneData :
			foreach(TileData Tile in OtherRegion.Tiles)
			{
				foreach (ZoneData Zone in Tile.Zones)
				{
					// Si la region actuelle de la zone est la region qu'on veut changer
					if (Zone.Region == OtherRegion)
					{
						Zone.Region = this;
					}
				}
			} 

			// Etape 2 : Fusionner les données des Régions
			Tiles.UnionWith(OtherRegion.Tiles);
			OpeningCount += OtherRegion.OpeningCount;
			//Debug.Log("FUUUUUUSION !!! " + Tiles.Count + " > " + OpeningCount);
			// - QUAND FERA LE SCORING FAUDRA PAS OUBLIER DE COPIER LES AUTRES DATAS DE LA REGION LORS DU MERGE
			if (OpeningCount == 0)
			{
				Debug.Log("La région est fermée");
				Debug.Log("Il y a " + Tiles.Count + " dans la zone");
			}
		}
	}
}
