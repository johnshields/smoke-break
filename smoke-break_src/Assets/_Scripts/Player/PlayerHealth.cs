using System.IO;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        private string _savePath;

        [Header("Health Settings")] [SerializeField]
        private int maxHealth = 100;

        [SerializeField] private int currentHealth;
        private bool _invulnerable;
        private PlayerRespawner _respawner;
        private PlayerProfiler _player;

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

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

        public int GetMaxHealth() => maxHealth;
        public int GetCurrentHealth() => currentHealth;

        public void SetInvulnerable(bool value)
        {
            _invulnerable = value;
        }

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
            if (_invulnerable) return;

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