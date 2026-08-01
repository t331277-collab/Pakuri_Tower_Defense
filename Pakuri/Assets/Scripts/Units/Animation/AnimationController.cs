/*
 * 역할: 유닛 애니메이션 재생.
 * 책임: Animator Parameter를 확인하고 Idle·스킬·피격·패배·부활 전이를 재생한다.
 */

using System.Collections;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 유닛의 대기·스킬·피격·패배·부활 요청을 Animator 전이로 연결한다.
    public class AnimationController : MonoBehaviour
    {
        private const string AttackTriggerName = "Attack";
        private const string HitTriggerName = "Hit";
        private const string DeathTriggerName = "Death";
        private const string AttackIndexParameterName = "AttackIndex";

        private Animator animator;
        [SerializeField] private string idleState = "Anim_Rin_Idle";
        [SerializeField] private string deadState = "Anim_Rin_Dead_1";
        [SerializeField] private int attackStateCount = 3;

        private bool dead;
        private Coroutine deathFreezeRoutine;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            animator = GetComponent<Animator>();
            PlayIdle();
        }

        public void PlayRandomAttack()
        {
            if (dead)
            {
                return;
            }

            animator.speed = 1f;

            animator.SetInteger(AttackIndexParameterName, Random.Range(0, Mathf.Max(1, attackStateCount)));
            animator.ResetTrigger(HitTriggerName);
            animator.SetTrigger(AttackTriggerName);
        }

        public void PlayHit()
        {
            if (dead)
            {
                return;
            }

            animator.speed = 1f;
            animator.ResetTrigger(AttackTriggerName);
            animator.SetTrigger(HitTriggerName);
        }

        public void PlayDeath()
        {
            if (dead)
            {
                return;
            }

            dead = true;

            var deathLength = ResolveClipLength(deadState);
            animator.speed = 1f;
            animator.ResetTrigger(AttackTriggerName);
            animator.ResetTrigger(HitTriggerName);
            animator.SetTrigger(DeathTriggerName);

            if (deathFreezeRoutine != null)
            {
                StopCoroutine(deathFreezeRoutine);
            }

            deathFreezeRoutine = StartCoroutine(FreezeDeathOnLastFrame(deathLength));
        }

        public void PlayIdle()
        {
            if (dead)
            {
                return;
            }

            PlayState(idleState);
        }

        public void ReviveToIdle()
        {
            if (deathFreezeRoutine != null)
            {
                StopCoroutine(deathFreezeRoutine);
                deathFreezeRoutine = null;
            }

            dead = false;
            animator.speed = 1f;

            PlayIdle();
        }

        private void PlayState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            animator.speed = 1f;
            animator.Play(stateName, 0, 0f);
        }

        private IEnumerator FreezeDeathOnLastFrame(float deathLength)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, deathLength));
            if (string.IsNullOrWhiteSpace(deadState))
            {
                yield break;
            }

            animator.Play(deadState, 0, 0.999f);
            animator.Update(0f);
            animator.speed = 0f;
            deathFreezeRoutine = null;
        }

        private float ResolveClipLength(string stateName)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (clip.name == stateName)
                {
                    return clip.length;
                }
            }

            throw new System.InvalidOperationException($"AnimationClip not found: {stateName}");
        }
    }
}
