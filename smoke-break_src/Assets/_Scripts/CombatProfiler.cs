using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace _Scripts
{
    public class CombatProfiler : MonoBehaviour
    {
        [Header("Combat Settings")]
        private Animator _animator;
        public Transform attackPoint;
        public GameObject axe;
        public float attackRange = 1.5f;
        public int attackDamage = 25;
        public LayerMask enemyLayers;
    
        private InputControls _actions;
        private bool _canAttack = true;
        private const float AttackCooldown = 0.5f;
        private PlayerProfiler _player;
        private float _lastAttackTime;
        private const float WeaponHideTime = 5f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
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
                axe.SetActive(false); // Hide weapon after inactivity
            }
        }

        private void AttackAction(InputAction.CallbackContext context)
        {
            if (!_canAttack) return;
            _lastAttackTime = Time.time;
            axe.SetActive(true);
            StartCoroutine(PerformAttack("Attack", attackDamage, AttackCooldown));
        }
        
        private IEnumerator PerformAttack(string attackType, int damage, float cooldown)
        {
            _canAttack = false;
            _player.disableMovement = true;
            _animator.SetTrigger(attackType); // Triggers attack animation

            yield return new WaitForSeconds(0.5f); // Delay before dealing damage

            var hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
            foreach (var enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            }

            yield return new WaitForSeconds(cooldown); // Attack cooldown
            _player.disableMovement = false;
            _canAttack = true;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange); // Visualize attack range
        }
    }
}
