using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class InitSave : MonoBehaviour
    {
        private PlayerProfiler _playerProfiler;
        private PistolProfiler _pistolProfiler;
        private PlayerHealth _playerHealth;
        public bool saveGame;
        public bool resetGame;
        private PlayerRespawner _playerRespawner;

        private void Start()
        {
            _pistolProfiler = FindObjectOfType<PistolProfiler>();
            _playerProfiler = FindObjectOfType<PlayerProfiler>();
            _playerHealth = FindObjectOfType<PlayerHealth>();
            _playerRespawner = FindObjectOfType<PlayerRespawner>();
        }

        private void Update()
        {
            if (saveGame)
            {
                saveGame = false;
                SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
            }
            else if (resetGame)
            {
                resetGame = false;
                SaveManager.ResetGame();
                _playerRespawner.InitRespawn();
            }
        }
    }
}