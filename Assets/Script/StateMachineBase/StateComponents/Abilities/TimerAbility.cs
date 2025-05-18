using TMPro;

namespace CardGame.StateMachine
{
    public class TimerAbility : Ability
	{
        public TMP_Text TimerDisplay;
		private float _maximumElapsedTime;
        private float _elapsedTime;

        public float GetElapsedTime()
        {
            return _elapsedTime;
        }

        public override void OnEnable()
        {
            _elapsedTime = _maximumElapsedTime;
            TimerDisplay.gameObject.SetActive(true);
        }

        public override void OnDisable()
        {
            TimerDisplay.gameObject.SetActive(false);
        }

        public override void Update(float deltaTime) 
        {
            _elapsedTime += deltaTime;
            TimerDisplay.text = _elapsedTime.ToString();
        }
    }

}