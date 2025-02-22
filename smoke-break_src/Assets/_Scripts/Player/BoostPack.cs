using System.Collections;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts.Player
{
    public class BoostPack : MonoBehaviour
    {
        private static readonly int Boost = Animator.StringToHash("Boost");

        [Header("Boost Settings")] [SerializeField]
        private float boostForce = 15f;

        [SerializeField] public float boostCooldown = 1f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float doubleJumpTimeLimit = 0.3f;
        public bool canBoost = true;
        private bool _jumpPressedOnce;
        private float _lastJumpTime;

        [Header("Visual Effects")] [SerializeField]
        private ParticleSystem boostEffect;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip boostSound;
        [SerializeField] private float vol = 0.2f;

        [Header("Dependencies")] private Rigidbody _rigidbody;
        private PlayerProfiler _player;
        private InputControls _actions;
        private Animator _animator;
        private InputAction _moveInput;
        private Camera _mainCamera;
        private HUDManager _hud;


        private void Awake()
        {
            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _moveInput = _actions.Profiler.Movement;
            _mainCamera = Camera.main;
            _hud = FindObjectOfType<HUDManager>();
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Jump.Enable();
            _actions.Profiler.Jump.performed += JumpAction;
        }

        private void OnDisable()
        {
            _actions.Profiler.Jump.performed -= JumpAction;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            if (_player.grounded)
            {
                _jumpPressedOnce = false;
                canBoost = true;
            }

            if (!_player.grounded && _rigidbody.velocity.y < 0)
            {
                _rigidbody.velocity += Vector3.down * (fallMultiplier * Time.deltaTime);
            }
        }

        private void JumpAction(InputAction.CallbackContext context)
        {
            if (_player.grounded || !_jumpPressedOnce)
            {
                _jumpPressedOnce = true;
                _lastJumpTime = Time.time;
                return;
            }

            if (_jumpPressedOnce && Time.time - _lastJumpTime <= doubleJumpTimeLimit && canBoost)
            {
                Debug.Log("🚀 Performing Boost!");
                PerformBoost();
                _jumpPressedOnce = false;
            }
        }

        private void PerformBoost()
        {
            canBoost = false;
            _animator.SetTrigger(Boost);

            var input = _moveInput.ReadValue<Vector2>();
            var boostDirection = GetBoostDirection(input);

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(boostDirection.normalized * boostForce, ForceMode.Impulse);

            if (boostEffect != null)
                boostEffect.Play();

            if (audioSource != null && boostSound != null)
                audioSource.PlayOneShot(boostSound, vol);

            if (_hud is null) return;
            StartCoroutine(_hud.UpdateBoostBar());

            Invoke(nameof(ApplyFastFall), 0.3f);
            Invoke(nameof(DisableEffects), 0.3f);
        }

        private Vector3 GetBoostDirection(Vector2 input)
        {
            var forward = _mainCamera.transform.forward;
            var right = _mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            var movementDirection = (right * input.x + forward * input.y).normalized;

            return movementDirection == Vector3.zero ? transform.forward + Vector3.up : movementDirection + Vector3.up;
        }

        private void ApplyFastFall()
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -fallMultiplier, _rigidbody.velocity.z);
        }

        private void DisableEffects()
        {
            if (boostEffect != null)
                boostEffect.Stop();
        }
    }
}