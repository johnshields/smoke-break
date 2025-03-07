using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.AI
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")] [SerializeField]
        private int health = 100;

        [Header("Hit Effect Settings")] [SerializeField]
        private bool isDroid;

        [SerializeField] private Color hitColor = Color.red;

        [SerializeField] private float hitEffectDuration = 0.2f;

        [Header("Knockback Settings")] [SerializeField]
        private float knockbackForce = 5f;

        [SerializeField] private float knockbackDuration = 0.2f;

        public Renderer enemyRenderer;
        private Color _originalColor;
        private Rigidbody _rigidbody;
        private bool _isKnockedBack;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip audioClip;
        private NavMeshAgent _agent;
        private EnemyAI _enemyAI;

        private void Start()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _agent = GetComponent<NavMeshAgent>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (enemyRenderer != null)
                _originalColor = enemyRenderer.material.color;
        }

        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (damage <= 0) return;

            health -= damage;
            _enemyAI?.EnterChaseState();

            if (enemyRenderer is not null)
                StartCoroutine(FlashEffect());

            if (_rigidbody is not null && !isDroid) StartCoroutine(ApplyKnockback(hitDirection));

            if (health <= 0)
                Die();
        }

        private IEnumerator FlashEffect()
        {
            if (enemyRenderer is null) yield break;

            enemyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(hitEffectDuration);
            enemyRenderer.material.color = _originalColor;
        }

        private IEnumerator ApplyKnockback(Vector3 direction)
        {
            if (_rigidbody is null || _isKnockedBack) yield break;

            _isKnockedBack = true;
            if (_agent) _agent.enabled = false;

            _rigidbody.linearVelocity = direction.normalized * knockbackForce;

            yield return new WaitForSeconds(knockbackDuration);

            if (_agent) _agent.enabled = true;
            _isKnockedBack = false;
        }

        private void Die()
        {
            print($"{gameObject.name} has been destroyed!");
            if (audioSource is not null && audioClip is not null)
                audioSource.PlayOneShot(audioClip, .5f);
            Destroy(gameObject);
        }
    }
}