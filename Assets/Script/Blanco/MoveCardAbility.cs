using CardGame.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Blanco
{
    public class MoveCardAbility : Ability
    {
        private Image _currentlyHoldImage;
        private ObjectManagerResource _objectManagerResource;
        private bool _hasMoved;

        public override void Init(Controller owner)
        {
            _objectManagerResource = owner.GetStateComponent<ObjectManagerResource>();
        }

        public override void OnEnable()
        {
            _hasMoved = false;
        }
    
        public void Click(Vector2 pos)
        {
            _currentlyHoldImage = _objectManagerResource.ContainsPos(pos);
        }
    
        public void HoldClick(Vector2 pos)
        {
            if (_currentlyHoldImage == null) return;
    
            _hasMoved = true;
            _currentlyHoldImage.transform.position = pos;
        }
    
        public void ReleaseClick()
        {
            _currentlyHoldImage = null;
            
            if (_hasMoved)
                Owner.SetState(typeof(HoldCombinedState));
        }
    }
}