using CardGame;
using CardGame.Card;
using CardGame.UI;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.AddressableAssets.Settings.AddressableAssetSettings;

public class TileCreator : EditorWindow
{
    private Vector2 _scrollLeft;
    private Vector2 _scrollRight;
    private List<TileSettings> _tileDataList = new();
    private TileSettings _selectedTile;
    private Dictionary<TileSettings, Texture2D> _tilePreviews = new();

    private void RefreshTileDataList()
    {
        _tileDataList.Clear();
        _tilePreviews.Clear(); // Reset previews

        string[] guids = AssetDatabase.FindAssets("t:TileSettings", new[] { "Assets/Script/Data" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/Trash")) continue;

            TileSettings tile = AssetDatabase.LoadAssetAtPath<TileSettings>(path);
            if (tile != null) _tileDataList.Add(tile);
        }
    }

    [MenuItem("Tools/Tile Creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<TileCreator>();
        window.titleContent = new GUIContent("Tile Creator");
        window.Show();
    }

    private void OnEnable()
    {
        RefreshTileDataList();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // COLONNE GAUCHE
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        GUILayout.Label("Tiles", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh"))
            RefreshTileDataList();

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Create New Tile"))
            CreateNewTile();
        GUI.backgroundColor = originalColor;

        _scrollLeft = EditorGUILayout.BeginScrollView(_scrollLeft);

        foreach (var tile in _tileDataList)
        {
            Color originalBg = GUI.backgroundColor;

            if (tile == _selectedTile)
                GUI.backgroundColor = Color.gray * 0.6f;

            if (GUILayout.Button(RenderTilePreview(tile), GUILayout.Width(180), GUILayout.Height(180)))
            {
                GUI.FocusControl(null);
                _selectedTile = tile;
            }

            GUI.backgroundColor = originalBg;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // COLONNE DROITE 
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Tile Informations", EditorStyles.boldLabel);
        _scrollRight = EditorGUILayout.BeginScrollView(_scrollRight);

        if (_selectedTile != null)
        {
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
            EditorGUILayout.BeginVertical("HelpBox");
            GUI.backgroundColor = originalColor;

			SerializedObject serializedTile = new SerializedObject(_selectedTile);
			serializedTile.Update();

			SerializedProperty presetProp = serializedTile.FindProperty("tilePreset");
			EditorGUILayout.PropertyField(presetProp);

			TilePreset preset = (TilePreset)presetProp.enumValueIndex;

            // Pour update les valeurs
			ZoneData north = _selectedTile.NorthZone;
			ZoneData east = _selectedTile.EastZone;
			ZoneData south = _selectedTile.SouthZone;
			ZoneData west = _selectedTile.WestZone;

			switch (preset)
			{
				case TilePreset.FourDifferentClosed:
					north.environment = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("North", north.environment);
					west.environment = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("West", west.environment);
					east.environment = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("East", east.environment);
					south.environment = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("South", south.environment);

					north.isOpen = false;
					west.isOpen = false;
					east.isOpen = false;
					south.isOpen = false;
					break;

				case TilePreset.ThreeSame:
					ENVIRONEMENT_TYPE small = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Small Zone", north.environment);
					ENVIRONEMENT_TYPE big = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Big Zone", east.environment);

					north.environment = small; north.isOpen = false;
					east.environment = big; east.isOpen = true;
					south.environment = big; south.isOpen = true;
					west.environment = big; west.isOpen = true;
					break;

				case TilePreset.DiagonalOpenFull:
					ENVIRONEMENT_TYPE diagA = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Diagonal A (N/E) : ", north.environment);
					ENVIRONEMENT_TYPE diagB = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Diagonal B (S/W) : ", south.environment);

					north.environment = diagA; north.isOpen = true;
                    east.environment = diagA; east.isOpen = true;

					south.environment = diagB; south.isOpen = true;
					west.environment = diagB; west.isOpen = true;
					break;

				case TilePreset.DiagonalOpenHalf:
					ENVIRONEMENT_TYPE a = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Open Diagonal (N/E)", north.environment);
					ENVIRONEMENT_TYPE b = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("South : ", south.environment);
					ENVIRONEMENT_TYPE c = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("West : ", west.environment);

					north.environment = a; north.isOpen = true;
					east.environment = a; east.isOpen = true;

					south.environment = b; south.isOpen = false;
					west.environment = c; west.isOpen = false;
					break;

				case TilePreset.Path:
					ENVIRONEMENT_TYPE main = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("Main Path (N & S) : ", north.environment);

					ENVIRONEMENT_TYPE capEast = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("East : ", east.environment);
					ENVIRONEMENT_TYPE capWest = (ENVIRONEMENT_TYPE)EditorGUILayout.EnumPopup("West : ", west.environment);

					north.environment = main; north.isOpen = true;
					south.environment = main; south.isOpen = true;

					west.environment = capWest; west.isOpen = false;
                    east.environment = capEast; east.isOpen = false;


					break;

			}

			serializedTile.ApplyModifiedProperties();

			// Set updated zone data back into the tile
			_selectedTile.SetZones(north, east, south, west);
			EditorUtility.SetDirty(_selectedTile);

			if (EditorGUI.EndChangeCheck())
            {
                _tilePreviews.Remove(_selectedTile);
                Repaint(); // force l’UI à redessiner la preview
            }

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            _selectedTile.PoolIndex = EditorGUILayout.IntField("Pool Index", _selectedTile.PoolIndex);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_selectedTile); // Marque comme modifié pour que Unity sauvegarde
            }

			GUILayout.Space(5);
			EditorGUI.BeginChangeCheck();
			_selectedTile.NumberOfCopies = EditorGUILayout.IntField("Number of Copies", _selectedTile.NumberOfCopies);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_selectedTile);
			}

			GUILayout.Space(10);

            // RENAME 
            EditorGUILayout.BeginHorizontal();
            _selectedTile.name = EditorGUILayout.TextField("Tile Name", _selectedTile.name);

            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Rename", GUILayout.Width(80)))
            {
                string path = AssetDatabase.GetAssetPath(_selectedTile);
                AssetDatabase.RenameAsset(path, _selectedTile.name);
                AssetDatabase.SaveAssets();
                _tilePreviews.Clear(); // Clear cache
                EditorApplication.delayCall += RefreshTileDataList;
            }
            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // DELETE 
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Move tile to trash"))
            {
                MoveTileToTrash(_selectedTile);
                _selectedTile = null;
                _tilePreviews.Clear(); // Clear cache
                EditorApplication.delayCall += RefreshTileDataList;
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("Select a tile from the list.");
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
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
        } while (File.Exists(path));

        TileSettings newTile = ScriptableObject.CreateInstance<TileSettings>();

        GameObject prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tile.prefab");
        if (prefabGO != null)
        {
            TileVisu tileUI = prefabGO.GetComponent<TileVisu>();
            if (tileUI != null)
            {
                SerializedObject serializedTile = new SerializedObject(newTile);
                var tileUIPrefabProp = serializedTile.FindProperty("_tileUIPrefab");
                tileUIPrefabProp.objectReferenceValue = tileUI;
                serializedTile.ApplyModifiedProperties();
            }
        }

        AssetDatabase.CreateAsset(newTile, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

#if UNITY_EDITOR
        string assetGUID = AssetDatabase.AssetPathToGUID(path);
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

        if (settings != null)
        {
            var entry = settings.CreateOrMoveEntry(assetGUID, settings.DefaultGroup);
            entry.SetAddress(path);
            entry.labels.Add("TileSetting");
            settings.SetDirty(ModificationEvent.EntryModified, entry, true);
        }
#endif

        _tilePreviews.Clear(); // Clear cache
        RefreshTileDataList();
    }

    private void MoveTileToTrash(TileSettings tile)
    {
        string originalPath = AssetDatabase.GetAssetPath(tile);
        string trashPath = "Assets/Script/Data/Trash";

        if (!AssetDatabase.IsValidFolder(trashPath))
            AssetDatabase.CreateFolder("Assets/Script/Data", "Trash");

        string fileName = Path.GetFileName(originalPath);
        string newPath = Path.Combine(trashPath, fileName);

        AssetDatabase.MoveAsset(originalPath, newPath);
        AssetDatabase.SaveAssets();
        _tilePreviews.Clear(); // Clear cache
        RefreshTileDataList();
    }

    private Texture2D RenderTilePreview(TileSettings tile)
    {
        if (_tilePreviews.TryGetValue(tile, out Texture2D cached))
            return cached;

        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black;

        Color GetColor(ENVIRONEMENT_TYPE type) => type switch
        {
			ENVIRONEMENT_TYPE.Neutral => Color.white,
			ENVIRONEMENT_TYPE.Grass => Color.green,
            ENVIRONEMENT_TYPE.Terrain => new Color(0.59f, 0.29f, 0.0f),
			ENVIRONEMENT_TYPE.Water => Color.blue,
			ENVIRONEMENT_TYPE.Fields => Color.yellow,
			_ => Color.gray
        };

        Vector2 center = new Vector2(size / 2f, size / 2f);

        bool IsInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s = a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y;
            float t = a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y;
            if ((s < 0) != (t < 0)) return false;

            float area = -b.y * c.x + a.y * (c.x - b.x) + a.x * (b.y - c.y) + b.x * c.y;
            return area < 0 ? (s <= 0 && s + t >= area) : (s >= 0 && s + t <= area);
        }

        void DrawTriangle(Color color, float angleDeg, float scale)
        {
            float radius = size * 0.95f * scale;
            Vector2 a = center;
            Vector2 b = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg - 45) * Vector2.up * radius);
            Vector2 c = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg + 45) * Vector2.up * radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    if (IsInTriangle(p, a, b, c))
                        tex.SetPixel(x, y, color);
                }
            }
        }

        void DrawZone(ZoneData zone, float angle)
        {
            DrawTriangle(GetColor(zone.environment), angle, 1f);
            if (!zone.isOpen)
                DrawTriangle(Color.black, angle, 0.4f);
        }

        DrawZone(tile.NorthZone, 0);
        DrawZone(tile.WestZone, 90);
        DrawZone(tile.SouthZone, 180);
        DrawZone(tile.EastZone, 270);

        int thickness = 2;
        for (int i = 0; i < size; i++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                int x1 = i + t;
                int y1 = i;
                int x2 = i + t;
                int y2 = size - 1 - i;

                if (x1 >= 0 && x1 < size && y1 >= 0 && y1 < size)
                    tex.SetPixel(x1, y1, Color.black);
                if (x2 >= 0 && x2 < size && y2 >= 0 && y2 < size)
                    tex.SetPixel(x2, y2, Color.black);
            }
        }

        int borderThickness = 3;
        for (int x = 0; x < size; x++)
        {
            for (int b = 0; b < borderThickness; b++)
            {
                tex.SetPixel(x, b, Color.black);
                tex.SetPixel(x, size - 1 - b, Color.black);
            }
        }
        for (int y = 0; y < size; y++)
        {
            for (int b = 0; b < borderThickness; b++)
            {
                tex.SetPixel(b, y, Color.black);
                tex.SetPixel(size - 1 - b, y, Color.black);
            }
        }

        tex.Apply();
        _tilePreviews[tile] = tex;
        return tex;
    }
}
