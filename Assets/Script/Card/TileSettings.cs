using System.Collections.Generic;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Card
{
    [CreateAssetMenu(fileName = "TileSettings", menuName = "TileSettings")]

    public class TileSettings: ScriptableObject
    {
        # region Variables

        [SerializeField] private TileUI _cardUIPrefab;
        private TileUI _tileUI;
        public TileUI TileUI => _tileUI;

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

        #endregion

        public TileUI CreateTileUI(Transform parent)
        {
            if (_cardUIPrefab == null)
            {
                Debug.LogWarning($"cardUIPrefab is null, card will not be created");
                return null;
            }
            
            _tileUI = Instantiate(_cardUIPrefab, parent);
            InitData();
            _tileUI.InitTile(this);
            
            return _tileUI;
        }
        
        private void InitData()
        {

        }
    }

    public enum ENVIRONEMENT_TYPE
    {
        None,
        Forest,
        Fields,
        Mountain,
        River,
        City,
    }

    [System.Serializable]
    public struct ZoneData // Je laisse publique car il y a rien qui modifie ca, en dehors du tools
    {
        public ENVIRONEMENT_TYPE environment;
        public bool isOpen;
    }
}