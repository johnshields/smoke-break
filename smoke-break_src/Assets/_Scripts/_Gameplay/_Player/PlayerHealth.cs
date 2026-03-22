using _Scripts._Systems.Managers;
using UnityEngine;

namespace _Scripts._Gameplay._Player
{
    public class PlayerHealth : MonoBehaviour
    {
        #region Fields

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
            _player = GetComponent<PlayerProfiler>();
            _respawner = GetComponent<PlayerRespawner>();

            var saveData = SaveManager.LoadFromDisk();
            if (saveData != null)
                currentHealth = saveData.playerHealth;
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
    }
}