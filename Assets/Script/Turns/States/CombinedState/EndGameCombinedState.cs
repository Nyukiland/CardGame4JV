using CardGame.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardGame.Turns
{
	public class EndGameCombinedState : CombinedState
	{
		private HUDResource _hud;

		public override void OnEnter()
		{
			base.OnEnter();

			RestartPlaymodeFromRuntime();

			GetStateComponent(ref _hud);

			if (GameManager.Instance.AmIWinning())
				_hud.OpenWin();
			else
				_hud.OpenLoose();
		}

#if UNITY_EDITOR
		private static bool restartRequested = false;

		public void RestartPlaymodeFromRuntime()
		{
			restartRequested = true;
			EditorApplication.isPlaying = false;
		}

		[InitializeOnLoadMethod]
		static void OnProjectLoadedInEditor()
		{
			EditorApplication.update += () =>
			{
				if (restartRequested && !EditorApplication.isPlaying)
				{
					restartRequested = false;
					EditorApplication.isPlaying = true;
				}
			};
		}
#endif
	}
}