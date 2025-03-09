using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.AI
{
    public class EnemyHealth : MonoBehaviour
    {
        #region Health Settings

        [Header("Health Settings")] [SerializeField]
        private int health = 100;

        #endregion

        #region Hit Effect Settings

        [Header("Hit Effect Settings")] [SerializeField]
        private Color hitColor = Color.red;

        [SerializeField] private float hitEffectDuration = 0.2f;

        #endregion

        #region Audio Settings

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip audioClip;

        #endregion

        #region Components

        private NavMeshAgent _agent;
        private EnemyAI _enemyAI;
        private Rigidbody _rigidbody;
        public Renderer enemyRenderer;
        private Color _originalColor;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _agent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (enemyRenderer != null)
                _originalColor = enemyRenderer.material.color;
        }

        #endregion

        #region Damage Handling

        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            health -= damage;
            _enemyAI?.EnterChaseState();

            if (enemyRenderer is not null)
                StartCoroutine(FlashEffect());

            if (_rigidbody is not null)
                StartCoroutine(StaggerEnemy(0.5f));

            if (health <= 0)
                Die();
        }

        private void Die()
        {
            if (audioSource is not null && audioClip is not null)
                audioSource.PlayOneShot(audioClip, 0.5f);

            Destroy(gameObject);
        }

        #endregion

        #region Visual Effects

        private IEnumerator FlashEffect()
        {
            if (enemyRenderer is null) yield break;

            enemyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(hitEffectDuration);
            enemyRenderer.material.color = _originalColor;
        }

        #endregion

        #region Knockback Handling

        private IEnumerator StaggerEnemy(float duration)
        {
            if (_agent is null) yield break;

            _agent.isStopped = true;
            yield return new WaitForSeconds(duration);
            _agent.isStopped = false;
        }

        #endregion
    }
}