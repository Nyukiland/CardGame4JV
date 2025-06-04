using System.Collections.Generic;
using UnityEngine;
using System;

namespace CardGame.Card
{
    [CreateAssetMenu(fileName = "TileSettings", menuName = "TileSettings")]

    public class TileSettings: ScriptableObject
    {
		[Disable]
		public int IdCode => GetHashedCode();

        [Header("Zone Data")]
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

		private int GetHashedCode()
		{
			return HashCode.Combine(_northZone, _eastZone, _southZone, _westZone);
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
    public struct ZoneData // Je laisse publique car il y a rien qui modifie ca, en dehors du tools
    {
        public ENVIRONEMENT_TYPE environment;
        public bool isOpen;
    }
}