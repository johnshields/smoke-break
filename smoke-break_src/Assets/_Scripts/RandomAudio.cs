using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    public class RandoAudio : MonoBehaviour
    {
        [Header("Gunshot Sounds")] [SerializeField]
        private List<AudioClip> pistolFireSounds; // ✅ Holds all pistol gunshot sounds

        public AudioClip GetRandomClip(string category)
        {
            if (category == "Sounds/Pistol/" && pistolFireSounds.Count > 0)
            {
                return pistolFireSounds[Random.Range(0, pistolFireSounds.Count)];
            }

            return null;
        }
    }
}