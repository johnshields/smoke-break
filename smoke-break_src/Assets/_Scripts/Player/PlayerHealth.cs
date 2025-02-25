using System.IO;
using _Scripts.Managers;
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
        private string _playerId;
        private string _savePath;

        private void Awake()
        {
            _playerId = SaveManager.GetOrCreatePlayerId();
            _savePath = Path.Combine(SaveManager.SaveDirectory, $"savegame_{_playerId}.json");

            _player = GetComponent<PlayerProfiler>();
            _respawner = GetComponent<PlayerRespawner>();

            LoadHealth();
        }

        private void Update()
        {
            if (currentHealth <= 0 && !_respawner.isRespawning)
            {
                _respawner.isRespawning = true;
                _respawner.InitRespawn();
            }
        }

        private void LoadHealth()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                currentHealth = data.playerHealth;
            }
            else
            {
                currentHealth = 30;
            }
        }

        public int GetCurrentHealth() => currentHealth;

        public void SetCurrentHealth(int health)
        {
            currentHealth = health;
            SaveHealth();
        }

        private void SaveHealth()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                data.playerHealth = currentHealth;
                File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
            }
        }

        public void TakeDamage(int damage)
        {
            if (invulnerable) return;

            if (_respawner.isRespawning) return;
            print($"🔥 Kanta took {damage} damage!");
            currentHealth -= damage;

            StartCoroutine(_player.StaggerEffect());
        }

        public void RestoreHealth(int itemValue)
        {
            currentHealth = Mathf.Clamp(currentHealth + itemValue, 0, maxHealth);
        }
    }
}