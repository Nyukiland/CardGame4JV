using UnityEngine;
using UnityEngine.UI;
using CardGame.Card;
using TMPro;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CardGame.Card
{
	public class CardInfo : MonoBehaviour
	{
		[Header("SetUp")]
		[SerializeField]
		private Image _background;
		[SerializeField]
		private Image _image;
		[SerializeField]
		private TextMeshProUGUI _title;
		[SerializeField]
		private TextMeshProUGUI _description;

		[Space()]
		[Header("Card Info")]

		[SerializeField] 
		private string _cardName;
		[SerializeField] 
		private string _cardDescritpion;
		[SerializeField] 
		private Sprite _visual;
		[SerializeField] 
		private Sprite _backgroundImage;

		[Space(10)]

		public List<CardEffectContainerScriptable> CardEffects = new();

		private Vector3 _goToPos;
		private float _speed;

		private void OnValidate()
		{
			_image.sprite = _visual;
			_background.sprite = _backgroundImage;
			_title.text = _cardName;
			gameObject.name = _cardName;
			_description.text = _cardDescritpion;
		}

		public void SetUp(CardInfo cardInfo)
		{
			CardEffects.ForEach(x => x.Effect.Init(this));
			CardEffects.ForEach(x => x.Effect.Init(this));
		}

		private void Update()
		{
			if (!IsAtDestination())
			{
				Vector3 direction = (_goToPos - transform.position);
				transform.position += _speed * Time.deltaTime * direction;
				//transform.rotation.SetLookRotation(Vector3.forward, direction.normalized);
			}
			else
			{
				transform.position = _goToPos;
				//transform.rotation = Quaternion.identity;
			}
		}

		public bool IsAtDestination()
		{
			return Vector3.Distance(transform.position, _goToPos) < 0.1f;
		}

		public bool ContainsPos(Vector3 pos)
		{
			return _background.rectTransform.rect.Contains(pos);
		}

		public void MoveCard(Vector3 pos, float speed)
		{
			_goToPos = pos;
			_speed = speed;
		}

		#region CardEffect
		public void OnCardPlaced()
		{
			CardEffects.ForEach(x => x.Effect.OnCardPlaced());
		}

		public void OnCardRetrieve()
		{
			CardEffects.ForEach(x => x.Effect.OnCardRetrieve());
		}

		public void BeforeScoring()
		{
			CardEffects.ForEach(x => x.Effect.BeforeScoring());
		}

		public void Scoring()
		{
			CardEffects.ForEach(x => x.Effect.Scoring());
		}

		public void AfterScoring()
		{
			CardEffects.ForEach(x => x.Effect.AfterScoring());
		}
		#endregion
	}
}

/*
#if UNITY_EDITOR

[CustomEditor(typeof(CardInfo))]
public class CardIntrepeterCustomInspector : Editor
{
	private bool _foldout;

	SerializedProperty _cardInfoProperty;

	private void OnEnable()
	{
		_cardInfoProperty = serializedObject.FindProperty("_cardInfo");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		base.OnInspectorGUI();

		GUI.enabled = false;

		EditorGUILayout.PropertyField(_cardInfoProperty);

		if (_cardInfoProperty.objectReferenceValue == null)
		{
			serializedObject.ApplyModifiedProperties();
			return;
		}

		GUI.enabled = true;

		_foldout = EditorGUILayout.Foldout(_foldout, "Card Info");

		GUI.enabled = false;

		if (_foldout)
		{
			Editor scriptableEditor = CreateEditor(_cardInfoProperty.objectReferenceValue);
			if (scriptableEditor != null)
			{
				EditorGUILayout.Space();
				scriptableEditor.OnInspectorGUI();
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}

#endif
*/