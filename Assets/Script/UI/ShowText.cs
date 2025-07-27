using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CardGame.UI
{
    public class ShowText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        public async UniTask ShowPopUp(string text)
        {
            _text.text = text;
            _canvasGroup.alpha = 1;

            await UniTask.WaitForSeconds(2f);
            
            _canvasGroup.alpha = 0;
        }

        public void HidePopUp()
        {
            _canvasGroup.alpha = 0;
        }
    }
}