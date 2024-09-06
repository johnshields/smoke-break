using UnityEngine;
using UnityEngine.InputSystem;

namespace Development._Scripts
{
    public class KantaAnimations : MonoBehaviour
    {
        private Animator _animator;
        private Rigidbody _rigidbody;
        private int _speed, _jump;
        private InputControls _controls;
        public bool grounded;
        private const float _maxSpeed = 5f;

        private void Awake()
        {
            _controls = new InputControls();
            _controls.Profiler.Jump.Disable();
            _speed = Animator.StringToHash("Speed");
            _jump = Animator.StringToHash("Jump");
            grounded = true;
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator.SetFloat(_speed, 0f);
            _animator.SetLayerWeight(1, 0f);
        }
        
        private void OnEnable()
        {
            _controls.Profiler.Enable();
            _controls.Profiler.Jump.performed += JumpAction;
        }

        private void OnDisable()
        {
            _controls.Profiler.Jump.performed -= JumpAction;
            _controls.Profiler.Disable();
        }
        
        private void OnCollisionEnter()
        {
            grounded = true;
        }
    
        private void FixedUpdate()
        {
            if (grounded)
            {
                // Update the "Speed" parameter in the Animator based on the character's current velocity
                // The velocity magnitude is normalized by dividing by the maximum speed from KantaProfiler
                _animator.SetFloat(_speed, _rigidbody.velocity.magnitude / _maxSpeed);
            }
        }
        
        private void JumpAction(InputAction.CallbackContext obj)
        {
            if (!grounded) return;
            // Prepare the jump animation
            _animator.SetFloat(_speed, 0f);
            _animator.SetLayerWeight(1, 1f);  // Enable jump animation layer
            _animator.SetTrigger(_jump);
            Invoke(nameof(ResetLayers), 1.1f); 
            grounded = false;
        }

        private void ResetLayers()
        {
            _animator.SetLayerWeight(1, 0);
            grounded = true;
        }
    }
}
