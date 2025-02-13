using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _Scripts
{
    public class HUDManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI bulletCounter;
        [SerializeField] private Slider healthBar;

        private PistolProfiler _pistol;
        private PlayerProfiler _player;

        private void Start()
        {
            _pistol = FindObjectOfType<PistolProfiler>();
            _player = FindObjectOfType<PlayerProfiler>();
            
            UpdateAmmo();
            UpdateHealth();
        }

        private void Update()
        {
            UpdateAmmo();
            UpdateHealth();
        }

        private void UpdateAmmo()
        {
            bulletCounter.text = $"Ammo: {_pistol.currentAmmo} / {_pistol.maxAmmo}";
        }

        private void UpdateHealth()
        {
            healthBar.value = _player.GetCurrentHealth();
        }
    }
}