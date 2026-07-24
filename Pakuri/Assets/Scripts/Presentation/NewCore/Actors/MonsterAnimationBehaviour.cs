using System.Collections;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class MonsterAnimationBehaviour : MonoBehaviour
    {
        private static readonly int Attack =
            Animator.StringToHash("Attack");
        private static readonly int Hit =
            Animator.StringToHash("Hit");
        private static readonly int Death =
            Animator.StringToHash("Death");
        private static readonly int AttackIndex =
            Animator.StringToHash("AttackIndex");

        [SerializeField] private string idleState = "Anim_Rin_Idle";
        [SerializeField] private string deadState = "Anim_Rin_Dead_1";
        [SerializeField] private int attackStateCount = 3;

        private Animator animator;
        private bool dead;
        private Coroutine freezeRoutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            PlayIdle();
        }

        public void PlayRandomAttack()
        {
            if (dead || animator == null)
            {
                return;
            }

            animator.speed = 1f;
            animator.SetInteger(
                AttackIndex,
                Random.Range(0, Mathf.Max(1, attackStateCount)));
            animator.ResetTrigger(Hit);
            animator.SetTrigger(Attack);
        }

        public void PlayHit()
        {
            if (dead || animator == null)
            {
                return;
            }

            animator.speed = 1f;
            animator.ResetTrigger(Attack);
            animator.SetTrigger(Hit);
        }

        public void PlayDeath()
        {
            if (dead || animator == null)
            {
                return;
            }

            dead = true;
            animator.speed = 1f;
            animator.ResetTrigger(Attack);
            animator.ResetTrigger(Hit);
            animator.SetTrigger(Death);
            if (freezeRoutine != null)
            {
                StopCoroutine(freezeRoutine);
            }

            freezeRoutine = StartCoroutine(
                FreezeDeath(ResolveClipLength()));
        }

        public void ReviveToIdle()
        {
            if (freezeRoutine != null)
            {
                StopCoroutine(freezeRoutine);
                freezeRoutine = null;
            }

            dead = false;
            if (animator != null)
            {
                animator.speed = 1f;
            }

            PlayIdle();
        }

        private void PlayIdle()
        {
            if (!dead
                && animator != null
                && !string.IsNullOrWhiteSpace(idleState))
            {
                animator.speed = 1f;
                animator.Play(idleState, 0, 0f);
            }
        }

        private IEnumerator FreezeDeath(float length)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, length));
            if (animator != null && !string.IsNullOrWhiteSpace(deadState))
            {
                animator.Play(deadState, 0, 1f);
                animator.speed = 0f;
            }
        }

        private float ResolveClipLength()
        {
            if (animator == null
                || animator.runtimeAnimatorController == null
                || string.IsNullOrWhiteSpace(deadState))
            {
                return 0.01f;
            }

            var clips = animator.runtimeAnimatorController.animationClips;
            for (var index = 0; index < clips.Length; index++)
            {
                if (clips[index] != null && clips[index].name == deadState)
                {
                    return clips[index].length;
                }
            }

            return 0.01f;
        }
    }
}
