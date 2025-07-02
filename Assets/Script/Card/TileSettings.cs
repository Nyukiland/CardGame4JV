using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace CardGame.Card
{
    [CreateAssetMenu(fileName = "TileSettings", menuName = "TileSettings")]

    public class TileSettings: ScriptableObject
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


        [Header("Effects")] [SerializeField, SerializeReference, SubclassSelector(typeof(CardEffect))]
        private List<CardEffect> _cardEffect = new();

        [SerializeField, SerializeReference, SubclassSelector(typeof(CardFeedback))]
        private List<CardFeedback> _cardFeedback = new();

		private int GetStableId()
		{
			//get a text
			string input = $"{_northZone.environment}-{_northZone.isOpen}-" +
						$"{_eastZone.environment}-{_eastZone.isOpen}-" +
						$"{_southZone.environment}-{_southZone.isOpen}-" +
						$"{_westZone.environment}-{_westZone.isOpen}";

			//using to auto dispose the var
			using SHA256 sha = SHA256.Create();
			//fun encryption again
			byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Take the first 4 bytes 
			return BitConverter.ToInt32(hash, 0);
		}

		// Faut passer par lç car c'est privé
		public void SetZones(ZoneData n, ZoneData e, ZoneData s, ZoneData w)
		{
			_northZone = n;
			_eastZone = e;
			_southZone = s;
			_westZone = w;
		}
	}

    public enum ENVIRONEMENT_TYPE
    {
        None,
        Neutral, // mort, vaut rien
		Terrain, // marron
        Grass,  // Vert
        Fields, // Jaune
        Water, // I mean...
    }

    [System.Serializable]
    public struct ZoneData // Je laisse publique car il y a rien qui modifie ca, en dehors du tools
    {
        public ENVIRONEMENT_TYPE environment;
        public bool isOpen;
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