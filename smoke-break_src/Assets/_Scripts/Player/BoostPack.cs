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
        private bool _jumpPressedOnce;
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

        [Header("Boost UI Elements")] [SerializeField]
        private Slider boostBar;

        [SerializeField] private Image boostFill;
        [SerializeField] private Color flashColor = Color.red;
        private Color _originalColor;

        private void Awake()
        {
            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _moveInput = _actions.Profiler.Movement;
            _mainCamera = Camera.main;

            if (boostFill is not null)
                _originalColor = boostFill.color;
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
                _canBoost = true;
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

            if (_jumpPressedOnce && Time.time - _lastJumpTime <= doubleJumpTimeLimit && _canBoost)
            {
                Debug.Log("🚀 Performing Boost!");
                PerformBoost();
                _jumpPressedOnce = false;
            }
        }

        private void PerformBoost()
        {
            _canBoost = false;
            _animator.SetTrigger(Boost);

            var input = _moveInput.ReadValue<Vector2>();
            var boostDirection = GetBoostDirection(input);

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

        private IEnumerator UpdateBoostBar()
        {
            if (boostBar is null || boostFill is null) yield break;

            var originalColor = boostFill.color;
            bool isFlashing = false;

            StartCoroutine(FadeBoostBar(1f));

            for (float elapsedTime = 0; elapsedTime < boostCooldown; elapsedTime += Time.deltaTime)
            {
                var fillAmount = Mathf.Clamp01(1f - (elapsedTime / boostCooldown));
                boostBar.value = fillAmount;

                if (fillAmount <= 0.15f && !isFlashing)
                {
                    isFlashing = true;
                    StartCoroutine(FlashBoostBar());
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            boostFill.color = originalColor;
            _canBoost = true;

            for (float elapsedTime = 0; elapsedTime < boostCooldown; elapsedTime += Time.deltaTime)
            {
                boostBar.value = Mathf.Clamp01(elapsedTime / boostCooldown);
                yield return null;
            }

            boostBar.value = 1f;

            StartCoroutine(FadeBoostBar(0f));
        }

        private IEnumerator FlashBoostBar()
        {
            while (boostBar.value <= 0.15f)
            {
                boostFill.color = (boostFill.color == flashColor) ? _originalColor : flashColor;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.1f);
            boostFill.color = _originalColor;
            _canBoost = true;
        }

        private IEnumerator FadeBoostBar(float targetAlpha)
        {
            var startAlpha = boostFill.color.a;
            const float fadeDuration = 0.5f;
            var elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                var newColor = boostFill.color;
                newColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                boostFill.color = newColor;
                yield return null;
            }
        }
    }
}