using System.Collections;
using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Managers
{
    public class HUDManager : MonoBehaviour
    {
        [Header("HUD Elements")] [SerializeField]
        private TextMeshProUGUI bulletCounter;

        [SerializeField] private Slider healthBar;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Image staminaFillImage;
        [SerializeField] private GameObject deathText;
        [SerializeField] private Color lowStaminaColor = Color.red;
        [SerializeField] private Color lowHealthColor = Color.red;

        private PistolProfiler _pistol;
        private PlayerProfiler _player;
        private PlayerHealth _playerHealth;
        private Color _originalStaminaColor;
        private Color _originalHealthColor;
        private bool _isFlashingHealth;
        private bool _isFlashingStamina;


        private void Start()
        {
            _pistol = FindObjectOfType<PistolProfiler>();
            _player = FindObjectOfType<PlayerProfiler>();
            _playerHealth = FindObjectOfType<PlayerHealth>();

            if (staminaFillImage != null)
                _originalStaminaColor = staminaFillImage.color;

            if (healthFillImage != null)
                _originalHealthColor = healthFillImage.color;

            UpdateAmmo();
            UpdateHealth();
            UpdateStamina(_player.maxStamina, _player.maxStamina);
        }

        private void Update()
        {
            UpdateAmmo();
            UpdateHealth();
            UpdateStamina(_player.currentStamina, _player.maxStamina);
        }

        private void UpdateAmmo()
        {
            if (bulletCounter is null) return;
            bulletCounter.text = $"{_pistol.currentClipAmmo}/{_pistol.storedAmmo}";
        }

        private void UpdateHealth()
        {
            if (healthBar is null) return;
            healthBar.value = _playerHealth.GetCurrentHealth();
            healthBar.fillRect.gameObject.SetActive(!(_playerHealth.GetCurrentHealth() <= 0));

            if (healthFillImage is not null)
                healthFillImage.color = _playerHealth.GetCurrentHealth() < _playerHealth.maxHealth * 0.1f
                    ? lowHealthColor
                    : _originalHealthColor;
        }

        private void UpdateStamina(float currentStamina, float maxStamina)
        {
            if (staminaBar is null) return;
            staminaBar.value = _player.currentStamina / _player.maxStamina;
            staminaBar.fillRect.gameObject.SetActive(_player.currentStamina >= 0);

            if (staminaFillImage is not null)
                staminaFillImage.color = currentStamina < maxStamina * 0.2f
                    ? lowStaminaColor
                    : _originalStaminaColor;
        }

        public void ShowDeathMessage()
        {
            if (deathText is null) return;
            deathText.SetActive(true);

            StartCoroutine(HideDeathMessageAfterDelay());
        }

        private IEnumerator HideDeathMessageAfterDelay()
        {
            yield return new WaitForSeconds(4.5f);
            deathText.SetActive(false);
        }
    }
}