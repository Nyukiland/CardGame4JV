using UnityEngine;

namespace CardGame
{
	[RequireComponent(typeof(Camera))]
    public class SyncCam : MonoBehaviour
    {
		private Camera _cam;

		[SerializeField]
		private Camera _toCopy;

		private void Start()
		{
			_cam = GetComponent<Camera>();
		}

		void Update()
        {
			_cam.orthographicSize = _toCopy.orthographicSize;
        }
    }
}
