using CardGame.StateMachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGame.Blanco
{
    public class TimerAbility : Ability
    {
        public float ElapsedTime { get; private set; }
        public const float HOLD_DURATION = 2f;
        
        [SerializeField] private TextMeshProUGUI _timerText;

        public override void OnEnable()
        {
            ElapsedTime = 0f;
            _timerText.gameObject.SetActive(true);
        }

        public override void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
            _timerText.text = ElapsedTime.ToString("0.00");
        }

        public override void OnDisable()
        {
            _timerText.gameObject.SetActive(false);
        }
    }
}