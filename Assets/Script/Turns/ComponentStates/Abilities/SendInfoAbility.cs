using CardGame.StateMachine;
using CardGame.Net;
using CardGame.Card;
using UnityEngine;

namespace CardGame.Turns
{
	public class SendInfoAbility : Ability
	{
		private NetworkResource _net;
		private int _tileInHand;

		public override void Init(Controller owner)
		{
			base.Init(owner);

			_net = owner.GetStateComponent<NetworkResource>();
            _tileInHand = owner.GetStateComponent<CreateHandAbility>().CountCard;
        }

		public void AskForSetUp()
		{
			if (!_net.IsNetActive()) return;

			_net.NetCom.SetUp(_tileInHand);
		}

		public void SendGridToOthers()
		{
			if (!_net.IsNetActive()) return;

			_net.NetCom.GenerateFirstGrid();
		}

		public void SendInfoTilePlaced(TileData send, Vector2Int pos)
		{
			if (!_net.IsNetActive()) return;
			_net.NetCom.SendTilePlaced(new DataToSend(send, pos));
		}

		public bool SendDiscardTile(int ID)
		{
			if (!_net.IsNetActive()) return false;

			_net.NetCom.SendDiscard(ID);
			return true;
		}

		public void SendTauntShake(Vector2 pos, bool special)
		{
			if (!_net.IsNetActive()) return;
			_net.NetCom.SendTauntShakeNet(pos, special);
		}

		public void SendTaunt(string taunt)
		{
			if (!_net.IsNetActive()) return;
			_net.NetCom.SendTauntNet(taunt);
		}

		public bool SendTurnFinished()
		{
			if (!_net.IsNetActive()) return false;

			_net.NetCom.TurnFinished();
			return true;
		}
	}
}