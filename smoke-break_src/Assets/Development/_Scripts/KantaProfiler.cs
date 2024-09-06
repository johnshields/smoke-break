using UnityEngine;
using UnityEngine.InputSystem;

namespace Development._Scripts
{
    public class KantaProfiler : MonoBehaviour
    {
        private Camera _mainCamera;
        private Rigidbody _rigidbody;
        private InputControls _controls;
        private Vector3 _forceDirection = Vector3.zero;
        private InputAction _moveKeys;
        public float movementForce = 1f;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
            _controls = new InputControls();
        }
        
        private void OnEnable()
        {
            _controls.Profiler.Enable();
            _moveKeys = _controls.Profiler.Movement;
        }

        private void OnDisable()
        {
            _controls.Profiler.Disable();
        }

        private void FixedUpdate()
        {
            _forceDirection = Vector3.zero;
            
            var input = _moveKeys.ReadValue<Vector2>();

            // Calculate force direction based on camera's orientation and movement input.
            _forceDirection += GetCameraRight(_mainCamera) * (input.x * movementForce);
            _forceDirection += GetCameraForward(_mainCamera) * (input.y * movementForce);

            _rigidbody.AddForce(_forceDirection, ForceMode.Impulse);

            LookAt(input);
        }

        private void LookAt(Vector2 input)
        {
            var direction = _rigidbody.velocity;
            direction.y = 0f;

            // Check if movement is significant enough to rotate the character
            if (input.sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
                _rigidbody.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private Vector3 GetCameraForward(Camera cam)
        {
            var forward = cam.transform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        private Vector3 GetCameraRight(Camera cam)
        {
            var right = cam.transform.right;
            right.y = 0;
            return right.normalized;
        }
    }
}
