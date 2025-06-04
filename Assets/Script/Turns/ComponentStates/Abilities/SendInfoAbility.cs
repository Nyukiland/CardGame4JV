using CardGame.StateMachine;
using CardGame.Net;

namespace CardGame.Turns
{
	public class SendInfoAbility : Ability
	{
		private NetworkResource _net;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_net = owner.GetStateComponent<NetworkResource>();
		}

		public void SendInfoTileMoved(DataToSend send)
		{
			if (!_net.IsNetActive()) return;

			//_net.NetCom
		}

		public void SendInfoTilePlaced(DataToSend send)
		{
			if (!_net.IsNetActive()) return;

			//_net.NetCom
		}

		public void SendTurnFinished()
		{
			if (!_net.IsNetActive()) return;

			//_net.NetCom
		}
	}
}