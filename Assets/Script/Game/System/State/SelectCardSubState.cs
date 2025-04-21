using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace CardGame.StateMachine
{
	public class SelectCardSubState : State
	{
		private AbilityToMoveCard _moveCard;

		public override void OnEnter()
		{
			base.OnEnter();
			GetStateComponent(ref _moveCard);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			if (Input.touchCount <= 0) return;

			Touch touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Began)
			{
				_moveCard.PickCard(touch.position);
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				_moveCard.MoveCard(touch.position);
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				_moveCard.ReleaseCard();
			}
		}
	}
}