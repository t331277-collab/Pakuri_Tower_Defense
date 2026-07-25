using System.Collections;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* Monster Model의 scene 결합과 공격·피격·사망 animation 생명주기를 소유한다. */
namespace Pakuri.NewCore.Units.Actors
{
    public sealed class MonsterActor : UnitActor
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

        public MonsterModel Monster => Model as MonsterModel;

        public bool IsDead => dead;

        public bool IsDeathFrozen => frozen;

        /* 같은 GameObject의 Animator를 연결하고 초기 idle 상태를 재생한다. */
        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                ReportMissingAnimator();
            }

            PlayIdle();
        }

        /* Model 생존 전환을 collider와 사망·부활 animation에 반영한다. */
        public override void SyncFromModel()
        {
            base.SyncFromModel();
            if (Model == null)
            {
                return;
            }

            if (!Model.IsAlive && !dead)
            {
                SetColliders(false);
                PlayDeath();
            }
            else if (Model.IsAlive && dead)
            {
                SetColliders(true);
                ReviveToIdle();
            }
        }

        /* 생존 Monster의 authored 공격 animation 중 하나를 재생한다. */
        public void PlayAttack()
        {
            PlayRandomAttack();
        }

        /* 생존 Monster의 피격 animation을 재생한다. */
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

        /* 공격 상태 수 범위에서 무작위 공격 index를 정해 trigger한다. */
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

        /* 사망 trigger를 재생하고 authored clip 끝 frame 고정을 예약한다. */
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

        /* 사망 coroutine을 중단하고 Monster를 idle animation으로 복구한다. */
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

        /* 생존 중 authored idle state의 첫 frame부터 재생한다. */
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

        /* 사망 clip 길이 뒤 authored dead state의 마지막 frame을 고정한다. */
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

        /* RuntimeAnimatorController에서 authored 사망 clip 길이를 찾는다. */
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

            AnimationClip[] clips =
                animator.runtimeAnimatorController.animationClips;
            for (int index = 0; index < clips.Length; index++)
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

        /* prefab의 Collider2D 전체를 생존 상태에 맞춰 전환한다. */
        private void SetColliders(bool enabled)
        {
            Collider2D[] colliders =
                GetComponentsInChildren<Collider2D>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = enabled;
            }
        }

        /* Animator 누락 오류를 Actor별 한 번만 Console에 기록한다. */
        private void ReportMissingAnimator()
        {
            if (missingAnimatorReported)
            {
                return;
            }

            missingAnimatorReported = true;
            Debug.LogError(
                $"{name}: MonsterActor requires an Animator.",
                this);
        }

        /* 사망 clip 구성 경고를 Actor별 한 번만 Console에 기록한다. */
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
