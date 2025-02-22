using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerEvents : MonoBehaviour
    {
        [SerializeField] private RandomAudio randomAudio;

        private void Steps(float vol)
        {
            randomAudio?.PlayRandomSound("steps", vol);
        }

        private void Jumps(float vol)
        {
            randomAudio?.PlayRandomSound("jumps", vol);
        }

        private void Dodge(float vol)
        {
            randomAudio?.PlayRandomSound("dodge", vol);
        }
    }
}