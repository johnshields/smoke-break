using System.Collections;
using UnityEngine;

namespace _Scripts
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int health = 100;

        [Header("Hit Effect Settings")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float hitEffectDuration = 0.2f;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;

        public Renderer enemyRenderer;
        private Color _originalColor;
        private Rigidbody _rigidbody;
        private bool _isKnockedBack;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            if (enemyRenderer != null)
                _originalColor = enemyRenderer.material.color;
        }

        public void TakeDamage(int damage, Vector3 hitDirection)
        {
            if (damage <= 0) return;
            
            health -= damage;
            GetComponent<EnemyAI>()?.EnterChaseState();

            if (enemyRenderer != null)
                StartCoroutine(FlashEffect());

            if (_rigidbody != null)
                StartCoroutine(ApplyKnockback(hitDirection));

            if (health <= 0)
                Die();
        }

        private IEnumerator FlashEffect()
        {
            if (enemyRenderer == null) yield break;

            enemyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(hitEffectDuration);
            enemyRenderer.material.color = _originalColor;
        }

        private IEnumerator ApplyKnockback(Vector3 direction)
        {
            if (_rigidbody == null || _isKnockedBack) yield break;

            _isKnockedBack = true;
            _rigidbody.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
            yield return new WaitForSeconds(knockbackDuration);
            _isKnockedBack = false;
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} has been destroyed!");
            Destroy(gameObject);
        }
    }
}
