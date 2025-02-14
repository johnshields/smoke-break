using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts
{
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Speed = Animator.StringToHash("Speed");

        private enum AIState { Patrolling, Chasing, Attacking }
        private AIState _currentState = AIState.Patrolling;

        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Patrolling Settings")]
        [SerializeField] private float waypointTolerance = 1.5f;

        private NavMeshAgent _agent;
        private Transform _player;
        private Animator _animator;
        private bool _canAttack = true;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;

            _agent.updateRotation = false;
        }

        private void Update()
        {
            var distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            switch (_currentState)
            {
                case AIState.Patrolling:
                    Patrol();
                    if (distanceToPlayer <= detectionRange)
                    {
                        _currentState = AIState.Chasing;
                    }
                    break;
                case AIState.Chasing:
                    ChasePlayer(distanceToPlayer);
                    break;
                case AIState.Attacking:
                    break;
            }

            _animator.SetFloat(Speed, _agent.velocity.magnitude);
        }

        private void Patrol()
        {
            if (_agent.remainingDistance <= waypointTolerance)
            {
                Vector3 randomPoint = GetRandomPoint(transform.position, 10f);
                _agent.SetDestination(randomPoint);
            }
            
            if (_agent.isStopped) _agent.isStopped = false;

            RotateTowards(_agent.steeringTarget);
        }
        
        private Vector3 GetRandomPoint(Vector3 center, float range)
        {
            var randomPos = center + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
            NavMeshHit hit;
            
            if (NavMesh.SamplePosition(randomPos, out hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center;
        }

        private void ChasePlayer(float distanceToPlayer)
        {
            if (distanceToPlayer <= attackRange && _canAttack)
            {
                _agent.isStopped = true; 
                _agent.velocity = Vector3.zero; 
                _currentState = AIState.Attacking;
                StartCoroutine(AttackPlayer());
            }
            else
            {
                if (_currentState != AIState.Attacking)
                {
                    _agent.isStopped = false; 
                    _agent.SetDestination(_player.position);
                    RotateTowards(_player.position);
                }

                if (distanceToPlayer > detectionRange)
                {
                    StartCoroutine(LosePlayerAfterDelay());
                }
            }
        }
        
        private IEnumerator LosePlayerAfterDelay()
        {
            yield return new WaitForSeconds(3f);
            if (_currentState == AIState.Chasing)
            {
                _currentState = AIState.Patrolling;
            }
        }

        private IEnumerator AttackPlayer()
        {
            _canAttack = false;
            _animator.SetBool(IsAttacking, true);

            yield return new WaitForSeconds(0.5f); 

            if (Vector3.Distance(transform.position, _player.position) <= attackRange)
            {
                var playerScript = _player.GetComponent<PlayerProfiler>();

                if (playerScript != null)
                {
                    playerScript.TakeDamage(attackDamage);
                }
            }

            yield return new WaitForSeconds(attackCooldown);

            _animator.SetBool(IsAttacking, false);
            _canAttack = true;

            if (Vector3.Distance(transform.position, _player.position) > attackRange)
            {
                _agent.isStopped = false;
                _currentState = AIState.Chasing;
            }
            else
            {
                StartCoroutine(AttackPlayer()); 
            }
        }


        private void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
    
            if (direction == Vector3.zero) return;

            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}