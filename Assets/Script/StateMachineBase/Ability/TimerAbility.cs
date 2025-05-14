using TMPro;
using UnityEngine;

namespace CardGame.StateMachine
{
    public class TimerAbility : Ability
    {
        [SerializeField] private float maxTime = 2f;
        private float _currentTime;

        [SerializeField] private TextMeshProUGUI timerText;

        public float CurrentTime => _currentTime;

        public override void OnEnable()
        {
            base.OnEnable();
            _currentTime = 0f;

            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                timerText.text = $"{(maxTime - _currentTime):F1}";
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _currentTime += deltaTime;

            if (timerText != null)
            {
                float timeLeft = Mathf.Max(0f, maxTime - _currentTime);
                timerText.text = $"{timeLeft:F1}";
            }
        }

        public bool IsFinished()
        {
            return _currentTime >= maxTime;
        }
    }
}
