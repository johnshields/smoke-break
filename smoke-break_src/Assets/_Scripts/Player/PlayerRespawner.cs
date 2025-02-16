using System.Collections;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerRespawner : MonoBehaviour
    {
        private static readonly int Fall = Animator.StringToHash("Fall");

        [Header("Respawn Settings")] [SerializeField]
        private float respawnDelay = 3f;

        [SerializeField] private Transform respawnPoint;
        [SerializeField] private HUDManager hudManager;

        private PlayerProfiler _player;
        private CombatProfiler _combatProfiler;
        private PistolProfiler _pistolProfiler;
        private Animator _animator;

        private void Awake()
        {
            _player = GetComponent<PlayerProfiler>();
            _combatProfiler = GetComponent<CombatProfiler>();
            _pistolProfiler = GetComponent<PistolProfiler>();
            _animator = GetComponent<Animator>();
        }

        public void InitRespawn()
        {
            _animator.SetTrigger(Fall);
            hudManager.GetComponent<HUDManager>().ShowDeathMessage();
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

            if (respawnPoint is not null)
                transform.position = respawnPoint.position;

            ResetPlayer();
        }

        private void ResetPlayer()
        {
            EnablePlayerActions();
            _player.currentHealth = _player.maxHealth;
            _animator.Rebind();
            _animator.Update(0f);
            Debug.Log("Kanta has respawned!");
        }
    }
}