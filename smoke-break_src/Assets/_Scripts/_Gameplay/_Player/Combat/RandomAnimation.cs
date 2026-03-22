using _Scripts.Utils;
using UnityEngine;

namespace _Scripts._Gameplay._Player
{
    public class RandomAnimation : MonoBehaviour
    {
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

            animator.SetTrigger(AnimHashes.Attack);
        }
    }
}