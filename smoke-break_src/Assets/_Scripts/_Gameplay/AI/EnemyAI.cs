using System.Collections;
using _Scripts._Gameplay._Player;
using _Scripts.enums;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Scripts._Gameplay.AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class EnemyAI : MonoBehaviour
    {
        #region Animation Hashes

        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking"),
            SpeedAnim = Animator.StringToHash("Speed");

        #endregion

        #region Constants

        private const float PatrolRadius = 15f;
        private const float StuckTimeout = 2f;
        private const float VelocityThreshold = 0.1f;
        private const float AttackWindUp = 0.5f;
        private const float LosePlayerDelay = 3f;
        private const float IdlePauseBeforePatrol = 1.5f;
        private const float RotationSpeed = 5f;
        private const float NavMeshSampleRange = 10f;
        private const int NavMeshSampleAttempts = 5;
        private const float NavMeshInitDelay = 0.1f;
        private const float DodgeCooldown = 0.2f;

        #endregion

        #region Serialised Fields

        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 25f;
        [SerializeField] private float attackRange = 5f;
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
        private Coroutine _losePlayerCoroutine;

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

            StartCoroutine(InitializeNavMeshAgent());
        }

        private void Update()
        {
            if (_agent == null || _player == null) return;

            if (!_agent.isOnNavMesh && _agent.enabled)
            {
                HandleNavMeshRecovery();
                return;
            }

            var distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            switch (_currentState)
            {
                case AIState.Patrolling:
                    Patrol();
                    if (distanceToPlayer <= detectionRange)
                        SetState(AIState.Chasing);
                    break;

                case AIState.Chasing:
                    ChasePlayer(distanceToPlayer);
                    break;

                case AIState.Attacking:
                    break;
            }
        }

        #endregion

        #region State Management

        private void SetState(AIState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;

            switch (newState)
            {
                case AIState.Patrolling:
                    _animator.SetFloat(SpeedAnim, 0f);
                    _animator.SetBool(IsAttacking, false);
                    _agent.isStopped = false;
                    CancelLosePlayerCoroutine();
                    break;

                case AIState.Chasing:
                    _animator.SetBool(IsAttacking, false);
                    _agent.isStopped = false;
                    break;

                case AIState.Attacking:
                    _animator.SetFloat(SpeedAnim, 0f);
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                    CancelLosePlayerCoroutine();
                    StartCoroutine(AttackPlayer());
                    break;
            }
        }

        public void EnterChaseState()
        {
            CancelLosePlayerCoroutine();
            SetState(AIState.Chasing);
        }

        #endregion

        #region AI Behaviour

        private void Patrol()
        {
            if (!_agent.isOnNavMesh) return;

            if (!_agent.hasPath || _agent.remainingDistance <= waypointTolerance)
            {
                var patrolTarget = GetRandomNavMeshPosition(transform.position, PatrolRadius);
                if (patrolTarget != Vector3.zero)
                {
                    _agent.SetDestination(patrolTarget);
                    _stuckTimer = 0f;
                }
            }

            if (_agent.velocity.magnitude < VelocityThreshold)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= StuckTimeout)
                {
                    _agent.ResetPath();
                    _stuckTimer = 0f;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }
        }

        private void ChasePlayer(float distanceToPlayer)
        {
            if (distanceToPlayer <= attackRange && _canAttack)
            {
                SetState(AIState.Attacking);
                return;
            }

            if (_agent.isOnNavMesh)
            {
                _agent.SetDestination(_player.position);
                _animator.SetFloat(SpeedAnim, _agent.velocity.magnitude);
                RotateTowards(_player.position);
            }

            if (distanceToPlayer > detectionRange && _losePlayerCoroutine == null)
                _losePlayerCoroutine = StartCoroutine(LosePlayerAfterDelay());
        }

        private IEnumerator LosePlayerAfterDelay()
        {
            yield return new WaitForSeconds(LosePlayerDelay);

            if (_currentState == AIState.Chasing &&
                Vector3.Distance(transform.position, _player.position) > detectionRange)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _animator.SetFloat(SpeedAnim, 0f);

                yield return new WaitForSeconds(IdlePauseBeforePatrol);
                SetState(AIState.Patrolling);
            }

            _losePlayerCoroutine = null;
        }

        private void CancelLosePlayerCoroutine()
        {
            if (_losePlayerCoroutine == null) return;

            StopCoroutine(_losePlayerCoroutine);
            _losePlayerCoroutine = null;
        }

        private IEnumerator AttackPlayer()
        {
            _canAttack = false;
            _animator.SetBool(IsAttacking, true);

            if (attackSound is not null && audioSource is not null)
                audioSource.PlayOneShot(attackSound);

            while (Vector3.Distance(transform.position, _player.position) <= attackRange)
            {
                RotateTowards(_player.position);
                yield return new WaitForSeconds(AttackWindUp);

                if (Vector3.Distance(transform.position, _player.position) <= attackRange)
                    _playerHealth?.TakeDamage(attackDamage);

                yield return new WaitForSeconds(attackCooldown);
            }

            _canAttack = true;
            SetState(AIState.Chasing);
        }

        #endregion

        #region Utility

        private void HandleNavMeshRecovery()
        {
            var newPosition = GetRandomNavMeshPosition(transform.position, NavMeshSampleRange);
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
            yield return new WaitForSeconds(NavMeshInitDelay);

            var spawnPosition = GetRandomNavMeshPosition(transform.position, NavMeshSampleRange);
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

        private static Vector3 GetRandomNavMeshPosition(Vector3 origin, float range)
        {
            for (var i = 0; i < NavMeshSampleAttempts; i++)
            {
                var randomPoint = origin + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

                if (NavMesh.SamplePosition(randomPoint, out var hit, range, NavMesh.AllAreas))
                    return hit.position;
            }

            return Vector3.zero;
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
            transform.rotation =
                Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * RotationSpeed);
        }

        #endregion
    }
}
