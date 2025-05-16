using CardGame.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Blanco
{
    public class MoveCardAbility : Ability
    {
        private Image _currentlyHoldImage;
        private ObjectManagerResource _objectManagerResource;
        private string _myString;
        private bool _hasMoved;

        public override void Init(Controller owner)
        {
            Debug.Log($"Init. {_objectManagerResource}");
            _myString = "LateInit";
            
            _objectManagerResource = owner.GetStateComponent<ObjectManagerResource>();
        }

        public override void LateInit()
        {
            Debug.Log($"LateInit. {_objectManagerResource}");
            Debug.Log(_myString);
            _myString = "OnEnable";
        }

        public override void OnEnable()
        {
            Debug.Log($"OnEnable. {_objectManagerResource}");
            Debug.Log(_myString);
            _myString = "Click";
            
            _hasMoved = false;
        }

        public void Click(Vector2 pos)
        {
            Debug.Log($"Click. {_objectManagerResource}");
            Debug.Log(_myString);
            
            // _currentlyHoldImage = _objectManagerResource.ContainsPos(pos);
        }

        public void HoldClick(Vector2 pos)
        {
            if (_currentlyHoldImage == null) return;

            // Debug.Log($"Hold");
            // _hasMoved = true;
            // _currentlyHoldImage.transform.position = pos;
        }

        public void ReleaseClick()
        {
            // Debug.Log($"Release");
            // _currentlyHoldImage = null;
            //
            // if (_hasMoved)
            //     Owner.SetState(typeof(HoldCombinedState));
        }
    }
}