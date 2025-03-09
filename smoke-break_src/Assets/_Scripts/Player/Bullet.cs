using _Scripts.AI;
using UnityEngine;

namespace _Scripts.Player
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int damage = 20;
        [SerializeField] private float lifetime = 3f;

        private void Start()
        {
            Destroy(gameObject, lifetime); // Destroy bullet after 3 seconds
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject); // Destroy bullet on impact
            }
        }
    }
}