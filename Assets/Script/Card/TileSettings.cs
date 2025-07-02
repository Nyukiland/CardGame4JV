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
        [SerializeField] public int PoolIndex = 0; // Pour les pools de tile bonus sur la map
        [SerializeField] private ZoneData _northZone;
        [SerializeField] private ZoneData _eastZone;
        [SerializeField] private ZoneData _southZone;
        [SerializeField] private ZoneData _westZone;

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
	}

    public enum ENVIRONEMENT_TYPE
    {
        None,
        Forest,
        Snow,
        Lava,
        River,
    }

    [System.Serializable]
    public class ZoneData // Je laisse publique car il y a rien qui modifie ca, en dehors du tools
    {
        public ENVIRONEMENT_TYPE environment;
        public bool isOpen;
        public Region Region = null;

		//public ZoneData(ENVIRONEMENT_TYPE Environment, bool IsOpen)
  //      {
  //          environment = Environment;
  //          isOpen = IsOpen;
		//	Region = null;
		//}
	}
}