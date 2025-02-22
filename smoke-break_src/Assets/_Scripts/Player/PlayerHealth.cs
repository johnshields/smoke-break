using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")] public int maxHealth = 100;
        public int currentHealth;
        public bool invulnerable;
        private PlayerRespawner _respawner;
        private PlayerProfiler _player;

        private void Awake()
        {
            _player = GetComponent<PlayerProfiler>();
            _respawner = GetComponent<PlayerRespawner>();
            currentHealth = PlayerPrefs.HasKey("PlayerHealth") ? PlayerPrefs.GetInt("PlayerHealth") : 20;
        }

        private void Update()
        {
            if (currentHealth <= 0 && !_respawner.isRespawning)
            {
                _respawner.isRespawning = true;
                _respawner.InitRespawn();
            }
        }

        public void SetCurrentHealth(int health)
        {
            currentHealth = health;
        }

        public int GetCurrentHealth()
        {
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            PlayerPrefs.Save();

            return currentHealth;
        }

        public void RestoreHealth(int itemValue)
        {
            currentHealth = Mathf.Clamp(currentHealth + itemValue, 0, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (invulnerable) return;

            if (_respawner.isRespawning) return;
            print($"🔥 Kanta took {damage} damage!");
            currentHealth -= damage;

            StartCoroutine(_player.StaggerEffect());
        }
    }
}