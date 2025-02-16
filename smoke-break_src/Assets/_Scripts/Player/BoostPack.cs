using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.Player
{
    public class BoostPack : MonoBehaviour
    {
        private static readonly int Boost = Animator.StringToHash("Boost");

        [Header("Boost Settings")] [SerializeField]
        private float boostForce = 15f;

        [SerializeField] private float boostCooldown = 1f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float doubleJumpTimeLimit = 0.3f;
        private bool _canBoost = true;
        private bool _jumpPressedOnce = false;
        private float _lastJumpTime;

        [Header("Visual Effects")] [SerializeField]
        private ParticleSystem boostEffect;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip boostSound;
        [SerializeField] private float boostVolume = 0.7f;

        [Header("Dependencies")] private Rigidbody _rigidbody;
        private PlayerProfiler _player;
        private InputControls _actions;
        private Animator _animator;
        private InputAction _moveInput;
        private Camera _mainCamera;

        [Header("UI Settings")] [SerializeField]
        private Slider boostBar;

        private void Awake()
        {
            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _moveInput = _actions.Profiler.Movement;
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
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
                _canBoost = true;
            }

            if (!_player.grounded && _rigidbody.velocity.y < 0)
            {
                _rigidbody.velocity += Vector3.down * (fallMultiplier * Time.deltaTime);
            }
        }

        private void JumpAction(InputAction.CallbackContext context)
        {
            if (_player.grounded)
            {
                _jumpPressedOnce = true;
                _lastJumpTime = Time.time;
                return;
            }

            if (_jumpPressedOnce && Time.time - _lastJumpTime <= doubleJumpTimeLimit && _canBoost)
            {
                PerformBoost();
                _jumpPressedOnce = false;
            }
        }

        private void PerformBoost()
        {
            _canBoost = false;
            _animator.SetTrigger(Boost);

            Vector2 input = _moveInput.ReadValue<Vector2>();
            Vector3 boostDirection = GetBoostDirection(input);

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(boostDirection.normalized * boostForce, ForceMode.Impulse);

            if (boostEffect != null)
                boostEffect.Play();

            if (audioSource != null && boostSound != null)
                audioSource.PlayOneShot(boostSound, boostVolume);

            if (boostBar != null)
                StartCoroutine(UpdateBoostBar());

            Invoke(nameof(ApplyFastFall), 0.3f);
            Invoke(nameof(DisableEffects), 0.3f);
        }

        private Vector3 GetBoostDirection(Vector2 input)
        {
            Vector3 forward = _mainCamera.transform.forward;
            Vector3 right = _mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            Vector3 movementDirection = (right * input.x + forward * input.y).normalized;

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

        private IEnumerator UpdateBoostBar()
        {
            if (HUDManager.Instance is null) yield break;

            HUDManager.Instance.FadeBoostUI(true);

            for (float elapsedTime = 0; elapsedTime < boostCooldown; elapsedTime += Time.deltaTime)
            {
                var fillAmount = Mathf.Clamp01(1f - (elapsedTime / boostCooldown));
                HUDManager.Instance.UpdateBoostBar(fillAmount);

                if (fillAmount <= 0.15f)
                {
                    HUDManager.Instance.StartFlashing();
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            _canBoost = true;

            for (float elapsedTime = 0; elapsedTime < boostCooldown; elapsedTime += Time.deltaTime)
            {
                HUDManager.Instance.UpdateBoostBar(Mathf.Clamp01(elapsedTime / boostCooldown));
                yield return null;
            }

            HUDManager.Instance.FadeBoostUI(false);
        }
    }
}