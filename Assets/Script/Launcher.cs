using CardGame.Useful;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;

namespace CardGame
{
	// Script used to initalize the game, must be moved later
	public class Launcher : Singleton<Launcher>
	{
		[SerializeField] private EventSystem _eventSystem;
		[SerializeField] private TextMeshProUGUI _scoreText;

		private void Start()
		{
			ResourceManager.ExceptionHandler = AddressableErrorHandler.HandleAddressableException;

			// Oui il y a UN message d'erreur de l'event system juste avant qu'il soit désac
			// On peut rien y faire déso
			if (SceneManager.loadedSceneCount > 1)
				_eventSystem.gameObject.SetActive(false);
		}
	}
}