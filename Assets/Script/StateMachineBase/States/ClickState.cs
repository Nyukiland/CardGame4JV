using CardGame.StateMachine;
using Unity.VisualScripting;
using UnityEngine;

namespace CardGame.StateMachine
{
    public class ClickState : State
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Entered ClickState");
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Controller.SetState<HoldState>(); //ca passe directement a hold donc il faut ajouter un timer
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Exiting ClickState");
        }
    }
}
