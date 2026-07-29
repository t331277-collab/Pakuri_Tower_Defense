/*
 * 역할: 유닛 애니메이션 재생.
 * 책임: Animator Parameter를 확인하고 Idle·스킬·피격·패배·부활 전이를 재생한다.
 */

using System.Collections;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>AnimationController</c>가 담당하는 입력 또는 표시 흐름을 조정하고 관련 런타임 상태를 갱신한다.</summary>
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

        /// <summary>Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.</summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
            PlayIdle();
        }

        /// <summary><c>PlayRandomAttack</c> 작업을 수행한다.</summary>
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

        /// <summary><c>PlayHit</c> 작업을 수행한다.</summary>
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

        /// <summary><c>PlayDeath</c> 작업을 수행한다.</summary>
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

        /// <summary><c>PlayIdle</c> 작업을 수행한다.</summary>
        public void PlayIdle()
        {
            if (dead)
            {
                return;
            }

            PlayState(idleState);
        }

        /// <summary><c>ReviveToIdle</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 <c>stateName</c> 값을 사용해 <c>PlayState</c> 작업을 수행한다.</summary>
        private void PlayState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            animator.speed = 1f;
            animator.Play(stateName, 0, 0f);
        }

        /// <summary>전달된 <c>deathLength</c> 값을 사용해 <c>FreezeDeathOnLastFrame</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>stateName</c> 값을 사용해 <c>ClipLength</c>를 결정한다.</summary>
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
