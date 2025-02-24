using System.Collections;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.AI
{
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

        [Header("AI Settings")] [SerializeField]
        private float detectionRange = 15f;

        private AIState _currentState = AIState.Patrolling;

        [SerializeField] private float attackRange = 2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Patrolling Settings")] [SerializeField]
        private float waypointTolerance = 1.5f;

        private NavMeshAgent _agent;
        private Transform _player;
        private Animator _animator;
        private bool _canAttack = true;
        private PlayerHealth _playerScript;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            _playerScript = _player.GetComponent<PlayerHealth>();

            _agent.enabled = false;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Debug.Log($"Warping {gameObject.name} to valid NavMesh position at {hit.position}");

                transform.position = hit.position;
                _agent.enabled = true;
                _agent.Warp(hit.position);

                _agent.ResetPath();
            }
        }

        private void Update()
        {
            var distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            switch (_currentState)
            {
                case AIState.Patrolling:
                    Patrol();
                    if (distanceToPlayer <= detectionRange)
                        _currentState = AIState.Chasing;
                    break;

                case AIState.Chasing:
                    ChasePlayer(distanceToPlayer);
                    break;

                case AIState.Attacking:
                    break;
            }
        }

        private void Patrol()
        {
            if (!_agent.hasPath || _agent.remainingDistance <= waypointTolerance)
            {
                var randomPoint = GetRandomPoint(transform.position, 10f);

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    _agent.SetDestination(hit.position);
            }

            if (_agent.isStopped) _agent.isStopped = false;

            RotateTowards(_agent.steeringTarget);
        }

        private Vector3 GetRandomPoint(Vector3 center, float range)
        {
            var randomPos = center + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

            return NavMesh.SamplePosition(randomPos, out var hit, range, NavMesh.AllAreas) ? hit.position : center;
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
                if (_currentState != AIState.Attacking && _agent.isOnNavMesh)
                {
                    _agent.isStopped = false;
                    _agent.ResetPath();
                    _agent.SetDestination(_player.position);
                }

                if (distanceToPlayer > detectionRange)
                    StartCoroutine(LosePlayerAfterDelay());
            }
        }

        private IEnumerator LosePlayerAfterDelay()
        {
            yield return new WaitForSeconds(3f);

            if (_currentState == AIState.Chasing)
            {
                _currentState = AIState.Patrolling;
                Patrol();
            }
        }

        public void EnterChaseState()
        {
            _currentState = AIState.Chasing;
        }

        private IEnumerator AttackPlayer()
        {
            _canAttack = false;
            _animator.SetBool(IsAttacking, true);

            while (Vector3.Distance(transform.position, _player.position) <= attackRange)
            {
                yield return new WaitForSeconds(.5f);

                if (Vector3.Distance(transform.position, _player.position) <= attackRange)
                {
                    _playerScript?.TakeDamage(attackDamage);
                }

                yield return new WaitForSeconds(attackCooldown);
            }

            _animator.SetBool(IsAttacking, false);
            _canAttack = true;
            _agent.isStopped = false;
            _currentState = AIState.Chasing;
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            direction.Normalize();

            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}