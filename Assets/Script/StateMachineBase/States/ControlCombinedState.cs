using CardGame.StateMachine;

namespace CardGame.StateMachine
{
    public class ControlCombinedState : CombinedState
    {
        public ControlCombinedState()
        {
            AddSubState(new ClickSubState());
        }
    }
}
