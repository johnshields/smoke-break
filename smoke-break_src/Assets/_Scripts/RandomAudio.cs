using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts
{
    public class RandomAudio : MonoBehaviour
    {
        [FormerlySerializedAs("pistolFireSounds")] [Header("Gunshot Sounds")] [SerializeField]
        private List<AudioClip> pistolSounds;

        [Header("Axe Sounds")] [SerializeField]
        private List<AudioClip> axeSounds;

        public AudioSource audioSource;

        private AudioClip GetRandomClip(string category)
        {
            return category switch
            {
                "Sounds/Pistol/" when pistolSounds.Count > 0 => pistolSounds[Random.Range(0, pistolSounds.Count)],
                "Sounds/Axe/" when axeSounds.Count > 0 => axeSounds[Random.Range(0, axeSounds.Count)],
                _ => null
            };
        }

        public void PlayRandomSound(string path, float vol)
        {
            audioSource.Stop();
            var randomClip = GetRandomClip(path);
            if (randomClip is not null)
            {
                audioSource.PlayOneShot(randomClip, vol);
            }
        }
    }
}