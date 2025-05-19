using UnityEngine;
using UnityEngine.UI;

namespace CardGame.StateMachine
{
	public class ObjectManagerResource : Resource
	{
		[SerializeField] 
		private Image _image;

        public Image ContainsPos(Vector3 pos)
        {
            //check si la position est sur l'objet UI en utilisant son Rect
            //un Rect étant la position et la dimension d'un objet dans l'UI
            if (RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, pos))
                return _image;

            return null; //si la position ne correspond pas à l'image ne rien renvoyer
        }
    }
}