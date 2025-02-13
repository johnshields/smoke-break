using UnityEngine;
using UnityEngine.AI;

namespace _Scripts
{
    public class EnemyAI : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _player;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;

            // ✅ Ensure the NavMeshAgent rotates properly
            _agent.updateRotation = false;
        }

        private void Update()
        {
            if (_player == null) return;

            RotateTowardsPlayer();
        }

        private void RotateTowardsPlayer()
        {
            Vector3 direction = (_player.position - transform.position).normalized;
            direction.y = 0; // ✅ Prevents tilting
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}