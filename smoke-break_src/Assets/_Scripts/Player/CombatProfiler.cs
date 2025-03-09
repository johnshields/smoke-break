using System.Collections;
using _Scripts.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player
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

        #region Audio Settings

        [Header("Audio Settings")]
        [SerializeField] private RandomAudio randomAudio;

        #endregion

        #region Dependencies

        private RandomAnimation _randomAnimation;
        private InputControls _actions;
        private PlayerProfiler _player;

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
            _player = GetComponent<PlayerProfiler>();

            axe.SetActive(false);
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
            _player.SetMovement(true);
            _randomAnimation.PlayRandomAttack();

            yield return new WaitForSeconds(AttackDelay);
            randomAudio.PlayRandomSound("axe", 0.2f);

            PerformHitDetection();

            yield return new WaitForSeconds(AttackCooldown);
            _player.SetMovement(false);
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
    }
}