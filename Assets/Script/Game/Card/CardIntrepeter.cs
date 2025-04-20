using UnityEngine;
using UnityEngine.UI;
using CardGame.Card;
using TMPro;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CardGame.Card
{
	public class CardIntrepeter : MonoBehaviour
	{
		[SerializeField]
		private Image _background;
		[SerializeField]
		private Image _image;
		[SerializeField]
		private TextMeshProUGUI _title;
		[SerializeField]
		private TextMeshProUGUI _description;

		[Space(10)]

		[SerializeField, HideInInspector]
		private CardScriptable _cardInfo;

		public CardScriptable CardInfo
		{
			get => _cardInfo;
			private set => _cardInfo = value;
		}

		public Vector3 GoToPos { get; set; }
		public float Speed { get; set; }

		public void SetUp(CardScriptable cardInfo)
		{
			_cardInfo = cardInfo;

			_image.sprite = _cardInfo.Visual;
			_background.sprite = _cardInfo.Background;
			_title.text = _cardInfo.CardName;
			_description.text = _cardInfo.CardDescritpion;

			_cardInfo.VisualEffect.ForEach(x => x.Init(this));
			_cardInfo.CardEffects.ForEach(x => x.Init(this));
		}

		private void Update()
		{
			if (!IsAtDestination())
			{
				Vector3 direction = (GoToPos - transform.position);
				transform.position += Speed * Time.deltaTime * direction;
			}
			else transform.position = GoToPos;
		}

		public bool IsAtDestination()
		{
			return Vector3.Distance(transform.position, GoToPos) < 0.1f;
		}

		#region CardEffect
		public void OnCardPlaced()
		{
			_cardInfo.VisualEffect.ForEach(x => x.OnCardPlaced());
		}

		public void OnCardRetrieve()
		{
			_cardInfo.VisualEffect.ForEach(x => x.OnCardRetrieve());
		}

		public void BeforeScoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.BeforeScoring());
		}

		public void Scoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.Scoring());
		}

		public void AfterScoring()
		{
			_cardInfo.CardEffects.ForEach(x => x.AfterScoring());
		}
		#endregion
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(CardIntrepeter))]
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