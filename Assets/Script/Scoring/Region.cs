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
			Debug.Log("Vous avez créé une région");
        }

        public void Merge(Region OtherRegion)
        {
			// prend toutes les connections d'une region et les intègre à une région
			// ajoute son compteur de d'ouverture à celui de la présente région

			//Nouvelle version :
			// - on prend la liste des tuiles contenues dans la deuxième région

				// - Pour chacune, on explore les Régions en lien avec chaque zone
	
				// - Si la Région == la deuxième région, on la remplace par la 1ère région
				// - On met la tuile dans la liste de la 1ère région

			// - Quand la boucle sur toutes les tuiles est terminée, on met à jour le compteur d'ouverture de la 1ère région


			Tiles.UnionWith(OtherRegion.Tiles);
			OpeningCount += OtherRegion.OpeningCount;
			Debug.Log("FUUUUUUSION !!! " + Tiles.Count + " > " + OpeningCount);
			// - QUAND FERA LE SCORING FAUDRA PAS OUBLIER DE COPIER LES AUTRES DATAS DE LA REGION LORS DU MERGE
			if (OpeningCount == 0)
			{
				Debug.Log("La région est fermée");
				Debug.Log("Il y a " + Tiles.Count + " dans la zone");
			}
		}
	}
}
