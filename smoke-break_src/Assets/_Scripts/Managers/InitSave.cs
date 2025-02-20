using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class InitSave : MonoBehaviour
    {
        private void Start()
        {
            SaveManager.SaveGame(FindObjectOfType<PlayerProfiler>(), FindObjectOfType<PistolProfiler>());
        }
    }
}