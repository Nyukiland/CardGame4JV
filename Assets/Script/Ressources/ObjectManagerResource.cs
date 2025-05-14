using UnityEngine;
using UnityEngine.UI;

namespace CardGame.StateMachine
{
    public class ObjectManagerResource : Resource
    {
        [SerializeField] private Image _image;

        public Image ContainsPos(Vector3 pos)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, pos))
                return _image;

            return null;
        }
    }
}
