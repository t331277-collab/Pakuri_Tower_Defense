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
        private bool frozen;
        private bool missingAnimatorReported;
        private bool deathClipDiagnosticReported;
        private Coroutine freezeRoutine;

        public bool IsDead => dead;

        public bool IsDeathFrozen => frozen;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                ReportMissingAnimator();
            }
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
            if (dead)
            {
                return;
            }

            dead = true;
            frozen = false;
            if (animator == null)
            {
                ReportMissingAnimator();
                return;
            }

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
            frozen = false;
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
                animator.Play(deadState, 0, 0.999f);
                animator.Update(0f);
                animator.speed = 0f;
                frozen = true;
            }
        }

        private float ResolveClipLength()
        {
            if (animator == null)
            {
                ReportMissingAnimator();
                return 0.01f;
            }
            if (animator.runtimeAnimatorController == null)
            {
                ReportDeathClipDiagnostic(
                    "has no RuntimeAnimatorController; "
                    + "death freeze uses the bounded 0.01 second fallback.");
                return 0.01f;
            }
            if (string.IsNullOrWhiteSpace(deadState))
            {
                ReportDeathClipDiagnostic(
                    "has no authored death state; "
                    + "death freeze uses the bounded 0.01 second fallback.");
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

            ReportDeathClipDiagnostic(
                $"cannot find death clip '{deadState}'; "
                + "death freeze uses the bounded 0.01 second fallback.");
            return 0.01f;
        }

        private void ReportMissingAnimator()
        {
            if (missingAnimatorReported)
            {
                return;
            }

            missingAnimatorReported = true;
            Debug.LogError(
                $"{name}: MonsterAnimationBehaviour requires an Animator.",
                this);
        }

        private void ReportDeathClipDiagnostic(string message)
        {
            if (deathClipDiagnosticReported)
            {
                return;
            }

            deathClipDiagnosticReported = true;
            Debug.LogWarning($"{name}: {message}", this);
        }
    }
}
