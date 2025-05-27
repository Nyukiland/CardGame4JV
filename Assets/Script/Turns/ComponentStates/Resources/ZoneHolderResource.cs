using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
    public class ZoneHolderResource : Resource
    {
        [SerializeField] private Transform _mainCanvas;
        public Transform MainCanvas => _mainCanvas;
        [SerializeField] private List<CardContainer> _zonesCardContainers;
        private readonly Dictionary<RectTransform, CardContainer> _cardZones = new();

        public override void LateInit()
        {
            foreach (CardContainer container in _zonesCardContainers)
            {
                _cardZones.TryAdd(container.RectTransform, container);
            }
        }

        public bool ContainsContainer(Vector2 position, out CardContainer container)
        {
            foreach (RectTransform cardZone in _cardZones.Keys)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(cardZone, position))
                    continue;
                
                container = _cardZones[cardZone];
                return true;
            }

            container = null;
            return false;
        }
    }
}