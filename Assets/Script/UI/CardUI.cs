using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class CardUI : MonoBehaviour
    {
        #region variables

        [Header("Data")] [SerializeField] private TextMeshProUGUI _lifePoints;
        [SerializeField] private TextMeshProUGUI _attackPoints;
        [SerializeField] private TextMeshProUGUI _manaCost;

        [Header("Visuals")] [SerializeField] private Transform _visualContainer;
        [SerializeField] private List<Sprite> _spriteList;

        // private CardData _cardData;

        #endregion

        public void InitCard()
        {
            UpdateTexts();
            UpdateImages();
        }

        public void UpdateTexts()
        {
            // _lifePoints.text = _cardData.LifePoints;
            // _attackPoints.text = _cardData.AttackPoints;
            // _manaCost.text = _cardData.ManaCost;
        }

        private void UpdateImages()
        {
            // returns if the gameObject is part of a PrefabAsset and not a scene
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            Transform container = _visualContainer;

            EditorApplication.delayCall += () =>
            {
                if (this == null || container == null || container == null)
                    return;

                for (int i = container.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = container.transform.GetChild(i);

                    if (Application.isPlaying)
                    {
                        if (child != null)
                            Destroy(child.gameObject);
                    }
                    else
                    {
                        if (child != null)
                            DestroyImmediate(child.gameObject);
                    }
                }

                foreach (Sprite sprite in _spriteList)
                {
                    GameObject newGameObject = new GameObject("Image");
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateImages();
        }
#endif
    }
}