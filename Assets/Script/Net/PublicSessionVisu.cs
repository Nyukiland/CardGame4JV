using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Net
{
	public class PublicSessionVisu : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _textName, _textCode;

		[SerializeField] private Button _connectButton;

		public string GameName { private set; get; }
		private string _code;

		private NetControllerParent _netController;
		
		public void SetUpVisu(string name, string code, NetControllerParent link)
		{
			_textName.text = name;
			_textCode.text = $"({code})";

			GameName = name;
			_code = code;

			_netController = link;
			_connectButton.onClick.AddListener(JoinSession);
		}

		public void JoinSession()
		{
			_netController.JoinGame(_code);
		}
		
		// Used only for tests
		private LocalNetControllerTestScene _testLinkUITest;
		public void SetUpVisu(string name, string code, LocalNetControllerTestScene link)
		{
			_textName.text = name;
			_textCode.text = $"({code})";

			GameName = name;
			_code = code;

			_testLinkUITest = link;
		}
	}
}