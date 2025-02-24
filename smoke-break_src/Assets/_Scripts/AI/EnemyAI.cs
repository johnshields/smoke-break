using System.Collections;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.AI
{
    public class EnemyAI : MonoBehaviour
    {
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Speed = Animator.StringToHash("Speed");

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
        private float _stuckTimer;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            _playerScript = _player.GetComponent<PlayerHealth>();

            StartCoroutine(InitializeNavMeshAgent()); // Initialize agent safely
        }

        private void Update()
        {
            if (_agent == null)
            {
                Debug.LogError($"❌ {gameObject.name} has NO NavMeshAgent! Destroying.");
                Destroy(gameObject);
                return;
            }

            if (!_agent.isOnNavMesh)
            {
                // Try to re-warp the agent to a valid NavMesh position
                Vector3 newPosition = GetRandomNavMeshPosition(transform.position, 10f, 5);
                if (newPosition != Vector3.zero)
                {
                    _agent.Warp(newPosition);
                }
                else
                {
                    Debug.LogError($"❌ {gameObject.name} could not recover a valid NavMesh position!");
                    gameObject.SetActive(false); // Disable the AI if recovery fails
                }

                return;
            }

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

        private IEnumerator InitializeNavMeshAgent()
        {
            if (_agent == null)
            {
                Debug.LogError($"🚨 {gameObject.name} is missing a NavMeshAgent component!");
                yield break;
            }

            _agent.enabled = false; // Disable before adjusting position
            yield return new WaitForSeconds(0.1f); // Small delay to let Unity process

            // Try to find a valid NavMesh position up to 5 times
            Vector3 spawnPosition = GetRandomNavMeshPosition(transform.position, 10f, 5);

            if (spawnPosition != Vector3.zero)
            {
                transform.position = spawnPosition;
                _agent.enabled = true;
                _agent.Warp(spawnPosition);
                _agent.ResetPath(); // Ensure it's on the NavMesh
            }
            else
            {
                Debug.LogError($"❌ {gameObject.name} could not find a valid NavMesh position! Disabling AI.");
                gameObject.SetActive(false); // Disable if placement fails
            }
        }

        private Vector3 GetRandomNavMeshPosition(Vector3 origin, float range, int maxAttempts)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPoint = origin + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
                {
                    return hit.position; // Found a valid position
                }
            }

            return Vector3.zero; // No valid position found
        }

        private void Patrol()
        {
            if (_agent == null || !_agent.isOnNavMesh)
            {
                Debug.LogWarning($"⚠️ {gameObject.name} cannot patrol - not on a NavMesh!");
                return;
            }

            // Check if the agent has no path OR has reached its destination
            if (!_agent.hasPath || _agent.remainingDistance <= waypointTolerance)
            {
                // Generate a random point and check if it's valid
                Vector3 patrolTarget = GetRandomNavMeshPosition(transform.position, 15f, 5);

                if (patrolTarget != Vector3.zero)
                    _agent.SetDestination(patrolTarget);
            }

            // Detect if the enemy is stuck (not moving for 2 seconds)
            if (_agent.velocity.magnitude < 0.1f)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= 2f) // If stuck for more than 2 seconds
                {
                    Debug.LogWarning($"⚠️ {gameObject.name} is stuck! Forcing new patrol path.");
                    _agent.ResetPath();
                    _stuckTimer = 0;
                }
            }
            else
            {
                _stuckTimer = 0; // Reset timer when moving
            }
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
                    _animator.SetFloat(Speed, _agent.velocity.magnitude);
                    RotateTowards(_player.position);
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