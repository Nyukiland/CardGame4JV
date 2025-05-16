using CardGame.StateMachine;
using UnityEngine;

namespace CardGame.Blanco
{
    public class HoldCombinedState : CombinedState
    {
        public HoldCombinedState()
        {
            AddSubState(new ClickSubState());
        }
    }
}