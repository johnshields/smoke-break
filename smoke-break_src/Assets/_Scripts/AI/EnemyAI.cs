using System.Collections;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Scripts.AI
{
    public class EnemyAI : MonoBehaviour
    {
        #region Animation Hashes

        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Speed = Animator.StringToHash("Speed");

        #endregion

        #region Serialized Fields

        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Patrolling Settings")]
        [SerializeField] private float waypointTolerance = 1.5f;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioSource audioSource;

        #endregion

        #region Private Variables

        private AIState _currentState = AIState.Patrolling;
        private NavMeshAgent _agent;
        private Transform _player;
        private Animator _animator;
        private PlayerHealth _playerHealth;
        private bool _canAttack = true;
        private float _stuckTimer;
        private int _idleStateHash;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogError($"{gameObject.name} could not find Player! Disabling AI.");
                enabled = false;
                return;
            }

            _player = playerObject.transform;
            _playerHealth = _player.GetComponent<PlayerHealth>();

            DetectIdleState();
            StartCoroutine(InitializeNavMeshAgent());
        }

        private void Update()
        {
            if (_agent == null)
            {
                Debug.LogError($"{gameObject.name} has NO NavMeshAgent! Destroying.");
                Destroy(gameObject);
                return;
            }

            if (!_agent.isOnNavMesh && _agent.enabled)
            {
                HandleNavMeshRecovery();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

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
                    // Attack logic is handled within AttackPlayer coroutine.
                    break;
            }
        }

        #endregion

        #region AI Behavior

        private void Patrol()
        {
            if (_agent == null || !_agent.isOnNavMesh) return;

            if (!_agent.hasPath || _agent.remainingDistance <= waypointTolerance)
            {
                Vector3 patrolTarget = GetRandomNavMeshPosition(transform.position, 15f, 5);
                if (patrolTarget != Vector3.zero)
                {
                    _agent.SetDestination(patrolTarget);
                    _stuckTimer = 0; // Reset stuck timer
                }
            }

            if (_agent.velocity.magnitude < 0.1f)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= 2f)
                {
                    _agent.ResetPath();
                    _stuckTimer = 0;
                }
            }
            else
            {
                _stuckTimer = 0;
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

            if (_currentState == AIState.Chasing && Vector3.Distance(transform.position, _player.position) > detectionRange)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _animator.SetFloat(Speed, 0f);

                if (_idleStateHash != 0 && _animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _idleStateHash)
                {
                    _animator.Play(_idleStateHash);
                }

                yield return new WaitForSeconds(1.5f);
                _currentState = AIState.Patrolling;
                _agent.isStopped = false;
                Patrol();
            }
        }

        private IEnumerator AttackPlayer()
        {
            _canAttack = false;
            _animator.SetBool(IsAttacking, true);

            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }

            while (Vector3.Distance(transform.position, _player.position) <= attackRange)
            {
                RotateTowards(_player.position);
                yield return new WaitForSeconds(0.5f);

                if (Vector3.Distance(transform.position, _player.position) <= attackRange)
                {
                    _playerHealth?.TakeDamage(attackDamage);
                }

                yield return new WaitForSeconds(attackCooldown);
            }

            _animator.SetBool(IsAttacking, false);
            _canAttack = true;
            _agent.isStopped = false;
            _currentState = AIState.Chasing;
        }
        
        public void EnterChaseState()
        {
            _currentState = AIState.Chasing;
        }

        #endregion

        #region Utility Functions

        private void DetectIdleState()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"{gameObject.name} has no Animator Controller assigned!");
                return;
            }

            _idleStateHash = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }

        private void HandleNavMeshRecovery()
        {
            Vector3 newPosition = GetRandomNavMeshPosition(transform.position, 10f, 5);
            if (newPosition != Vector3.zero)
            {
                _agent.Warp(newPosition);
            }
            else
            {
                Debug.LogError($"{gameObject.name} could not recover a valid NavMesh position!");
                gameObject.SetActive(false);
            }
        }

        private IEnumerator InitializeNavMeshAgent()
        {
            if (_agent == null) yield break;

            _agent.enabled = false;
            yield return new WaitForSeconds(0.1f);

            Vector3 spawnPosition = GetRandomNavMeshPosition(transform.position, 10f, 5);
            if (spawnPosition != Vector3.zero)
            {
                transform.position = spawnPosition;
                _agent.enabled = true;
                _agent.Warp(spawnPosition);
                _agent.ResetPath();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private Vector3 GetRandomNavMeshPosition(Vector3 origin, float range, int maxAttempts)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPoint = origin + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

            return Vector3.zero;
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        #endregion
    }
}