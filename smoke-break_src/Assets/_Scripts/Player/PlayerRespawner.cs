using System.Collections;
using System.IO;
using _Scripts.Managers;
using _Scripts.Objects;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerRespawner : MonoBehaviour
    {
        #region Fields

        private string _savePath;
        private static readonly int Fall = Animator.StringToHash("Fall");
        public bool isRespawning = false;

        #endregion

        #region Respawn Settings

        [Header("Respawn Settings")] [SerializeField]
        private float respawnDelay = 3f;

        [SerializeField] private Transform respawnPoint;
        [SerializeField] private HUDManager hudManager;

        #endregion

        #region Audio Settings

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip wilhelm;

        #endregion

        #region Dependencies

        private PlayerProfiler _player;
        private PlayerHealth _playerHealth;
        private CombatProfiler _combatProfiler;
        private PistolProfiler _pistolProfiler;
        private Animator _animator;
        private HUDManager _hudComponent;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

            _hudComponent = hudManager.GetComponent<HUDManager>();
            _player = GetComponent<PlayerProfiler>();
            _playerHealth = GetComponent<PlayerHealth>();
            _combatProfiler = GetComponent<CombatProfiler>();
            _pistolProfiler = GetComponent<PistolProfiler>();
            _animator = GetComponent<Animator>();
        }

        #endregion

        #region Respawn System

        public void InitRespawn()
        {
            PlayDeathSound();
            _animator.SetTrigger(Fall);
            _hudComponent.ShowDeathMessage();

            DisablePlayerActions();
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);

            SetRespawnPoint();

            if (respawnPoint != null)
                transform.position = respawnPoint.position;

            ResetPlayer();
        }

        private void SetRespawnPoint()
        {
            if (!File.Exists(_savePath)) return;

            var json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            var savedPosition = new Vector3(data.playerX, data.playerY, data.playerZ);
            respawnPoint.position = savedPosition;
            transform.position = respawnPoint.position;
        }

        private void ResetPlayer()
        {
            EnablePlayerActions();
            _playerHealth.SetCurrentHealth(_playerHealth.GetMaxHealth());
            _animator.Rebind();
            _animator.Update(0f);
            isRespawning = false;
            Debug.Log("Kanta has respawned!");
        }

        #endregion

        #region Player Actions

        private void DisablePlayerActions()
        {
            _combatProfiler.enabled = false;
            _pistolProfiler.enabled = false;
            _player.SetActions(false);
        }

        private void EnablePlayerActions()
        {
            _combatProfiler.enabled = true;
            _pistolProfiler.enabled = true;
            _player.SetActions(true);
        }

        #endregion

        #region Audio System

        private void PlayDeathSound()
        {
            if (audioSource != null && wilhelm != null)
                audioSource.PlayOneShot(wilhelm, 0.5f);
        }

        #endregion
    }
}