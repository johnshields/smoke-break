using _Scripts._Gameplay._Player;
using UnityEngine;

namespace _Scripts.Utils
{
    // Cached player component lookups shared across UI, managers, and debug tools.
    public static class PlayerRefs
    {
        private static PlayerProfiler _profiler;
        private static PlayerHealth _health;
        private static PistolProfiler _pistol;

        public static PlayerProfiler Profiler =>
            _profiler != null ? _profiler : _profiler = Object.FindFirstObjectByType<PlayerProfiler>();

        public static PlayerHealth Health =>
            _health != null ? _health : _health = Object.FindFirstObjectByType<PlayerHealth>();

        public static PistolProfiler Pistol =>
            _pistol != null ? _pistol : _pistol = Object.FindFirstObjectByType<PistolProfiler>();

        // Call on scene load to clear stale references.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _profiler = null;
            _health = null;
            _pistol = null;
        }
    }
}
