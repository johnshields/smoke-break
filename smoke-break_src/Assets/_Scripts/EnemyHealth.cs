using System.Collections;
using UnityEngine;

namespace _Scripts
{
    public class EnemyHealth : MonoBehaviour
    {
        public int health = 100; // Set enemy health
        private Renderer _enemyRenderer;
        private Color _originalColor;
        public Color hitColor = new Color(0.61f, 0.14f, 0.17f); 
        public float hitEffectDuration = 0.2f;

        private void Start()
        {
            _enemyRenderer = GetComponent<Renderer>(); // ✅ Get enemy material
            if (_enemyRenderer != null)
            {
                _originalColor = _enemyRenderer.material.color;
            }
        }
        
        public void TakeDamage(int damage)
        {
            print("TakeDamage() called on: " + gameObject.name);
            health -= damage;
            print(gameObject.name + " took " + damage + " damage! Remaining HP: " + health);

            if (_enemyRenderer != null)
            {
                StartCoroutine(FlashEffect()); 
            }

            if (health <= 0)
            {
                Die();
            }
        }
        
        private IEnumerator FlashEffect()
        {
            _enemyRenderer.material.color = hitColor; 
            yield return new WaitForSeconds(hitEffectDuration);
            _enemyRenderer.material.color = _originalColor;
        }

        private void Die()
        {
            print(gameObject.name + " has been destroyed!");
            Destroy(gameObject); // Destroy the cube when health reaches 0
        }
    }
}