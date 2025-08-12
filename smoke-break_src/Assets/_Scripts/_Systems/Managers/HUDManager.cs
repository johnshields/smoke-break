using System.Collections;
using _Scripts._Gameplay._Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Systems.Managers
{
    public class HUDManager : MonoBehaviour
    {
        #region Fields

        #region Pistol HUD

        [Header("Pistol HUD Elements")]
        [SerializeField] private TextMeshProUGUI bulletCounter;

        #endregion

        #region Health HUD

        [Header("Health HUD Elements")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color lowHealthColor = Color.red;
        private Color _originalHealthColor;
        [SerializeField] private Image injuryOverlay;
        [SerializeField] private GameObject deathText;
        [SerializeField] private Image heartIcon;

        #endregion

        #region Stamina HUD

        [Header("Stamina HUD Elements")]
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Color lowStaminaColor = Color.red;
        [SerializeField] private Image staminaIcon;
        private Color _originalStaminaColor;

        #endregion

        #region Boost HUD

        [Header("Boost Pack HUD Elements")]
        [SerializeField] private Slider boostBar;
        [SerializeField] private Image boostFill;
        [SerializeField] private Color lowBoostColor = Color.red;
        private Color _originalBoostColor;
        private float _boostCooldown;

        #endregion

        #region Player References

        private PistolProfiler _pistol;
        private PlayerProfiler _player;
        private PlayerHealth _playerHealth;
        private BoostPack _boostPack;

        #endregion

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _pistol = FindFirstObjectByType<PistolProfiler>();
            _player = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _boostPack = FindFirstObjectByType<BoostPack>();
            _boostCooldown = _boostPack.GetBoostCooldown();

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
            UpdateHeartIcon();
            UpdateStamina();
            UpdateStaminaIcon();
        }

        #endregion

        #region Ammo System

        private void UpdateAmmo()
        {
            if (bulletCounter is null) return;
            bulletCounter.text = $"{_pistol.currentClip}/{_pistol.storedAmmo}";
        }

        #endregion

        #region Health System

        private void UpdateHealth()
        {
            if (healthBar is null) return;
            healthBar.value = _playerHealth.GetCurrentHealth();
            healthBar.fillRect.gameObject.SetActive(_playerHealth.GetCurrentHealth() > 0);

            if (healthFill is not null)
            {
                var healthPercentage = (float)_playerHealth.GetCurrentHealth() / _playerHealth.GetMaxHealth();
                healthFill.color = Color.Lerp(healthFill.color,
                    healthPercentage < 0.3f ? lowHealthColor : _originalHealthColor, Time.deltaTime * 5f);
            }

            var isLowHealth = _playerHealth.GetCurrentHealth() <= 10;
            var targetAlpha = isLowHealth ? 0.5f : 0f;

            injuryOverlay.color = new Color(lowHealthColor.r, lowHealthColor.g, lowHealthColor.b,
                Mathf.Lerp(injuryOverlay.color.a, targetAlpha, Time.deltaTime * 5f));
        }

        private void UpdateHeartIcon()
        {
            var healthPercentage = (float)_playerHealth.GetCurrentHealth() / _playerHealth.GetMaxHealth();

            if (_playerHealth.GetCurrentHealth() <= 0)
            {
                var popEffect = 1.0f + Mathf.Sin(Time.time * 15f) * 0.2f;
                heartIcon.transform.localScale = Vector3.one * popEffect;
                return;
            }

            heartIcon.transform.localScale = healthPercentage <= 0.3f
                ? Vector3.one * (1.0f + Mathf.Sin(Time.time * 5f) * 0.1f)
                : Vector3.one;
        }

        #endregion

        #region Stamina System

        private void UpdateStamina()
        {
            var currentStamina = _player.GetCurrentStamina();
            var maxStamina = _player.GetMaxStamina();

            if (staminaBar is null) return;
            staminaBar.value = currentStamina / maxStamina;
            staminaBar.fillRect.gameObject.SetActive(currentStamina >= 0);

            if (staminaFill is null) return;
            var staminaPercentage = (float)currentStamina / maxStamina;
            staminaFill.color = Color.Lerp(staminaFill.color,
                staminaPercentage < 0.2f ? lowStaminaColor : _originalStaminaColor, Time.deltaTime * 5f);
        }

        private void UpdateStaminaIcon()
        {
            var staminaPercentage = (float)_player.GetCurrentStamina() / _player.GetMaxStamina();

            staminaIcon.transform.localScale = staminaPercentage < 0.2f
                ? Vector3.one * (1.0f + Mathf.Sin(Time.time * 5f) * 0.1f)
                : Vector3.one;
        }

        #endregion

        #region Boost System

        public IEnumerator UpdateBoostBar()
        {
            if (boostBar is null || boostFill is null) yield break;

            StartCoroutine(FadeBoostBar(1f));

            yield return DrainBoostBar();
            yield return new WaitForSeconds(0.25f);

            _boostPack.canBoost = true;
            boostBar.value = 1f;
        }

        private IEnumerator DrainBoostBar()
        {
            var originalColor = boostFill.color;
            var isFlashing = false;

            for (float elapsedTime = 0; elapsedTime < _boostCooldown; elapsedTime += Time.deltaTime)
            {
                var fillAmount = 1f - (elapsedTime / _boostCooldown);
                boostBar.value = fillAmount;

                if (fillAmount <= 0.15f && !isFlashing)
                {
                    isFlashing = true;
                    yield return StartCoroutine(FlashBoostBar(originalColor));
                    StartCoroutine(FadeBoostBar(0f));
                }

                yield return null;
            }

            boostBar.value = 0f;
        }

        private IEnumerator FlashBoostBar(Color originalColor)
        {
            var flashColor = lowBoostColor;
            const int flashCount = 6;
            const float flashDuration = 0.1f;

            for (var i = 0; i < flashCount; i++)
            {
                boostFill.color = (boostFill.color == flashColor) ? originalColor : flashColor;
                yield return new WaitForSeconds(flashDuration);
            }

            boostFill.color = originalColor;
        }

        private IEnumerator FadeBoostBar(float targetAlpha)
        {
            var startAlpha = boostFill.color.a;
            const float fadeDuration = 0.1f;
            var elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                var newColor = boostFill.color;
                newColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                boostFill.color = newColor;
                yield return null;
            }

            boostFill.color = new Color(boostFill.color.r, boostFill.color.g, boostFill.color.b, targetAlpha);
        }

        #endregion

        #region Death Message

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

        #endregion
    }
}