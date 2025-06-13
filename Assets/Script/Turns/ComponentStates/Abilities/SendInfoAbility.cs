using CardGame.StateMachine;
using CardGame.Net;
using CardGame.Card;
using UnityEngine;

namespace CardGame.Turns
{
	public class SendInfoAbility : Ability
	{
		private NetworkResource _net;
		private int tileInHand;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_net = owner.GetStateComponent<NetworkResource>();
            tileInHand = owner.GetStateComponent<CreateHandAbility>().CountCard;

        }

		public void AskForSetUp()
		{
			if (!_net.IsNetActive()) return;

			_net.NetCom.SetUp(tileInHand);
		}

		public void SendInfoTileMoved(DataToSend send)
		{
			if (!_net.IsNetActive()) return;

			
		}

		public void SendInfoTilePlaced(TileData send, Vector2Int pos)
		{
			if (!_net.IsNetActive()) return;
			_net.NetCom.SendTilePlaced(new DataToSend(send, pos));
		}

		public bool SendTurnFinished()
		{
			if (!_net.IsNetActive()) return false;

			_net.NetCom.TurnFinished();
			return true;
		}
	}
}