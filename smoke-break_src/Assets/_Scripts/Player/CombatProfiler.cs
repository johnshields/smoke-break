using System.Collections;
using _Scripts.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player
{
    public class CombatProfiler : MonoBehaviour
    {
        [Header("Combat Settings")] [SerializeField]
        private Transform attackPoint;

        [SerializeField] private GameObject axe;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private int attackDamage = 25;
        [SerializeField] private LayerMask enemyLayers;

        [Header("Combat Timings")] private const float AttackCooldown = 0.5f;
        private const float WeaponHideTime = 20f;
        private const float AttackDelay = 0.5f;

        [Header("Audio Settings")] [SerializeField]
        private RandomAudio randomAudio;

        private RandomAnimation _randomAnimation;
        private InputControls _actions;
        private PlayerProfiler _player;
        private bool _canAttack = true;
        private float _lastAttackTime;

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
            if (axe.activeSelf && Time.time - _lastAttackTime > WeaponHideTime)
            {
                axe.SetActive(false);
            }
        }

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
            _player.disableMovement = true;
            _randomAnimation.PlayRandomAttack();

            yield return new WaitForSeconds(AttackDelay);
            randomAudio.PlayRandomSound("Sounds/Axe/", 0.2f);

            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
            foreach (var enemy in hitEnemies)
            {
                var knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage, knockbackDirection);
            }

            yield return new WaitForSeconds(AttackCooldown);
            _player.disableMovement = false;
            _canAttack = true;
        }
    }
}