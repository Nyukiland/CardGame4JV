using CardGame.Card;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class TileCreator : EditorWindow
{
    private Vector2 _scroll;
    private Dictionary<TileSettings, bool> _foldouts = new(); // Pour save les menus pliés et dépliés
    private Dictionary<TileSettings, string> _renameBuffer = new(); // Pour save le nom qu'on est en train de mettre


    [MenuItem("Tools/Tile Creator")]
    public static void ShowWindow()
    {
        TileCreator window = GetWindow<TileCreator>();
        window.titleContent = new GUIContent("Tile Creator");
        window.Show();
    }

    private List<TileSettings> _tileDataList = new();

    private void RefreshTileDataList()
    {
        _tileDataList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:TileData", new[] { "Assets/Script/Data" }); // Dossier qui contient les cartes

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.Contains("/Trash")) // - la corbeille
                continue;

            TileSettings tileData = AssetDatabase.LoadAssetAtPath<TileSettings>(path);

            if (tileData != null)
                _tileDataList.Add(tileData);
        }
    }

    private void CreateNewTile()
    {
        string folderPath = "Assets/Script/Data";
        string baseName = "NewTile_";
        int index = 1;
        string path;

        do 
        {
            path = Path.Combine(folderPath, baseName + index.ToString("D2") + ".asset");
            index++;
        }
        while (File.Exists(path)); // On tente un nom et modifie l'index jusqu'a avoir un nom dispo

        TileSettings newTile = ScriptableObject.CreateInstance<TileSettings>();
        AssetDatabase.CreateAsset(newTile, path);
        AssetDatabase.SaveAssets();

        RefreshTileDataList();
        EditorGUIUtility.PingObject(newTile);
        Selection.activeObject = newTile;
    }

    private void MoveTileToTrash(TileSettings tile) // On deplace la carte dans un sous dossier Trash
    {
        string originalPath = AssetDatabase.GetAssetPath(tile);
        string trashPath = "Assets/Script/Data/Trash";

        if (!AssetDatabase.IsValidFolder(trashPath))
            AssetDatabase.CreateFolder("Assets/Script/Data", "Trash");

        string fileName = Path.GetFileName(originalPath);
        string newPath = Path.Combine(trashPath, fileName);

        AssetDatabase.MoveAsset(originalPath, newPath);
        AssetDatabase.SaveAssets();
        RefreshTileDataList();
    }

    private void OnEnable()
    {
        RefreshTileDataList();
    }

    private void OnDisable()
    {
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUIStyle headerStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        GUILayout.Label("Tile List", headerStyle);
        GUILayout.Space(5);

        if (GUILayout.Button("Refresh"))
        {
            RefreshTileDataList();
        }
        GUILayout.Space(5);

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Create New Tile"))
        {
            CreateNewTile();
        }
        GUI.backgroundColor = originalColor;
        GUILayout.Space(5);

        _scroll = GUILayout.BeginScrollView(_scroll);

        Rect line = EditorGUILayout.GetControlRect(false, 2); // Rectangle blanc plutot qu'une ligne
        EditorGUI.DrawRect(line, Color.white);

        for (int i = _tileDataList.Count - 1; i >= 0; i--) // Comme un for each, mais permet de del / add sans risque
        {
            var tile = _tileDataList[i];

            GUILayout.Space(5);

            if (!_foldouts.ContainsKey(tile))
                _foldouts[tile] = false;

            GUIStyle foldoutStyle = new(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green },
                onNormal = { textColor = Color.green }
            };

            _foldouts[tile] = EditorGUILayout.Foldout(_foldouts[tile], tile.name, true, foldoutStyle);

            if (_foldouts[tile])
            {
                GUILayout.Space(3);

                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
                EditorGUILayout.BeginVertical("HelpBox");
                GUI.backgroundColor = originalColor;

                Editor editor = Editor.CreateEditor(tile);
                editor.OnInspectorGUI();

                GUILayout.Space(10);

                ///////////////
                // RENAME FIELD
                if (!_renameBuffer.ContainsKey(tile))
                    _renameBuffer[tile] = tile.name;

                EditorGUILayout.BeginHorizontal();
                _renameBuffer[tile] = EditorGUILayout.TextField("Tile Name : ", _renameBuffer[tile]);

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Rename", GUILayout.Width(80))) // Bouton rename
                {
                    string path = AssetDatabase.GetAssetPath(tile);
                    AssetDatabase.RenameAsset(path, _renameBuffer[tile]);
                    AssetDatabase.SaveAssets();

                    EditorApplication.delayCall += RefreshTileDataList;
                }
                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(3);

                ///////////////
                // DELETE FIELD
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Move tile to trash"))
                {
                    MoveTileToTrash(tile);
                    EditorApplication.delayCall += RefreshTileDataList;
                }

                GUI.backgroundColor = originalColor;

                GUILayout.Space(3);

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
            line = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(line, Color.white);

        } // End of the for loop

        GUILayout.Space(10);

        if (GUILayout.Button("Open Tile Data Folder"))
        {
            string folderPath = "Assets/Script/Data/Trash"; //On vise trash car sinon ca n'ouvre pas Data
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);

            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                Debug.LogWarning("Folder not found: " + folderPath);
            }
        }

        GUILayout.EndScrollView();
    }
}
