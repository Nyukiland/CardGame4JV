using System.Collections.Generic;
using CardGame.Card;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class TileUI : MonoBehaviour
    {
        #region variables

        [Header("Data")] 
        [SerializeField] private TextMeshProUGUI _healthPoints;
        [SerializeField] private TextMeshProUGUI _attackPoints;
        [SerializeField] private TextMeshProUGUI _manaCost;

        [Header("Visuals")]
        [SerializeField] private Transform _visualContainer;
        [SerializeField] private List<Sprite> _spriteList;
        
        private RectTransform _tileRectTransform;
        public RectTransform TileRectTransform => _tileRectTransform;

        public TileSettings TileSettings { get; set; }

        [SerializeField] private Image _image;
        public Image Image => _image;


        #endregion

        public void InitTile(TileSettings tileSettingsRef)
        {
            _tileRectTransform = GetComponent<RectTransform>();
            TileSettings = tileSettingsRef;
            UpdateTexts();
            UpdateImages();
        }

        public void ChangeParent(Transform parent)
        {
            gameObject.transform.SetParent(parent);
        }

        public void UpdateTexts()
        {
            //_healthPoints.text = TileData.CurrentHealthPoints.ToString();
            //_attackPoints.text = TileData.CurrentAttackPoints.ToString();
            //_manaCost.text = TileData.CurrentManaCost.ToString();
        }

        private void UpdateImages()
        {
            Transform container = _visualContainer;

            if (this == null || container == null || container == null)
                return;

            // destroy previous sprites
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = container.transform.GetChild(i);

                if (child != null)
                    Destroy(child.gameObject);
            }

            // create new sprites
            foreach (Sprite sprite in _spriteList)
            {
                GameObject newGameObject = new("Image");
                newGameObject.transform.SetParent(container, false);

                Image newImage = newGameObject.AddComponent<Image>();
                newImage.sprite = sprite;
                newImage.type = Image.Type.Sliced;

                RectTransform rt = newGameObject.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;
            }
        }

        #region OnEditor

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateImagesEditor();
        }

        private void UpdateImagesEditor()
        {
            // returns if the gameObject is part of a PrefabAsset and not a scene
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            Transform container = _visualContainer;

            EditorApplication.delayCall += () =>
            {
                if (this == null || container == null || container == null)
                    return;

                // destroy previous sprites
                for (int i = container.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = container.transform.GetChild(i);

                    if (child != null)
                        DestroyImmediate(child.gameObject);
                }

                // create new sprites
                foreach (Sprite sprite in _spriteList)
                {
                    GameObject newGameObject = new("Image");
                    newGameObject.transform.SetParent(container, false);

                    Image newImage = newGameObject.AddComponent<Image>();
                    newImage.sprite = sprite;
                    newImage.type = Image.Type.Sliced;

                    RectTransform rt = newGameObject.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.localPosition = Vector3.zero;
                    rt.localScale = Vector3.one;
                }
            };
        }
#endif

        #endregion
    }
}