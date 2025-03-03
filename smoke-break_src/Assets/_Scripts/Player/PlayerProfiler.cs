using System.Collections;
using System.IO;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.UI;

namespace _Scripts.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(PlayerHealth))]
    public class PlayerProfiler : MonoBehaviour
    {
        #region Variables

        private string _savePath;

        [Header("Animation Parameters")] private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int DodgeBack = Animator.StringToHash("DodgeBack");
        private static readonly int DodgeRoll = Animator.StringToHash("DodgeRoll");
        private static readonly int Injured = Animator.StringToHash("Injured");
        private static readonly int Stagger = Animator.StringToHash("Stagger");

        [Header("References")] private Rigidbody _rigidbody;
        private Animator _animator;
        private Camera _mainCamera;
        private InputControls _actions;
        private InputAction _moveKeys;
        private PlayerHealth _playerHealth;
        private PauseMenu _pauseMenu;

        [Header("Movement Settings")] private bool disableMovement;
        [SerializeField] private float movementForce = 1f;
        private Vector3 _forceDirection = Vector3.zero;
        private const float MaxSpeed = 5f;

        [Header("Jump Settings")] public bool grounded = true;
        [SerializeField] private float jumpForce = 5f;
        private bool _canJump;

        [Header("DodgeBack Settings")] [SerializeField]
        private float dodgeDistance = 20f;

        [SerializeField] private float dodgeDuration = 0.8f;
        [SerializeField] private float staggerDuration = 0.5f;
        private bool _canDodge = true;
        private bool _isDodging;

        [Header("Sprint Settings")] [SerializeField]
        private float maxStamina = 100f;

        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float sprintMultiplier = 2f;
        private bool _isSprinting;
        private bool _isExhausted;

        [Header("Gravity Settings")] [SerializeField]
        private float gravityForce = -9.81f;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.2f;
        private bool _groundedGravity;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioSource heartbeatAudio;
        [SerializeField] private AudioClip heartbeatSound;
        [SerializeField] private AudioClip staggerSound;

        public int injuryThreshold = 30;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _playerHealth = GetComponent<PlayerHealth>();
            _actions = new InputControls();
            _mainCamera = Camera.main;

            if (_pauseMenu != null)
                _pauseMenu = FindObjectOfType<PauseMenu>();

            currentStamina = maxStamina;
            _moveKeys = _actions.Profiler.Movement;

            LoadPlayerPosition();
        }

        private void OnEnable()
        {
            if (_pauseMenu is not null && _pauseMenu.isPaused) return;

            _actions.Profiler.Enable();
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

        private void Update()
        {
            DrainStamina();
            UpdateInjuryState();
        }

        private void FixedUpdate()
        {
            if (_isDodging || disableMovement) return;

            var input = _moveKeys.ReadValue<Vector2>();
            MoveCharacter(input);
            RotateCharacter(input);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                grounded = true;
                _animator.SetBool(Grounded, true);
                Invoke(nameof(EnableJump), .2f);
            }
        }

        #endregion

        #region Getters and Setters

        public float GetMaxStamina()
        {
            return maxStamina;
        }

        public float GetCurrentStamina()
        {
            return currentStamina;
        }

        public void SetMovement(bool value)
        {
            disableMovement = value;
        }

        #endregion

        #region Position

        private void LoadPlayerPosition()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            }
        }

        #endregion

        #region Movement

        private void MoveCharacter(Vector2 input)
        {
            // Get camera directions for movement
            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            // Reset movement force direction
            _forceDirection = Vector3.zero;
            _forceDirection += GetCameraDirection(cameraRight, input.x); // Apply right movement
            _forceDirection += GetCameraDirection(cameraForward, input.y); // Apply forward movement

            // Determine movement speed based on sprinting state
            var speedMultiplier = _isSprinting ? sprintMultiplier : 1f;

            // Apply movement force to rigidbody
            _rigidbody.AddForce(_forceDirection * (movementForce * speedMultiplier), ForceMode.Impulse);

            // Check if character is grounded using a downward raycast
            _groundedGravity =
                Physics.Raycast(transform.position, Vector3.down, out _, groundCheckRadius, groundLayer);

            // Apply gravity if the character is not grounded
            if (!_groundedGravity)
            {
                _rigidbody.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
            }

            // If the character is not grounded, exit function
            if (!grounded || !_groundedGravity) return;

            // Adjust animation speed based on movement speed
            var speedFactor = _isSprinting ? sprintMultiplier : 1f;
            _animator.SetFloat(Speed, (_rigidbody.velocity.magnitude / MaxSpeed) * speedFactor);
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

        #endregion

        #region Jumping & Dodging

        private void EnableJump()
        {
            _canJump = true;
        }

        private void JumpAction(InputAction.CallbackContext context)
        {
            if (!grounded || disableMovement || !_canJump) return;

            _canJump = false;
            grounded = false;
            _animator.SetBool(Grounded, false);
            _animator.SetTrigger(Jump);
            Invoke(nameof(DelayedJump), .2f);
        }

        private void DelayedJump()
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void DodgeAction(InputAction.CallbackContext context)
        {
            if (!TryDodge()) return;

            var input = _moveKeys.ReadValue<Vector2>();
            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            var dodgeDirection = GetCameraDirection(cameraRight, input.x) + GetCameraDirection(cameraForward, input.y);

            var isMoving = input.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                _animator.SetTrigger(DodgeRoll);
            }
            else
            {
                _animator.SetTrigger(DodgeBack);
                dodgeDirection = -transform.forward;
            }

            StartCoroutine(SmoothDodge(dodgeDirection.normalized, isMoving));
        }

        private bool TryDodge()
        {
            if (!_canDodge || _isDodging || currentStamina < 10 || !grounded) return false;

            currentStamina -= 10;
            _playerHealth.SetInvulnerable(true);
            _isDodging = true;
            _canDodge = false;
            disableMovement = true;

            return true;
        }

        private IEnumerator SmoothDodge(Vector3 dodgeDirection, bool isRolling)
        {
            _isDodging = true;
            _canDodge = false;
            disableMovement = true;

            var rollMultiplier = isRolling ? 1.5f : 1f; // Rolls travel further
            var elapsedTime = 0f;
            var startPosition = transform.position;
            var targetPosition = startPosition + (dodgeDirection.normalized * (dodgeDistance * rollMultiplier));

            while (elapsedTime < dodgeDuration)
            {
                var t = elapsedTime / dodgeDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                var newPosition = Vector3.Lerp(startPosition, targetPosition, t);
                _rigidbody.MovePosition(newPosition);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _rigidbody.MovePosition(targetPosition);
            _playerHealth.SetInvulnerable(false);
            _isDodging = false;
            disableMovement = false;

            yield return new WaitForSeconds(0.2f);
            _canDodge = true;
        }

        #endregion

        #region Sprinting & Stamina

        private void StartSprinting(InputAction.CallbackContext context)
        {
            if (_isExhausted) return;

            sprintMultiplier = _playerHealth.GetCurrentHealth() < 50 ? 1.5f : 2f;
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

        #region Combat & Effects

        public IEnumerator StaggerEffect()
        {
            _animator.SetTrigger(Stagger);

            if (staggerSound is not null && audioSource is not null)
                audioSource.PlayOneShot(staggerSound);

            var originalMovementForce = movementForce;
            movementForce = 0.5f;

            var knockback = -transform.forward * 20f;
            _rigidbody.velocity = knockback;

            yield return new WaitForSeconds(staggerDuration);

            movementForce = originalMovementForce;
        }

        private void UpdateInjuryState()
        {
            var isLowHealth = _playerHealth.GetCurrentHealth() <= injuryThreshold;
            var isStandingStill = _moveKeys.ReadValue<Vector2>().sqrMagnitude < 0.01f;

            var shouldBeInjured = isLowHealth && isStandingStill;
            _animator.SetBool(Injured, shouldBeInjured);

            HandleHeartbeat(isLowHealth);
        }

        private void HandleHeartbeat(bool isLowHealth)
        {
            var health = _playerHealth.GetCurrentHealth();

            if (health <= 0)
            {
                if (heartbeatAudio.isPlaying) heartbeatAudio.Stop();
                return;
            }

            if (isLowHealth)
            {
                if (!heartbeatAudio.isPlaying)
                {
                    heartbeatAudio.clip = heartbeatSound;
                    heartbeatAudio.Play();
                }

                var healthRatio = health / (float)injuryThreshold;
                heartbeatAudio.volume = Mathf.Lerp(1f, 0.2f, healthRatio);
                heartbeatAudio.pitch = Mathf.Lerp(1.5f, 0.6f, healthRatio);
            }
            else
            {
                if (heartbeatAudio.isPlaying) heartbeatAudio.Stop();
            }
        }

        #endregion

        #region Misc

        public void SetActions(bool active)
        {
            _rigidbody.constraints = active
                ? RigidbodyConstraints.FreezeRotation
                : RigidbodyConstraints.FreezeAll;
        }

        #endregion
    }
}