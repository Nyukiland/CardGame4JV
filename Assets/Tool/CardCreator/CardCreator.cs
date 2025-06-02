using CardGame.Card;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CardCreator : EditorWindow
{
    private Vector2 _scroll;
    private Dictionary<CardData, bool> _foldouts = new(); // Pour save les menus pliés et dépliés
    private Dictionary<CardData, string> _renameBuffer = new(); // Pour save le nom qu'on est en train de mettre


    [MenuItem("Tools/CardCreator")]
    public static void ShowWindow()
    {
        CardCreator window = GetWindow<CardCreator>();
        window.titleContent = new GUIContent("Card Creator");
        window.Show();
    }

    private List<CardData> _cardDataList = new();

    private void RefreshCardDataList()
    {
        _cardDataList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { "Assets/Script/Data" }); // Dossier qui contient les cartes

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.Contains("/Trash")) // - la corbeille
                continue;

            CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (cardData != null)
                _cardDataList.Add(cardData);
        }
    }

    private void CreateNewCard()
    {
        string folderPath = "Assets/Script/Data";
        string baseName = "NewCard_";
        int index = 1;
        string path;

        do 
        {
            path = Path.Combine(folderPath, baseName + index.ToString("D2") + ".asset");
            index++;
        }
        while (File.Exists(path)); // On tente un nom et modifie l'index jusqu'a avoir un nom dispo

        CardData newCard = ScriptableObject.CreateInstance<CardData>();
        AssetDatabase.CreateAsset(newCard, path);
        AssetDatabase.SaveAssets();

        RefreshCardDataList();
        EditorGUIUtility.PingObject(newCard);
        Selection.activeObject = newCard;
    }

    private void MoveCardToTrash(CardData card) // On deplace la carte dans un sous dossier Trash
    {
        string originalPath = AssetDatabase.GetAssetPath(card);
        string trashPath = "Assets/Script/Data/Trash";

        if (!AssetDatabase.IsValidFolder(trashPath))
            AssetDatabase.CreateFolder("Assets/Script/Data", "Trash");

        string fileName = Path.GetFileName(originalPath);
        string newPath = Path.Combine(trashPath, fileName);

        AssetDatabase.MoveAsset(originalPath, newPath);
        AssetDatabase.SaveAssets();
        RefreshCardDataList();
    }

    private void OnEnable()
    {
        RefreshCardDataList();
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

        GUILayout.Label("Card List", headerStyle);
        GUILayout.Space(5);

        if (GUILayout.Button("Refresh"))
        {
            RefreshCardDataList();
        }
        GUILayout.Space(5);

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Create New Card"))
        {
            CreateNewCard();
        }
        GUI.backgroundColor = originalColor;
        GUILayout.Space(5);

        _scroll = GUILayout.BeginScrollView(_scroll);

        Rect line = EditorGUILayout.GetControlRect(false, 2); // Rectangle blanc plutot qu'une ligne
        EditorGUI.DrawRect(line, Color.white);

        for (int i = _cardDataList.Count - 1; i >= 0; i--) // Comme un for each, mais permet de del / add sans risque
        {
            var card = _cardDataList[i];

            GUILayout.Space(5);

            if (!_foldouts.ContainsKey(card))
                _foldouts[card] = false;

            GUIStyle foldoutStyle = new(EditorStyles.foldout)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green },
                onNormal = { textColor = Color.green }
            };

            _foldouts[card] = EditorGUILayout.Foldout(_foldouts[card], card.name, true, foldoutStyle);

            if (_foldouts[card])
            {
                GUILayout.Space(3);

                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
                EditorGUILayout.BeginVertical("HelpBox");
                GUI.backgroundColor = originalColor;

                Editor editor = Editor.CreateEditor(card);
                editor.OnInspectorGUI();

                GUILayout.Space(10);

                ///////////////
                // RENAME FIELD
                if (!_renameBuffer.ContainsKey(card))
                    _renameBuffer[card] = card.name;

                EditorGUILayout.BeginHorizontal();
                _renameBuffer[card] = EditorGUILayout.TextField("Card Name : ", _renameBuffer[card]);

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Rename", GUILayout.Width(80))) // Bouton rename
                {
                    string path = AssetDatabase.GetAssetPath(card);
                    AssetDatabase.RenameAsset(path, _renameBuffer[card]);
                    AssetDatabase.SaveAssets();

                    EditorApplication.delayCall += RefreshCardDataList;
                }
                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(3);

                ///////////////
                // DELETE FIELD
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Move card to trash"))
                {
                    MoveCardToTrash(card);
                    EditorApplication.delayCall += RefreshCardDataList;
                }

                GUI.backgroundColor = originalColor;

                GUILayout.Space(3);

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
            line = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(line, Color.white);
        }


        GUILayout.EndScrollView();
    }
}
