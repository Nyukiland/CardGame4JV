using UnityEngine;
using UnityEngine.UI;

namespace CardGame.StateMachine
{
    public class MoveCardAbility : Ability
    {
        private Image _selectedImage;
        private ObjectManagerResource _objectManager;

        public override void Init(Controller owner)
        {
            base.Init(owner);
            _objectManager = Owner.GetStateComponent<ObjectManagerResource>();
        }

        public void OnTouchDown(Vector2 pos)
        {
            _selectedImage = _objectManager.ContainsPos(pos);
        }

        public void OnTouchHold(Vector2 pos)
        {
            if (_selectedImage != null)
            {
                RectTransform rect = _selectedImage.rectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect.parent as RectTransform, pos, null, out var localPoint);
                rect.anchoredPosition = localPoint;
            }
        }

        public bool OnTouchRelease()
        {
            bool moved = _selectedImage != null;
            _selectedImage = null;
            return moved;
        }
    }
}
