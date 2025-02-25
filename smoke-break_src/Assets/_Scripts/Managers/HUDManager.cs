using System.Collections;
using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Managers
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Pistol HUD Elements")] [SerializeField]
        private TextMeshProUGUI bulletCounter;

        [Header("Health HUD Elements")] [SerializeField]
        private Slider healthBar;

        [SerializeField] private Image healthFill;
        [SerializeField] private Color lowHealthColor = Color.red;
        private Color _originalHealthColor;
        [SerializeField] private Image injuryOverlay;
        [SerializeField] private GameObject deathText;

        [Header("Stamina HUD Elements")] [SerializeField]
        private Slider staminaBar;

        [SerializeField] private Image staminaFill;
        [SerializeField] private Color lowStaminaColor = Color.red;
        private Color _originalStaminaColor;

        [Header("Boost Pack HUD Elements")] [SerializeField]
        private Slider boostBar;

        [SerializeField] private Image boostFill;
        [SerializeField] private Color lowBoostColor = Color.red;
        private Color _originalBoostColor;

        private PistolProfiler _pistol;
        private PlayerProfiler _player;
        private PlayerHealth _playerHealth;
        private BoostPack _boostPack;

        private void Start()
        {
            _pistol = FindObjectOfType<PistolProfiler>();
            _player = FindObjectOfType<PlayerProfiler>();
            _playerHealth = FindObjectOfType<PlayerHealth>();
            _boostPack = FindObjectOfType<BoostPack>();

            _originalStaminaColor = staminaFill?.color ?? Color.white;
            _originalHealthColor = healthFill?.color ?? Color.white;
            _originalBoostColor = boostFill?.color ?? Color.white;

            UpdateAmmo();
            UpdateHealth();
            UpdateStamina();
        }

        private void Update()
        {
            UpdateAmmo();
            UpdateHealth();
            UpdateStamina();
        }

        private void UpdateAmmo()
        {
            if (bulletCounter is null) return;
            bulletCounter.text = $"{_pistol.currentClip}/{_pistol.storedAmmo}";
        }

        private void UpdateHealth()
        {
            if (healthBar is null) return;
            healthBar.value = _playerHealth.GetCurrentHealth();
            healthBar.fillRect.gameObject.SetActive(_playerHealth.GetCurrentHealth() > 0);

            if (healthFill is not null)
                healthFill.color = _playerHealth.GetCurrentHealth() < _playerHealth.maxHealth * 0.2f
                    ? lowHealthColor
                    : _originalHealthColor;

            var isLowHealth = _playerHealth.currentHealth <= 10;
            var targetAlpha = isLowHealth ? 0.5f : 0f;

            injuryOverlay.color = new Color(lowHealthColor.r, lowHealthColor.g, lowHealthColor.b,
                Mathf.Lerp(injuryOverlay.color.a, targetAlpha, Time.deltaTime * 5f));
        }

        private void UpdateStamina()
        {
            if (staminaBar is null) return;
            staminaBar.value = _player.currentStamina / _player.maxStamina;
            staminaBar.fillRect.gameObject.SetActive(_player.currentStamina >= 0);

            if (staminaFill is not null)
                staminaFill.color = _player.currentStamina < _player.maxStamina * 0.1f
                    ? lowStaminaColor
                    : _originalStaminaColor;
        }

        public IEnumerator UpdateBoostBar()
        {
            if (boostBar is null || boostFill is null) yield break;

            var originalColor = boostFill.color;
            var isFlashing = false;

            StartCoroutine(FadeBoostBar(1f));

            // First phase: Draining the boost bar over the cooldown period.
            for (float elapsedTime = 0; elapsedTime < _boostPack.boostCooldown; elapsedTime += Time.deltaTime)
            {
                // Calculate the fill amount based on elapsed time and cooldown duration.
                var fillAmount = Mathf.Clamp01(1f - (elapsedTime / _boostPack.boostCooldown));

                // Update the boost bar's fill value.
                boostBar.value = fillAmount;

                // If the fill amount is below 15% and the flashing effect hasn't started, trigger it.
                if (fillAmount <= 0.15f && !isFlashing)
                {
                    isFlashing = true;
                    StartCoroutine(FlashBoostBar());
                }

                // Wait until the next frame before continuing the loop.
                yield return null;
            }

            // Small delay to visually indicate the boost bar is completely drained.
            yield return new WaitForSeconds(0.5f);

            // Restore the original color of the boost fill after flashing.
            boostFill.color = originalColor;

            // Allow the boost to be used again.
            _boostPack.canBoost = true;

            // Second phase: Refilling the boost bar over the cooldown duration.
            for (float elapsedTime = 0; elapsedTime < _boostPack.boostCooldown; elapsedTime += Time.deltaTime)
            {
                // Gradually increase the boost bar's fill value.
                boostBar.value = Mathf.Clamp01(elapsedTime / _boostPack.boostCooldown);

                // Wait until the next frame before continuing the loop.
                yield return null;
            }

            boostBar.value = 1f;

            StartCoroutine(FadeBoostBar(0f));
        }

        private IEnumerator FlashBoostBar()
        {
            while (boostBar.value <= 0.15f)
            {
                boostFill.color = (boostFill.color == lowBoostColor) ? _originalBoostColor : lowBoostColor;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.1f);
            boostFill.color = _originalBoostColor;
            _boostPack.canBoost = true;
        }

        private IEnumerator FadeBoostBar(float targetAlpha)
        {
            var startAlpha = boostFill.color.a;
            const float fadeDuration = 0.5f;
            var elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                var newColor = boostFill.color;
                newColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                boostFill.color = newColor;
                yield return null;
            }
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