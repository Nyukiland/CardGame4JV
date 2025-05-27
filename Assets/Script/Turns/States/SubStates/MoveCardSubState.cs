using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Turns
{
    public class MoveCardSubState : State
    {
        private MoveCardAbility _moveCardAbility;
        private ZoneHolderResource _zoneHolderResource;

        public override void OnEnter()
        {
            base.OnEnter();
            GetStateComponent(ref _moveCardAbility);
        }
        
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Input.touchCount <= 0) return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _moveCardAbility.PickCard(touch.position);
                    break;
                case TouchPhase.Moved:
                    _moveCardAbility.MoveCard(touch.position);
                    break;
                case TouchPhase.Ended:
                    _moveCardAbility.ReleaseCard(touch.position);
                    break;
            }
        }
    }
}