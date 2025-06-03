using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class TileUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        
        public RectTransform RectTransform => _rectTransform;
        public Image Image => _image;
    }
}