using CardGame.Card;
using CardGame.UI;
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
		
		public HashSet<TileVisu> Tiles = new();

		public bool AlreadyScored = false;
        public Region(int openingCount)
        {
            OpeningCount = openingCount;
			//Debug.Log("Vous avez créé une région");
        }

        public void Merge(ref ZoneData zoneData)
        {
			Region otherRegion = zoneData.Region;

			zoneData.Region = this;

			// Etape 2 : Fusionner les données des Régions
			Tiles.UnionWith(otherRegion.Tiles);
			OpeningCount += otherRegion.OpeningCount;
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
