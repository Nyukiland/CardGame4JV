using UnityEngine;
using UnityEngine.UI;

namespace CardGame.StateMachine
{
	public class MoveCardAbility : Ability
	{
        private ObjectManagerResource _objectManagerResource;
        private Image _cardToMove;


        public override void Init(Controller owner)
        {
            base.Init(owner);
            _objectManagerResource = Owner.GetStateComponent<ObjectManagerResource>();
        }

        public void OnClick(Vector3 pos)
		{
            _cardToMove = _objectManagerResource.ContainsPos(pos);
        }

        public void OnMaintain(Vector3 pos)
        {
            if (_cardToMove == null)
                return;

            _cardToMove.transform.position = pos;
        }

        public void OnRelease()
        {
            if (_cardToMove == null)
                return;

            _cardToMove = null;
        }

    }
}