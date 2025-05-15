using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class ControlCombinedState : CombinedState
    {
        public ControlCombinedState()
        {
            AddSubState(new ClickSubState());
        }
    }
}