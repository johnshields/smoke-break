using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class PlayerProfiler : MonoBehaviour
    {
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Dodge = Animator.StringToHash("Dodge");

        [Header("References")] 
        private Camera _mainCamera;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private InputControls _actions;
        private InputAction _moveKeys;

        [Header("Health Settings")] 
        [SerializeField] private int maxHealth = 100;

        private int _currentHealth;

        [Header("Movement Settings")] 
        public float movementForce = 1f;
        private Vector3 _forceDirection = Vector3.zero;
        private const float MaxSpeed = 5f;

        [Header("Jump Settings")] 
        public bool grounded = true;
        public float jumpForce = 5f;

        [Header("Dodge Settings")] 
        public float dodgeDistance = 5f;
        public float dodgeDuration = 0.35f;
        private bool _canDodge = true;
        private bool _isDodging;
        public bool disableMovement;

        [Header("Sprint Settings")] 
        [SerializeField] private float sprintMultiplier = 2f;
        private bool _isSprinting;
        private InputAction _sprintAction;


        [Header("Animation Parameters")] 
        private int _speedHash;
        private int _groundedHash;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _speedHash = Animator.StringToHash("Speed");
            _animator.SetFloat(_speedHash, 0f);
            _animator.SetBool(Grounded, true);

            _currentHealth = maxHealth;
            _actions = new InputControls();
            _mainCamera = Camera.main;
            grounded = true;
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _moveKeys = _actions.Profiler.Movement;
            _sprintAction = _actions.Profiler.Sprint;

            _actions.Profiler.Jump.performed += JumpAction;
            _actions.Profiler.Dodge.performed += DodgeAction;
            _sprintAction.performed += StartSprinting;
            _sprintAction.canceled += StopSprinting;
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Dodge.performed -= DodgeAction;
            _sprintAction.performed -= StartSprinting;
            _sprintAction.canceled -= StopSprinting;
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
            if (!grounded) return;

            grounded = false;

            _animator.SetBool(Grounded, false);
            _animator.SetTrigger(Jump);
            Invoke(nameof(DelayedJump), 0.2f);
        }

        private void DelayedJump()
        {
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
            _currentHealth -= damage;

            if (_currentHealth <= 0)
            {
                Debug.Log("💀 Kanta has died!");
                Die();
            }
        }


        private void Die()
        {
            Debug.Log("Player has died!");
            // Add respawn or game over logic here
        }

        public float GetCurrentHealth()
        {
            return _currentHealth;
        }
    }
}