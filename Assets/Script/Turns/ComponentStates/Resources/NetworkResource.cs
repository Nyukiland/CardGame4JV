using Cysharp.Threading.Tasks;
using CardGame.StateMachine;
using Unity.Netcode;
using CardGame.Net;

namespace CardGame.Turns
{
	public class NetworkResource : Resource
	{
		public NetCommunication NetCom { get; private set; }

		public override void Init(Controller owner)
		{
			base.Init(owner);

			GetNetComForThisClientAsync().Forget();
		}

		public bool IsNetActive()
		{
			return NetCom != null;
		}

		private async UniTask GetNetComForThisClientAsync()
		{
			NetCommunication netCom = null;

			await UniTask.WaitUntil(() =>
				NetCommunication.Instances.TryGetValue(NetworkManager.Singleton.LocalClientId, out netCom));

			NetCom = netCom;
			//link event
		}
	}
}