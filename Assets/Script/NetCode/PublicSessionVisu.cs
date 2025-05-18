using TMPro;
using UnityEngine;

namespace CardGame.Net
{
	public class PublicSessionVisu : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _textName, _textCode;

		public string GameName { private set; get; }
		private string _code;

		private LocalNetController _testLinkUI;

		public void SetUpVisu(string name, string code, LocalNetController link)
		{
			_textName.text = name;
			_textCode.text = $"({code})";

			GameName = name;
			_code = code;

			_testLinkUI = link;
		}

		public void JoinSession()
		{
			_testLinkUI.JoinGame(_code);
		}
	}
}