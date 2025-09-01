using UnityEngine;

namespace CardGame
{
    public class LookAtCam : MonoBehaviour
    {
		[SerializeField]
		private Vector3 _lookAtOffset = new(0, -10, -10);

        void Update()
        {
			transform.LookAt(transform.position + _lookAtOffset);
        }
    }
}
