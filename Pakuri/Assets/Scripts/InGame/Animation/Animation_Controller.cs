using System.Collections;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 몬스터 유닛의 Animator를 제어하는 컴포넌트.
     * 대기, 공격, 피격, 사망, 부활 애니메이션을 재생하고
     * 사망 애니메이션이 끝나면 마지막 프레임을 유지한다.
     */
    public sealed class Animation_Controller : MonoBehaviour
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

        /*
         * Unity가 컴포넌트를 초기화할 때 호출한다.
         * 같은 GameObject의 Animator를 저장하고 대기 애니메이션을 시작한다.
         */
        private void Awake()
        {
            animator = GetComponent<Animator>();
            PlayIdle();
        }

        /*
         * 설정된 공격 개수 안에서 무작위 공격 인덱스를 선택하고
         * Animator의 공격 Trigger를 실행한다.
         */
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

        /*
         * 진행 중인 공격 Trigger를 해제하고 피격 애니메이션을 실행한다.
         * 사망 상태에서는 실행하지 않는다.
         */
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

        /*
         * 사망 상태를 기록하고 사망 애니메이션을 실행한다.
         * 클립 재생 시간이 지나면 마지막 프레임을 유지하는 코루틴을 시작한다.
         */
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

        /*
         * 설정된 대기 상태를 처음부터 재생한다.
         * 사망 상태에서는 사망 애니메이션을 보존하기 위해 실행하지 않는다.
         */
        public void PlayIdle()
        {
            if (dead)
            {
                return;
            }

            PlayState(idleState);
        }

        /*
         * 사망 프레임 고정 코루틴을 중단하고 사망 상태를 해제한다.
         * Animator 속도를 복구한 뒤 대기 애니메이션으로 돌아간다.
         */
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

        /*
         * 전달받은 Animator 상태를 0번 레이어의 첫 프레임부터 재생한다.
         * 상태 이름이 비어 있으면 실행하지 않는다.
         */
        private void PlayState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            animator.speed = 1f;
            animator.Play(stateName, 0, 0f);
        }

        /*
         * 사망 클립 길이만큼 기다린 뒤 사망 상태의 마지막 프레임으로 이동한다.
         * Animator 속도를 0으로 설정해 해당 포즈를 유지한다.
         */
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

        /*
         * RuntimeAnimatorController에서 지정된 이름의 AnimationClip 길이를 찾는다.
         */
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
