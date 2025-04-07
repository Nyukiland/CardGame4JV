using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LockUserUtility
{
	private static List<string> _userListCache = null;

	public static bool CheckCanEdit()
	{
		LockUserWindow[] windows = Resources.FindObjectsOfTypeAll<LockUserWindow>();
		if (windows.Length > 0 && windows[0].AllowAccess) return true;

		if (_userListCache.Count == 0) _userListCache = ReadLockJsonFile();
		List<string> users = _userListCache;
		if (users.Contains(System.Environment.UserName)) return true;

		return false;
	}

	public static List<string> ReadLockJsonFile()
	{
		string[] guids = AssetDatabase.FindAssets($"{nameof(LockUserUtility)} t:script");

		if (guids.Length == 0)
		{
			Debug.LogError($"Script with name '{nameof(LockUserUtility)}' not found!");
			return null;
		}

		string path = AssetDatabase.GUIDToAssetPath(guids[0]);
		path = path.Replace("Editor/LockUserUtility.cs", "LockFile.json");

		if (!File.Exists(path))
		{
			List<string> defaultList = new List<string>();
			string defaultJson = JsonUtility.ToJson(new LockList { list = defaultList }, true);
			File.WriteAllText(path, defaultJson);
			AssetDatabase.Refresh();
		}

		string json = File.ReadAllText(path);
		LockList data = JsonUtility.FromJson<LockList>(json);

		return data?.list ?? new List<string>();
	}

	public static void SaveLockJsonFile(List<string> data)
	{
		string[] guids = AssetDatabase.FindAssets($"{nameof(LockUserUtility)} t:script");
		if (guids.Length == 0)
		{
			Debug.LogError($"Script with name '{nameof(LockUserUtility)}' not found!");
			return;
		}

		string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
		string jsonPath = scriptPath.Replace("Editor/LockUserUtility.cs", "LockFile.json");

		string json = JsonUtility.ToJson(new LockList { list = data }, true);
		File.WriteAllText(jsonPath, json);

		AssetDatabase.Refresh();

		_userListCache = data;
	}

	[System.Serializable]
	private class LockList
	{
		public List<string> list;
	}
}