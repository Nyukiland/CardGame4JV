using System.Collections.Generic;
using CardGame.StateMachine;
using CardGame.UI;
using UnityEngine;

namespace CardGame.Turns
{
    public class ZoneHolderResource : Resource
    {
        [SerializeField] private Canvas _mainCanvas;
        public Canvas MainCanvas => _mainCanvas;
        [SerializeField] private List<TileContainer> _zonesCardContainers;
        private readonly Dictionary<RectTransform, TileContainer> _cardZones = new();

        public override void LateInit()
        {
            foreach (TileContainer container in _zonesCardContainers)
            {
                _cardZones.TryAdd(container.RectTransform, container);
            }
        }

        public bool ContainsContainer(Vector2 position, out TileContainer container)
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