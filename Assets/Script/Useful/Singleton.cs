using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
	private static T _instance;
	public static T Instance
	{
		get
		{
			if (CheckInstance()) return _instance;
			else return SetUP();
		}
		private set { _instance = value; }
	}

	private bool _destroyAwake = false;

	[SerializeField, LockUser]
	protected bool _isDestroyOnLoad = true;

	public void ForceInstance() { }

	public static bool CheckInstance() { return _instance != null; }

	private static T SetUP()
	{
		T instance = FindFirstObjectByType<T>();
		instance ??= new GameObject(nameof(T)).AddComponent<T>();
		return instance;
	}

	protected virtual void Awake()
	{
		if (CheckInstance())
		{
			_destroyAwake = true;
			Destroy(this);
			return;
		}

		_instance = this as T;
		if (_isDestroyOnLoad) DontDestroyOnLoad(this);
	}

	private void OnDestroy()
	{
		if (!_destroyAwake) _instance = null;
	}
}