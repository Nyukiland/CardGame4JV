using System;
using System.Collections;
using System.Collections.Generic;
using CardGame.Card;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
	public class CardUI : MonoBehaviour
	{
		#region variables

		[Header("Data")][SerializeField] private TextMeshProUGUI _healthPoints;
		[SerializeField] private TextMeshProUGUI _attackPoints;
		[SerializeField] private TextMeshProUGUI _manaCost;

		[Header("Visuals")][SerializeField] private Transform _visualContainer;
		[SerializeField] private List<Sprite> _spriteList;

		public CardData _cardData;

		#endregion

		public void InitCard(CardData cardDataRef)
		{
			_cardData = cardDataRef;
			UpdateTexts();
			UpdateImages();
		}

		public void UpdateTexts()
		{
			_healthPoints.text = _cardData.CurrentHealthPoints.ToString();
			_attackPoints.text = _cardData.CurrentAttackPoints.ToString();
			_manaCost.text = _cardData.CurrentManaCost.ToString();
		}

		private IEnumerator UpdateImages()
		{
			// returns if the gameObject is part of a PrefabAsset and not a scene
			if (PrefabUtility.IsPartOfPrefabAsset(this))
				StopCoroutine(UpdateImages());

			Transform container = _visualContainer;

			yield return null;


			if (this == null || container == null || container == null)
				StopCoroutine(UpdateImages());

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
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			StartCoroutine(UpdateImages());
			if (_cardData == null) return;
			_cardData.InitData();
			InitCard(_cardData);
		}
#endif
	}
}