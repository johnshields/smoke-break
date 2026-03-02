using System.Collections;
using System.IO;
using _Scripts._Systems.Managers;
using _Scripts._Systems.Objects;
using _Scripts._Systems.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts._Gameplay._Player
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(PlayerHealth))]
    public class PlayerMovement : MonoBehaviour
    {
        #region Variables

        private string _savePath;

        private static readonly int
            GroundedAnim = Animator.StringToHash("Grounded"),
            SpeedAnim = Animator.StringToHash("Speed"),
            JumpAnim = Animator.StringToHash("Jump"),
            DodgeBackAnim = Animator.StringToHash("DodgeBack"),
            DodgeRollAnim = Animator.StringToHash("DodgeRoll");

        private Rigidbody _rigidbody;
        private Animator _animator;
        private Camera _mainCamera;
        private PlayerHealth _playerHealth;
        private PlayerStamina _playerStamina;
        private PauseMenu _pauseMenu;
        private InputControls _actions;
        private InputAction _moveKeys;

        [Header("Movement Settings")] [SerializeField]
        private float movementForce = 1f;

        private bool _disableMovement;
        private Vector3 _forceDirection = Vector3.zero;

        private const float
            MaxSpeed = 5f,
            DodgeStaminaCost = 10f,
            DodgeRollMultiplier = 1.5f,
            DodgeCooldown = 0.2f;

        [Header("Jump Settings")] [SerializeField]
        private float jumpForce = 30f;

        private bool _canJump;
        public bool grounded;

        [Header("Dodge Settings")] [SerializeField]
        private float dodgeDistance = 5f;

        [SerializeField] private float dodgeDuration = 0.8f;
        private bool _canDodge = true;
        private bool _isDodging;

        [Header("Gravity Settings")] [SerializeField]
        private float gravityForce = 50f;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.5f;
        private bool _groundedGravity;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _playerHealth = GetComponent<PlayerHealth>();
            _playerStamina = GetComponent<PlayerStamina>();
            _actions = new InputControls();
            _mainCamera = Camera.main;

            _pauseMenu = FindFirstObjectByType<PauseMenu>();
            _moveKeys = _actions.Profiler.Movement;

            grounded = true;
            _animator.SetBool(GroundedAnim, true);

            LoadPlayerPosition();
        }

        private void OnEnable()
        {
            if (_pauseMenu is not null && _pauseMenu.isPaused) return;

            _actions.Profiler.Enable();
            _actions.Profiler.Jump.performed += JumpAction;
            _actions.Profiler.Dodge.performed += DodgeAction;
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Dodge.performed -= DodgeAction;
            _actions.Profiler.Disable();
        }

        private void FixedUpdate()
        {
            if (_isDodging || _disableMovement) return;

            var input = _moveKeys.ReadValue<Vector2>();
            MoveCharacter(input);
            RotateCharacter(input);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                grounded = true;
                _animator.SetBool(GroundedAnim, true);
                Invoke(nameof(EnableJump), .2f);
            }
        }

        #endregion

        #region Public API

        public void SetMovement(bool value)
        {
            _disableMovement = value;
        }

        public void SetActions(bool active)
        {
            _rigidbody.constraints = active
                ? RigidbodyConstraints.FreezeRotation
                : RigidbodyConstraints.FreezeAll;
        }

        public float GetMovementForce() => movementForce;

        public void SetMovementForce(float value)
        {
            movementForce = value;
        }

        public Vector2 GetMoveInput() => _moveKeys.ReadValue<Vector2>();

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
            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            _forceDirection = Vector3.zero;
            _forceDirection += GetCameraDirection(cameraRight, input.x);
            _forceDirection += GetCameraDirection(cameraForward, input.y);

            var speedMultiplier = _playerStamina.SprintMultiplier;
            _rigidbody.AddForce(_forceDirection * (movementForce * speedMultiplier), ForceMode.Impulse);

            _groundedGravity =
                Physics.Raycast(transform.position, Vector3.down, out _, groundCheckRadius, groundLayer);

            if (!_groundedGravity)
            {
                _rigidbody.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
            }

            if (!grounded || !_groundedGravity) return;

            _animator.SetFloat(SpeedAnim, (_rigidbody.linearVelocity.magnitude / MaxSpeed) * speedMultiplier);
        }

        private void RotateCharacter(Vector2 input)
        {
            var direction = _rigidbody.linearVelocity;
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
            if (!grounded || _disableMovement || !_canJump) return;

            _canJump = false;
            grounded = false;
            _animator.SetBool(GroundedAnim, false);
            _animator.SetTrigger(JumpAnim);
            Invoke(nameof(DelayedJump), .2f);
        }

        private void DelayedJump()
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
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
                _animator.SetTrigger(DodgeRollAnim);
            }
            else
            {
                _animator.SetTrigger(DodgeBackAnim);
                dodgeDirection = -transform.forward;
            }

            StartCoroutine(SmoothDodge(dodgeDirection.normalized, isMoving));
        }

        private bool TryDodge()
        {
            if (!_canDodge || _isDodging || !grounded) return false;
            if (!_playerStamina.TryConsumeStamina(DodgeStaminaCost)) return false;

            _playerHealth.SetInvulnerable(true);
            _isDodging = true;
            _canDodge = false;
            _disableMovement = true;

            return true;
        }

        private IEnumerator SmoothDodge(Vector3 dodgeDirection, bool isRolling)
        {
            var rollMultiplier = isRolling ? DodgeRollMultiplier : 1f;
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
            _disableMovement = false;

            yield return new WaitForSeconds(DodgeCooldown);
            _canDodge = true;
        }

        #endregion
    }
}
