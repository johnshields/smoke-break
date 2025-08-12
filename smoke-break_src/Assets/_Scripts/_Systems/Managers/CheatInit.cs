using _Scripts._Gameplay._Player;
using UnityEngine;

namespace _Scripts._Systems.Managers
{
    public class CheatInit : MonoBehaviour
    {
        [SerializeField] private bool saveGame;
        [SerializeField] private bool resetGame;
        [SerializeField] private bool fullHealth;
        [SerializeField] private bool fullAmmo;
        private PlayerProfiler _playerProfiler;
        private PistolProfiler _pistolProfiler;
        private PlayerHealth _playerHealth;
        private PlayerRespawner _playerRespawner;

        private void Start()
        {
            _pistolProfiler = FindFirstObjectByType<PistolProfiler>();
            _playerProfiler = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _playerRespawner = FindFirstObjectByType<PlayerRespawner>();
        }

        private void Update()
        {
            HandleSave();
            HandleReset();
            HandleFullHealth();
            HandleFullAmmo();
        }

        private void HandleSave()
        {
            if (!saveGame) return;

            saveGame = false;
            SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
        }

        private void HandleReset()
        {
            if (!resetGame) return;

            resetGame = false;
            SaveManager.ResetGame();
            _playerRespawner.InitRespawn();
        }

        private void HandleFullHealth()
        {
            if (!fullHealth) return;

            fullHealth = false;
            _playerHealth.SetCurrentHealth(100);
            _playerHealth.SetInvulnerable(true);
        }

        private void HandleFullAmmo()
        {
            if (!fullAmmo) return;

            fullAmmo = false;
            _pistolProfiler.SetAmmo(9, 99);
        }
    }
}