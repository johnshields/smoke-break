using System.Collections.Generic;
using UnityEngine;

namespace _Scripts._Systems.Managers
{
    public class RandomAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private readonly Dictionary<string, AudioClip[]> _clipCache = new();

        private AudioClip GetRandomClip(string path)
        {
            if (!_clipCache.TryGetValue(path, out var clips))
            {
                clips = Resources.LoadAll<AudioClip>(path);
                _clipCache[path] = clips;
            }

            if (clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
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