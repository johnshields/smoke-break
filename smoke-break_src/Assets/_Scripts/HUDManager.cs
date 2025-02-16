using _Scripts.Player;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace _Scripts
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance;

        [Header("HUD Elements")] [SerializeField]
        private TextMeshProUGUI bulletCounter;

        [SerializeField] private Slider healthBar;
        [SerializeField] private GameObject deathText;

        [Header("BoostBar Elements")] [SerializeField]
        private Slider boostBar;

        [SerializeField] private Image boostFill;
        [SerializeField] private Color flashColor = Color.red;
        private Color _originalColor;

        private PistolProfiler _pistol;
        private PlayerProfiler _player;

        private void Start()
        {
            if (Instance == null) Instance = this;

            _pistol = FindObjectOfType<PistolProfiler>();
            _player = FindObjectOfType<PlayerProfiler>();
            _originalColor = boostFill.color;

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
            bulletCounter.text = $"{_pistol.currentClipAmmo}/{_pistol.storedAmmo}";
        }

        private void UpdateHealth()
        {
            healthBar.value = _player.GetCurrentHealth();
            healthBar.fillRect.gameObject.SetActive(!(_player.GetCurrentHealth() <= 0));
        }

        public void ShowDeathMessage()
        {
            deathText.SetActive(true);

            StartCoroutine(HideDeathMessageAfterDelay());
        }

        private IEnumerator HideDeathMessageAfterDelay()
        {
            yield return new WaitForSeconds(4.5f);
            deathText.SetActive(false);
        }

        public void UpdateBoostBar(float value)
        {
            if (boostBar != null) boostBar.value = value;
        }

        public void StartFlashing()
        {
            StopCoroutine(nameof(FlashBoostFill));
            StartCoroutine(nameof(FlashBoostFill));
        }

        public void FadeBoostUI(bool fadeIn)
        {
            StopCoroutine(nameof(FadeBoostFill));
            StartCoroutine(FadeBoostFill(fadeIn ? 1f : 0f));
        }

        private IEnumerator FlashBoostFill()
        {
            while (boostBar.value <= 0.15f)
            {
                boostFill.color = flashColor;
                yield return new WaitForSeconds(0.1f);
                boostFill.color = _originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator FadeBoostFill(float targetAlpha)
        {
            const float fadeDuration = 0.5f;
            var startAlpha = boostFill.color.a;
            var elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                Color newColor = boostFill.color;
                newColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                boostFill.color = newColor;
                yield return null;
            }
        }
    }
}