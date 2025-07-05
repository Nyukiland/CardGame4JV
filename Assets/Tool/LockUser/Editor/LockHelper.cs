using UnityEditor;
using UnityEngine;
using System;

[InitializeOnLoad]
public static class LockHelper
{
	private const string LastShownKey = "LockHelper_LastShownDate";

	static LockHelper()
	{
		EditorApplication.delayCall += ShowWelcomeMessage;
		EditorApplication.playModeStateChanged += OnPlayModeChanged;
	}

	private static void ShowWelcomeMessage()
	{
		DateTime now = DateTime.Now;

		if (now.Month != 8) return;

		string today = now.ToString("yyyy-MM-dd");
		string lastShown = EditorPrefs.GetString(LastShownKey, "");

		if (lastShown != today)
		{
			EditorUtility.DisplayDialog(
				"On a dit VACANCE!!!!!!",
				"Je t'encourage fortement à ne pas travailler ajd \n Le playmode est innaccessible en semaine \n En cas de besoin reviens dimanche et la tu pourras bosser \n\n REPOSE TOI!!!!",
				"OK"
			);

			EditorPrefs.SetString(LastShownKey, today);
		}
	}

	private static void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
		{
			DateTime now = DateTime.Now;

			if (now.DayOfWeek == DayOfWeek.Sunday && now.Month == 8)
			{
				EditorApplication.delayCall += () =>
				{
					EditorUtility.DisplayDialog(
						"ON BOSSE PAS!!!",
						"Regarde dehors, il fait beau, non? \n Alors tu sors et tu fais autre chose \n Si vraiment tu veux bosser tu attends dimanche \n\n Maintenant ouste vas te reposer",
						"OK"
					);

					EditorApplication.isPlaying = false;
				};
			}
		}
	}
}