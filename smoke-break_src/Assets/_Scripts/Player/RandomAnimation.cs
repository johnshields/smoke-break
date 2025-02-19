using UnityEngine;

namespace _Scripts.Player
{
    public class RandomAnimation : MonoBehaviour
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorOverrideController overrideController;

        [SerializeField] private AnimationClip[] attackClips;

        private void Start()
        {
            animator.runtimeAnimatorController = overrideController;
        }

        public void PlayRandomAttack()
        {
            if (attackClips.Length == 0) return;

            AnimationClip randomClip = attackClips[Random.Range(0, attackClips.Length)];

            overrideController["Attack0"] = randomClip;

            animator.SetTrigger(Attack);
        }
    }
}