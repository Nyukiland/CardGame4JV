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
			Debug.Log("Vous avez cr�� une r�gion");
        }

        public void Merge(Region OtherRegion)
        {
			// prend toutes les connections d'une region et les int�gre � une r�gion
			// ajoute son compteur de d'ouverture � celui de la pr�sente r�gion

			//Nouvelle version :
			// - on prend la liste des tuiles contenues dans la deuxi�me r�gion

				// - Pour chacune, on explore les R�gions en lien avec chaque zone
	
				// - Si la R�gion == la deuxi�me r�gion, on la remplace par la 1�re r�gion
				// - On met la tuile dans la liste de la 1�re r�gion

			// - Quand la boucle sur toutes les tuiles est termin�e, on met � jour le compteur d'ouverture de la 1�re r�gion


			Tiles.UnionWith(OtherRegion.Tiles);
			OpeningCount += OtherRegion.OpeningCount;
			Debug.Log("FUUUUUUSION !!! " + Tiles.Count + " > " + OpeningCount);
			// - QUAND FERA LE SCORING FAUDRA PAS OUBLIER DE COPIER LES AUTRES DATAS DE LA REGION LORS DU MERGE
			if (OpeningCount == 0)
			{
				Debug.Log("La r�gion est ferm�e");
				Debug.Log("Il y a " + Tiles.Count + " dans la zone");
			}
		}
	}
}
