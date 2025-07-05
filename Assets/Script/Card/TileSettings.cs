using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace CardGame.Card
{
	[CreateAssetMenu(fileName = "TileSettings", menuName = "TileSettings")]

	public class TileSettings : ScriptableObject
	{
		[Disable]
		public int IdCode => GetStableId();

		[Header("Zone Data")]
		public int PoolIndex = 0; // Pour les pools de tile bonus sur la map
		public int NumberOfCopies = 1;
		[SerializeField] private ZoneData _northZone;
		[SerializeField] private ZoneData _eastZone;
		[SerializeField] private ZoneData _southZone;
		[SerializeField] private ZoneData _westZone;

		[HideInInspector] public TilePreset tilePreset = TilePreset.FourDifferentClosed;

		// Permet de recuperer 
		public ZoneData NorthZone => _northZone;
		public ZoneData EastZone => _eastZone;
		public ZoneData SouthZone => _southZone;
		public ZoneData WestZone => _westZone;


		[Header("Effects")]
		[SerializeField, SerializeReference, SubclassSelector(typeof(CardEffect))]
		private List<CardEffect> _cardEffect = new();

		[SerializeField, SerializeReference, SubclassSelector(typeof(CardFeedback))]
		private List<CardFeedback> _cardFeedback = new();

		private int GetStableId()
		{
			int id = (int)_northZone.environment + (BoolToInt(_northZone.isOpen) * 10)
				+ ((int)_eastZone.environment * 100) + (BoolToInt(_eastZone.isOpen) * 1000)
				+ ((int)_southZone.environment * 10000) + (BoolToInt(_southZone.isOpen) * 100000)
				+ ((int)_westZone.environment * 1000000) + (BoolToInt(_westZone.isOpen) * 10000000);

			return id;

			int BoolToInt(bool b)
			{
				return b ? 1 : 0;
			}
		}

		// Faut passer par la car c'est privé
		public void SetZones(ZoneData n, ZoneData e, ZoneData s, ZoneData w)
		{
			_northZone = n;
			_eastZone = e;
			_southZone = s;
			_westZone = w;
		}

		public float GetEnvironementAltitude(ENVIRONEMENT_TYPE environement) // Chaque environement voit son mesh set a une hauteur différente
		{
			return environement switch
			{
				ENVIRONEMENT_TYPE.Neutral => 0.0f,
				ENVIRONEMENT_TYPE.Terrain => 0.0f,
				ENVIRONEMENT_TYPE.Grass => 0.2f,
				ENVIRONEMENT_TYPE.Fields => 0.4f,
				ENVIRONEMENT_TYPE.Water => -0.2f,
				_ => 0f,
			};
		}
	}

	public enum ENVIRONEMENT_TYPE
	{
		None = 0,
		Neutral = 1, // mort, vaut rien
		Terrain = 2, // marron
		Grass = 3,  // Vert
		Fields = 4, // Jaune
		Water = 5, // I mean...
	}

	[System.Serializable]
	public struct ZoneData // Je laisse publique car il y a rien qui modifie ca, en dehors du tools
	{
		public ENVIRONEMENT_TYPE environment;
		public bool isOpen;
		public Region Region;
	}

	// Aide a la generation des tiles
	public enum TilePreset
	{
		FourDifferentClosed, // 1 2 3 4 
		ThreeSame,           // 1 2 2 2 
		DiagonalOpenFull,    // 1 1 2 2 
		DiagonalOpenHalf,    // 1 1 2 3
		Path                 // 1 2 2 1, 1 2 2 3
	}
}