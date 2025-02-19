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
        [SerializeField] private GameObject deathText;

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
    }
}