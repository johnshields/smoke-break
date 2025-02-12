using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class KantaProfiler : MonoBehaviour
    {
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");

        [Header("References")]
        private Camera _mainCamera;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private InputControls _actions;
        private InputAction _moveKeys;

        [Header("Movement Settings")]
        public float movementForce = 1f;
        private Vector3 _forceDirection = Vector3.zero;
        private const float MaxSpeed = 5f;

        [Header("Jump Settings")]
        public bool grounded = true;
        public float jumpForce = 5f;
        
        [Header("Animation Parameters")]
        private int _speedHash;
        private int _jumpBlendHash;
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
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Disable();
        }

        private void OnCollisionEnter()
        {
            grounded = true;
            _animator.SetBool(Grounded, true); // Smoothly transitions back to movement
        }

        private void FixedUpdate()
        {
            if (grounded)
            {
                _animator.SetFloat(Speed, _rigidbody.velocity.magnitude / MaxSpeed);
            }

            _forceDirection = Vector3.zero;
            var input = _moveKeys.ReadValue<Vector2>();

            // Correctly access camera direction
            var cameraRight = _mainCamera.transform.right;
            var cameraForward = _mainCamera.transform.forward;

            // Apply corrected movement direction
            _forceDirection += GetCameraDirection(cameraRight, input.x);
            _forceDirection += GetCameraDirection(cameraForward, input.y);

            if (grounded) 
            {
                _rigidbody.AddForce(_forceDirection * movementForce, ForceMode.Impulse);
            }
            else 
            {
                _rigidbody.AddForce(_forceDirection * (movementForce * 0.2f), ForceMode.Impulse);
            }

            RotateCharacter(input);
        }

        private void RotateCharacter(Vector2 input)
        {
            Vector3 direction = _rigidbody.velocity;
            direction.y = 0f;

            // Rotate only if movement input is significant
            if (input.sqrMagnitude > 0.5f && direction.sqrMagnitude > 0.5f)
            {
                _rigidbody.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private Vector3 GetCameraDirection(Vector3 direction, float inputAxis)
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
    }
}
