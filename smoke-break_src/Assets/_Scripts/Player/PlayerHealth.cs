using System.IO;
using _Scripts.Managers;
using _Scripts.Objects;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        #region Fields

        private string _savePath;
        private bool _invulnerable;
        private PlayerRespawner _respawner;
        private PlayerProfiler _player;

        #endregion

        #region Health Settings

        [Header("Health Settings")] [SerializeField]
        private int currentHealth = 100;

        [SerializeField] private int maxHealth = 100;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();
            _player = GetComponent<PlayerProfiler>();
            _respawner = GetComponent<PlayerRespawner>();

            LoadHealth();
        }

        private void Update()
        {
            CheckForRespawn();
        }

        #endregion

        #region Health Management

        public int GetMaxHealth() => maxHealth;
        public int GetCurrentHealth() => currentHealth;

        public void SetCurrentHealth(int health)
        {
            currentHealth = health;
            SaveHealth();
        }

        public void RestoreHealth(int amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        }

        public void SetInvulnerable(bool value)
        {
            _invulnerable = value;
        }

        #endregion

        #region Damage Handling

        public void TakeDamage(int damage)
        {
            if (_invulnerable || _respawner.isRespawning) return;

            currentHealth -= damage;
            StartCoroutine(_player.StaggerEffect());
        }

        private void CheckForRespawn()
        {
            if (currentHealth <= 0 && !_respawner.isRespawning)
            {
                _respawner.isRespawning = true;
                _respawner.InitRespawn();
            }
        }

        #endregion

        #region Save System

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

        private void SaveHealth()
        {
            if (!File.Exists(_savePath)) return;

            var json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            data.playerHealth = currentHealth;
            File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
        }

        #endregion
    }
}