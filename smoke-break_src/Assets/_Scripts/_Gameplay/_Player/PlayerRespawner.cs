using System.Collections;
using _Scripts._Systems.Managers;
using _Scripts.Utils;
using UnityEngine;

namespace _Scripts._Gameplay._Player
{
    public class PlayerRespawner : MonoBehaviour
    {
        #region Fields

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

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
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
            _animator.SetTrigger(AnimHashes.Fall);
            hudManager.ShowDeathMessage();

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
            var data = SaveManager.LoadFromDisk();
            if (data == null) return;

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