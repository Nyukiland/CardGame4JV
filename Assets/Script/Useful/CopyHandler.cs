using System.Runtime.InteropServices;
using UnityEngine;

public static class CopyHandler
{
	public static void CopyToClipboard(string text)
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_ANDROID
		GUIUtility.systemCopyBuffer = text;
#elif UNITY_IOS
        _CopyToClipboard(text);
#else
        Debug.LogWarning("Clipboard copy not supported on this platform.");
#endif
	}

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _CopyToClipboard(string text);
#endif
}