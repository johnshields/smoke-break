using System;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class InitSave : MonoBehaviour
    {
        public bool saveGame;
        private PlayerProfiler _playerProfiler;
        private PistolProfiler _pistolProfiler;

        private void Start()
        {
            _pistolProfiler = FindObjectOfType<PistolProfiler>();
            _playerProfiler = FindObjectOfType<PlayerProfiler>();
            SaveManager.SaveGame(_playerProfiler, _pistolProfiler);
        }

        private void Update()
        {
            if (saveGame)
            {
                saveGame = false;
                SaveManager.SaveGame(_playerProfiler, _pistolProfiler);
            }
        }
    }
}