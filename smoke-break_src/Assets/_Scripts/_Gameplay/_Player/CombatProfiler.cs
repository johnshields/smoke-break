using System.Collections;
using _Scripts._Gameplay.AI;
using _Scripts._Systems.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts._Gameplay._Player
{
    public class CombatProfiler : MonoBehaviour
    {
        #region Combat Settings

        [Header("Combat Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private GameObject axe;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private int attackDamage = 25;
        [SerializeField] private LayerMask enemyLayers;

        #endregion

        #region Combat Timings

        [Header("Combat Timings")]
        private const float AttackCooldown = 0.5f;
        private const float WeaponHideTime = 20f;
        private const float AttackDelay = 0.5f;

        #endregion

        #region Stagger & Injury Settings

        [Header("Stagger & Injury Settings")]
        [SerializeField] private float staggerDuration = 0.5f;
        public int injuryThreshold = 30;

        private const float
            StaggerMovementForce = 0.5f,
            StaggerKnockbackForce = 20f;

        private static readonly int
            InjuredAnim = Animator.StringToHash("Injured"),
            StaggerAnim = Animator.StringToHash("Stagger");

        #endregion

        #region Audio Settings

        [Header("Audio Settings")]
        [SerializeField] private RandomAudio randomAudio;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource heartbeatAudio;
        [SerializeField] private AudioClip heartbeatSound, staggerSound;

        #endregion

        #region Dependencies

        private RandomAnimation _randomAnimation;
        private InputControls _actions;
        private PlayerMovement _playerMovement;
        private PlayerHealth _playerHealth;
        private Animator _animator;
        private Rigidbody _rigidbody;

        #endregion

        #region Private Variables

        private bool _canAttack = true;
        private float _lastAttackTime;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _randomAnimation = GetComponent<RandomAnimation>();
            _actions = new InputControls();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerHealth = GetComponent<PlayerHealth>();
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();

            axe.SetActive(false);

            var isLowHealth = _playerHealth.GetCurrentHealth() <= injuryThreshold;
            _animator.SetBool(InjuredAnim, isLowHealth);
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Attack.performed += AttackAction;
        }

        private void OnDisable()
        {
            _actions.Profiler.Attack.performed -= AttackAction;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            HandleWeaponVisibility();
            UpdateInjuryState();
        }

        #endregion

        #region Combat System

        private void AttackAction(InputAction.CallbackContext context)
        {
            if (!_canAttack) return;

            _lastAttackTime = Time.time;
            axe.SetActive(true);
            StartCoroutine(PerformAttack());
        }

        private IEnumerator PerformAttack()
        {
            _canAttack = false;
            _playerMovement.SetMovement(true);
            _randomAnimation.PlayRandomAttack();

            yield return new WaitForSeconds(AttackDelay);
            randomAudio.PlayRandomSound("axe", 0.2f);

            PerformHitDetection();

            yield return new WaitForSeconds(AttackCooldown);
            _playerMovement.SetMovement(false);
            _canAttack = true;
        }

        private void PerformHitDetection()
        {
            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
            foreach (var enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
            }
        }

        private void HandleWeaponVisibility()
        {
            if (axe.activeSelf && Time.time - _lastAttackTime > WeaponHideTime)
            {
                axe.SetActive(false);
            }
        }

        #endregion

        #region Stagger & Injury

        public IEnumerator StaggerEffect()
        {
            _animator.SetTrigger(StaggerAnim);

            if (staggerSound is not null && audioSource is not null)
                audioSource.PlayOneShot(staggerSound);

            var originalMovementForce = _playerMovement.GetMovementForce();
            _playerMovement.SetMovementForce(StaggerMovementForce);

            var knockback = -transform.forward * StaggerKnockbackForce;
            _rigidbody.linearVelocity = knockback;

            yield return new WaitForSeconds(staggerDuration);

            _playerMovement.SetMovementForce(originalMovementForce);
        }

        private void UpdateInjuryState()
        {
            var isLowHealth = _playerHealth.GetCurrentHealth() <= injuryThreshold;
            var isStandingStill = _playerMovement.GetMoveInput().sqrMagnitude < 0.01f;

            var shouldBeInjured = isLowHealth && isStandingStill;
            _animator.SetBool(InjuredAnim, shouldBeInjured);

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
    }
}
