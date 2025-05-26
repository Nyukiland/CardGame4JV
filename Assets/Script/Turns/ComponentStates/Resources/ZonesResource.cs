using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
    public class ZonesResource : Resource
    {
        [SerializeField] private Transform _mainCanvas;
        public Transform MainCanvas => _mainCanvas;
        [SerializeField] private List<RectTransform> _zonesRectTransforms;
        [SerializeField] private List<CardContainer> _zonesCardContainers;
        private Dictionary<RectTransform, CardContainer> _cardZones = new();

        public override void Init(Controller owner)
        {
            if (_zonesRectTransforms.Count != _zonesCardContainers.Count)
            {
                Debug.LogWarning($"The two lists of ZoneResource must have the same amount of elements");
                return;
            }

            for (int i = 0; i < _zonesRectTransforms.Count; i++)
            {
                _cardZones.TryAdd(_zonesRectTransforms[i], _zonesCardContainers[i]);
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