using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts
{
    public class RandomAudio : MonoBehaviour
    {
        public List<AudioClip> clipList;
        public AudioClip[] clipListArray;

        public AudioSource audioSource;

        private AudioClip GetRandomClip(string path)
        {
            clipListArray = Resources.LoadAll<AudioClip>(path);

            clipList = clipListArray.ToList();
            var clipToPlay = clipList[Random.Range(0, clipList.Count)];

            return clipToPlay;
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