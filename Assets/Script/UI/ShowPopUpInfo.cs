using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class ShowPopUpInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;

        public async UniTask ShowPopUp(string text)
        {
			_text.enabled = true;
			_image.enabled = false;
            _text.text = text;
            _canvasGroup.alpha = 1;

            await UniTask.WaitForSeconds(2f);
            
            _canvasGroup.alpha = 0;
        }

		public async UniTask ShowPopUp(Sprite[] anim, float timer)
		{
			_text.enabled = false;
			_image.enabled = true;
            _canvasGroup.alpha = 1;

			float frameDuration = timer / anim.Length;

			foreach (Sprite sprite in anim)
			{
				_image.sprite = sprite;

				await UniTask.WaitForSeconds(frameDuration);
			}

			_canvasGroup.alpha = 0;
		}

		public void HidePopUp()
        {
            _canvasGroup.alpha = 0;
        }
    }
}