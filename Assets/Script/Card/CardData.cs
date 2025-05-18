using System.Collections.Generic;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Card
{
    [CreateAssetMenu(fileName = "CardData", menuName = "CardData")]

    public class CardData : ScriptableObject
    {
        private CardUI _cardUI; // Le card UI spawned a partir de ce data

        [Header("Base Stats")]

        [SerializeField] private int _baseManaCost = 5;
        [SerializeField] private int _baseHealthPoints = 5;
        [SerializeField] private int _baseAttackPoints = 5;

        [Header("Effects")]
        [SerializeField, SerializeReference, SubclassSelector(typeof(CardEffect))] private List<CardEffect> _cardEffect = new();
        [SerializeField, SerializeReference, SubclassSelector(typeof(CardFeedback))] private List<CardFeedback> _cardFeedback = new();

        private int _currentManaCost;
        public int CurrentManaCost
        {
            get
            {
                return _currentManaCost;
            }
            set
            {
                _currentManaCost = Mathf.Clamp(value, 0, 99); //Montant arbitraire 
                if (_cardUI != null)
                    _cardUI.UpdateTexts();
            }
        }

        private int _currentHealthPoints;
        public int CurrentHealthPoints
        {
            get
            {
                return _currentHealthPoints;
            }
            set
            {
                _currentHealthPoints = Mathf.Clamp(value, 0, 99); //Montant arbitraire
                if (_currentHealthPoints <= 0)
                {
                    // Mourir plz
                    return;
                }
                if (_cardUI != null)
                    _cardUI.UpdateTexts();
            }
        }

        private int _currentAttackPoints;
        public int CurrentAttackPoints
        {
            get
            {
                return _currentAttackPoints;
            }
            set
            {
                _currentAttackPoints = Mathf.Clamp(value, 0, 99); //Montant arbitraire
                if (_cardUI != null)
                    _cardUI.UpdateTexts();
            }
        }


        // Faut une methode pour spawn le card UI et lui filer sa ref : _cardUI.InitCard(this);

        public void InitData()
        {
            _currentManaCost = _baseManaCost;
            _currentHealthPoints = _baseHealthPoints;
            _currentAttackPoints = _baseAttackPoints;


        }
    }
}