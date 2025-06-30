using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.Net
{
	public abstract class NetControllerParentTest : NetControllerParent
	{
		[SerializeField]
		protected TextMeshProUGUI _netComText, _receivedInfo;

		[SerializeField]
		protected TMP_InputField _passwordField, _connectCode, _gameName;

		[SerializeField]
		protected Toggle _toggleHost;

		[SerializeField]
		protected VerticalLayoutGroup _publicSessionVerticalLayout;

		protected virtual void Update()
		{
			if (_netCommunication != null)
			{
				_netComText.text = _netCommunication.gameObject.name + " / \n Join Code: " + _joinCode;
			}
		}

		protected virtual void NetCommunication_ReceiveEvent(string data)
		{
			_receivedInfo.text = data;
		}

		public virtual void SendInfo()
		{
			_netCommunication.SubmitInfoTest(_netCommunication.name + " / \\n password: " + _passwordField.text);
		}

		protected virtual void OnDestroy()
		{
			if (_netCommunication != null)
			{
				_netCommunication.ReceiveEventTest -= NetCommunication_ReceiveEvent;
			}
		}

		protected override async UniTask GetNetComForThisClientAsync()
		{
			await base.GetNetComForThisClientAsync();
			_netCommunication.ReceiveEventTest += NetCommunication_ReceiveEvent;
		}
	}
}