using UnityEngine;


namespace CardGame.StateMachine
{
	public class HoldState : State
	{
        float elapsedTime;

        public override void OnEnter()
        {
            Debug.Log("HoldState");
        }


        

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            elapsedTime += deltaTime;

            if (elapsedTime >= 2.0f)
                Controller.SetState<ClickState>();
        }
    }
}
