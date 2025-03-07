using System.Collections;
using System.IO;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerRespawner : MonoBehaviour
    {
        private string _savePath;
        private static readonly int Fall = Animator.StringToHash("Fall");

        [Header("Respawn Settings")] [SerializeField]
        private float respawnDelay = 3f;

        [SerializeField] private Transform respawnPoint;
        [SerializeField] private HUDManager hudManager;

        public bool isRespawning = false;
        private PlayerProfiler _player;
        private PlayerHealth _playerHealth;
        private CombatProfiler _combatProfiler;
        private PistolProfiler _pistolProfiler;
        private Animator _animator;
        private HUDManager _component;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip wilhelm;

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

            _component = hudManager.GetComponent<HUDManager>();
            _player = GetComponent<PlayerProfiler>();
            _playerHealth = GetComponent<PlayerHealth>();
            _combatProfiler = GetComponent<CombatProfiler>();
            _pistolProfiler = GetComponent<PistolProfiler>();
            _animator = GetComponent<Animator>();
        }

        public void InitRespawn()
        {
            if (audioSource is not null && wilhelm is not null)
                audioSource.PlayOneShot(wilhelm, .5f);

            _animator.SetTrigger(Fall);
            _component.ShowDeathMessage();
            DisablePlayerActions();
            StartCoroutine(Respawn());
        }

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

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);

            GetRespawnPoint();

            if (respawnPoint is not null)
                transform.position = respawnPoint.position;

            ResetPlayer();
        }

        private void GetRespawnPoint()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                var savedPosition = new Vector3(data.playerX, data.playerY, data.playerZ);
                respawnPoint.transform.position = savedPosition;

                transform.position = respawnPoint.position;
            }
        }

        private void ResetPlayer()
        {
            EnablePlayerActions();
            _playerHealth.SetCurrentHealth(_playerHealth.GetMaxHealth());
            _animator.Rebind();
            _animator.Update(0f);
            isRespawning = false;
            print("Kanta has respawned!");
        }
    }
}