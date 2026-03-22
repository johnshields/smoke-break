using UnityEngine;

namespace _Scripts._Systems.Utils
{
    // Centralised animator parameter hashes to avoid duplicate StringToHash calls.
    public static class AnimHashes
    {
        // Player movement
        public static readonly int Grounded = Animator.StringToHash("Grounded");
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int DodgeBack = Animator.StringToHash("DodgeBack");
        public static readonly int DodgeRoll = Animator.StringToHash("DodgeRoll");
        public static readonly int Boost = Animator.StringToHash("Boost");

        // Player combat
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Shoot = Animator.StringToHash("Shoot");
        public static readonly int Injured = Animator.StringToHash("Injured");
        public static readonly int Stagger = Animator.StringToHash("Stagger");
        public static readonly int Fall = Animator.StringToHash("Fall");

        // Enemy
        public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    }
}
