using System.Collections;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class PlayerProfiler : MonoBehaviour
    {
        [Header("Animation Parameters")] private int _speedHash;
        private int _groundedHash;
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Dodge = Animator.StringToHash("Dodge");

        [Header("References")] private Camera _mainCamera;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private InputControls _actions;
        private InputAction _moveKeys;
        private PistolProfiler _pistol;

        [Header("Health Settings")] public int maxHealth = 100;
        public int currentHealth;

        [Header("Movement Settings")] public float movementForce = 1f;
        private Vector3 _forceDirection = Vector3.zero;
        private const float MaxSpeed = 5f;

        [Header("Jump Settings")] public bool grounded = true;
        public float jumpForce = 5f;

        [Header("Dodge Settings")] public float dodgeDistance = 5f;
        public float dodgeDuration = 0.35f;
        private bool _canDodge = true;
        private bool _isDodging;
        public bool disableMovement;
        public float staggerDuration = 0.5f;

        [Header("Sprint Settings")] [SerializeField]
        private float sprintMultiplier = 2f;

        private bool _isSprinting;
        private PlayerRespawner _respawner;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip staggerSound;
        [SerializeField] private AudioClip jumpSound;


        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _speedHash = Animator.StringToHash("Speed");
            _animator.SetFloat(_speedHash, 0f);
            _animator.SetBool(Grounded, true);
            _respawner = GetComponent<PlayerRespawner>();
            _pistol = GetComponent<PistolProfiler>();

            _actions = new InputControls();
            _mainCamera = Camera.main;
            grounded = true;

            LoadHealth();

            currentHealth = PlayerPrefs.HasKey("PlayerHealth") ? PlayerPrefs.GetInt("PlayerHealth") : 100;

            SaveManager.LoadGame(this, _pistol);
        }

        public void SetCurrentHealth(int health)
        {
            currentHealth = health;
        }

        private void LoadHealth()
        {
            currentHealth = PlayerPrefs.HasKey("PlayerHealth") ? PlayerPrefs.GetInt("PlayerHealth") : 100;
        }

        public int GetCurrentHealth()
        {
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            PlayerPrefs.Save();

            return currentHealth;
        }

        public void RestoreHealth(int itemValue)
        {
            currentHealth = Mathf.Clamp(currentHealth + itemValue, 0, maxHealth);
        }

        private void OnEnable()
        {
            if (FindObjectOfType<PauseMenu>().isPaused) return; // ✅ Don't re-enable input if paused
            _actions.Profiler.Enable();

            _actions.Profiler.Enable();
            _moveKeys = _actions.Profiler.Movement;

            _actions.Profiler.Jump.performed += JumpAction;
            _actions.Profiler.Dodge.performed += DodgeAction;
            _actions.Profiler.Sprint.performed += StartSprinting;
            _actions.Profiler.Sprint.canceled += StopSprinting;
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Dodge.performed -= DodgeAction;
            _actions.Profiler.Sprint.performed -= StartSprinting;
            _actions.Profiler.Sprint.canceled -= StopSprinting;
            _actions.Profiler.Disable();
        }

        private void OnCollisionEnter()
        {
            grounded = true;
            _animator.SetBool(Grounded, true);
        }

        private void FixedUpdate()
        {
            if (_isDodging || disableMovement) return;

            if (grounded)
            {
                float speedFactor = _isSprinting ? sprintMultiplier : 1f;
                _animator.SetFloat(Speed, (_rigidbody.velocity.magnitude / MaxSpeed) * speedFactor);
            }

            _forceDirection = Vector3.zero;
            Vector2 input = _moveKeys.ReadValue<Vector2>();

            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            _forceDirection += GetCameraDirection(cameraRight, input.x);
            _forceDirection += GetCameraDirection(cameraForward, input.y);

            float speedMultiplier = _isSprinting ? sprintMultiplier : 1f;
            _rigidbody.AddForce(_forceDirection * (movementForce * speedMultiplier), ForceMode.Impulse);

            RotateCharacter(_moveKeys.ReadValue<Vector2>());
        }

        private void RotateCharacter(Vector2 input)
        {
            var direction = _rigidbody.velocity;
            direction.y = 0f;

            if (input.sqrMagnitude > 0.5f && direction.sqrMagnitude > 0.5f)
            {
                _rigidbody.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private static Vector3 GetCameraDirection(Vector3 direction, float inputAxis)
        {
            direction.y = 0;
            return direction.normalized * inputAxis;
        }

        private void JumpAction(InputAction.CallbackContext context)
        {
            if (!grounded && disableMovement) return;

            grounded = false;
            _animator.SetBool(Grounded, false);
            _animator.SetTrigger(Jump);
            Invoke(nameof(DelayedJump), 0.2f);
        }

        private void DelayedJump()
        {
            if (jumpSound is not null && audioSource is not null)
                audioSource.PlayOneShot(jumpSound);

            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void DodgeAction(InputAction.CallbackContext context)
        {
            if (!_canDodge || _isDodging) return;

            _isDodging = true;
            _canDodge = false;
            disableMovement = true;
            _animator.SetTrigger(Dodge);

            var input = _moveKeys.ReadValue<Vector2>();
            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            var dodgeDirection = GetCameraDirection(cameraRight, input.x) + GetCameraDirection(cameraForward, input.y);

            if (dodgeDirection == Vector3.zero) dodgeDirection = -transform.forward;

            _rigidbody.velocity = dodgeDirection.normalized * dodgeDistance;

            Invoke(nameof(EndDodge), dodgeDuration);
        }

        private void EndDodge()
        {
            _isDodging = false;
            disableMovement = false;
            _canDodge = true;
            _animator.ResetTrigger(Dodge);
        }

        private void StartSprinting(InputAction.CallbackContext context)
        {
            _isSprinting = true;
        }

        private void StopSprinting(InputAction.CallbackContext context)
        {
            _isSprinting = false;
        }

        public void TakeDamage(int damage)
        {
            Debug.Log($"🔥 Kanta took {damage} damage!");
            currentHealth -= damage;

            StartCoroutine(StaggerEffect());

            if (currentHealth <= 0) _respawner.InitRespawn();
        }

        private IEnumerator StaggerEffect()
        {
            if (staggerSound is not null && audioSource is not null)
                audioSource.PlayOneShot(staggerSound);

            var originalMovementForce = movementForce;
            movementForce = 0.5f;

            var knockback = -transform.forward * 20f;
            _rigidbody.AddForce(knockback, ForceMode.Impulse);

            yield return new WaitForSeconds(staggerDuration);

            movementForce = originalMovementForce;
        }

        public void SetActions(bool active)
        {
            if (active)
            {
                _rigidbody.constraints = RigidbodyConstraints.None;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }
}