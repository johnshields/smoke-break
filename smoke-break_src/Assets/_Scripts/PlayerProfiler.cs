using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _Scripts
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class PlayerProfiler : MonoBehaviour
    {
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Dodge = Animator.StringToHash("Dodge");

        [Header("References")] private Camera _mainCamera;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private InputControls _actions;
        private InputAction _moveKeys;

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

        [Header("Animation Parameters")] private int _speedHash;
        private int _groundedHash;

        private void Awake()
        {
            // Cache components
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();

            // Initialize input system
            _actions = new InputControls();

            // Cache animator parameters
            _speedHash = Animator.StringToHash("Speed");

            // Set initial animation states
            _animator.SetFloat(_speedHash, 0f);

            // Get main camera reference
            _mainCamera = Camera.main;

            _animator.SetBool(Grounded, true);

            grounded = true;
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _moveKeys = _actions.Profiler.Movement;
            _actions.Profiler.Jump.performed += JumpAction;
            _actions.Profiler.Dodge.performed += DodgeAction;
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Dodge.performed -= DodgeAction;
            _actions.Profiler.Disable();
        }

        private void OnCollisionEnter()
        {
            grounded = true;
            _animator.SetBool(Grounded, true); // Smoothly transitions back to movement
        }

        private void FixedUpdate()
        {
            if (_isDodging || disableMovement) return;

            if (grounded)
            {
                _animator.SetFloat(Speed, _rigidbody.velocity.magnitude / MaxSpeed);
            }

            _forceDirection = Vector3.zero;
            Vector2 input = _moveKeys.ReadValue<Vector2>();

            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            // Apply corrected movement direction
            _forceDirection += GetCameraDirection(cameraRight, input.x);
            _forceDirection += GetCameraDirection(cameraForward, input.y);
            _rigidbody.AddForce(_forceDirection * movementForce, ForceMode.Impulse);

            RotateCharacter(_moveKeys.ReadValue<Vector2>());
        }

        private void RotateCharacter(Vector2 input)
        {
            var direction = _rigidbody.velocity;
            direction.y = 0f;

            // Rotate only if movement input is significant
            if (input.sqrMagnitude > 0.5f && direction.sqrMagnitude > 0.5f)
            {
                _rigidbody.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private static Vector3 GetCameraDirection(Vector3 direction, float inputAxis)
        {
            direction.y = 0;  // Ensure movement is only horizontal
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
    
            if (dodgeDirection == Vector3.zero) dodgeDirection = -transform.forward; // Default to backward

            _rigidbody.velocity = dodgeDirection.normalized * dodgeDistance; // Move instantly

            Invoke(nameof(EndDodge), dodgeDuration); // Ends dodge after duration
        }

        private void EndDodge()
        {
            _isDodging = false;
            disableMovement = false;
            _canDodge = true;
            _animator.ResetTrigger(Dodge);
        }
    }
}