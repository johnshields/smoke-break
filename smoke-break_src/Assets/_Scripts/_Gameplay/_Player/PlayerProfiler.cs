using System.Collections;
using UnityEngine;

namespace _Scripts._Gameplay._Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerStamina), typeof(CombatProfiler))]
    public class PlayerProfiler : MonoBehaviour
    {
        private PlayerMovement _movement;
        private PlayerStamina _stamina;
        private CombatProfiler _combat;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _stamina = GetComponent<PlayerStamina>();
            _combat = GetComponent<CombatProfiler>();
        }

        public float GetMaxStamina() => _stamina.GetMaxStamina();
        public float GetCurrentStamina() => _stamina.GetCurrentStamina();

        public void SetMovement(bool value) => _movement.SetMovement(value);
        public void SetActions(bool active) => _movement.SetActions(active);

        public bool grounded => _movement.grounded;
        public int injuryThreshold => _combat.injuryThreshold;

        public IEnumerator StaggerEffect() => _combat.StaggerEffect();
    }
}
