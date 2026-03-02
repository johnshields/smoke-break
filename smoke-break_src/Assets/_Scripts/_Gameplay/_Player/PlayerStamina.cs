using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts._Gameplay._Player
{
    public class PlayerStamina : MonoBehaviour
    {
        #region Variables

        private const int LowHealthSprintThreshold = 50;

        private const float
            LowHealthSprintMultiplier = 1.5f,
            FullHealthSprintMultiplier = 2f;

        [Header("Sprint Settings")] [SerializeField]
        private float maxStamina = 100f;

        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaDrainRate = 5f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float sprintMultiplier = 2f;

        private PlayerHealth _playerHealth;
        private InputControls _actions;
        private bool _isSprinting;
        private bool _isExhausted;

        #endregion

        #region Properties

        public bool IsSprinting => _isSprinting;
        public float SprintMultiplier => _isSprinting ? sprintMultiplier : 1f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _actions = new InputControls();
            currentStamina = maxStamina;
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Sprint.performed += StartSprinting;
            _actions.Profiler.Sprint.canceled += StopSprinting;
        }

        private void OnDisable()
        {
            _actions.Profiler.Sprint.performed -= StartSprinting;
            _actions.Profiler.Sprint.canceled -= StopSprinting;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            DrainStamina();
        }

        #endregion

        #region Public API

        public float GetMaxStamina() => maxStamina;

        public float GetCurrentStamina() => currentStamina;

        public bool TryConsumeStamina(float amount)
        {
            if (currentStamina < amount) return false;
            currentStamina -= amount;
            return true;
        }

        #endregion

        #region Sprint & Stamina

        private void StartSprinting(InputAction.CallbackContext context)
        {
            if (_isExhausted) return;

            sprintMultiplier = _playerHealth.GetCurrentHealth() < LowHealthSprintThreshold
                ? LowHealthSprintMultiplier
                : FullHealthSprintMultiplier;
            _isSprinting = true;
        }

        private void StopSprinting(InputAction.CallbackContext context)
        {
            _isSprinting = false;
        }

        private void DrainStamina()
        {
            if (_isSprinting)
            {
                currentStamina = Mathf.Clamp(currentStamina - staminaDrainRate * Time.deltaTime, 0, maxStamina);
                _isExhausted = currentStamina == 0;
                if (_isExhausted) _isSprinting = false;
            }
            else
            {
                currentStamina = Mathf.Clamp(currentStamina + staminaRegenRate * Time.deltaTime, 0, maxStamina);
                _isExhausted = currentStamina < maxStamina / 2;
            }
        }

        #endregion
    }
}
