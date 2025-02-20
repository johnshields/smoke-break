using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class InitSave : MonoBehaviour
    {
        private PlayerProfiler _playerProfiler;
        private PistolProfiler _pistolProfiler;
        public bool saveGame;
        public bool resetGame;
        private PlayerRespawner _playerRespawner;

        private void Start()
        {
            _pistolProfiler = FindObjectOfType<PistolProfiler>();
            _playerProfiler = FindObjectOfType<PlayerProfiler>();
            _playerRespawner = FindObjectOfType<PlayerRespawner>();
            SaveManager.SaveGame(_playerProfiler, _pistolProfiler);
        }

        private void Update()
        {
            if (saveGame)
            {
                saveGame = false;
                SaveManager.SaveGame(_playerProfiler, _pistolProfiler);
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