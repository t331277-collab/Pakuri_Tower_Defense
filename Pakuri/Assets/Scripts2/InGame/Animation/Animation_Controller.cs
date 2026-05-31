using System.Collections;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class Animation_Controller : MonoBehaviour
    {
        private static readonly string[] AttackStates =
        {
            "Anim_Rin_Attack_1",
            "Anim_Rin_Attack_2",
            "Anim_Rin_Attack_3"
        };

        [SerializeField] private Animator animator;
        [SerializeField] private string idleState = "Anim_Rin_Idle";
        [SerializeField] private string hitState = "Anim_Rin_Hit";
        [SerializeField] private string deadState = "Anim_Rin_Dead_1";

        private float transientStateEndTime;
        private bool hasTransientState;
        private bool dead;
        private Coroutine deathFreezeRoutine;

        private void Awake()
        {
            ResolveAnimator();
            PlayIdle();
        }

        private void Update()
        {
            if (dead || !hasTransientState || Time.time < transientStateEndTime)
            {
                return;
            }

            hasTransientState = false;
            PlayIdle();
        }

        public void PlayRandomAttack()
        {
            if (dead)
            {
                return;
            }

            var index = Random.Range(0, AttackStates.Length);
            PlayTransient(AttackStates[index]);
        }

        public void PlayHit()
        {
            if (dead)
            {
                return;
            }

            PlayTransient(hitState);
        }

        public void PlayDeath()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            hasTransientState = false;
            var deathLength = ResolveClipLength(deadState);
            PlayState(deadState);
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
            hasTransientState = false;
            if (ResolveAnimator() != null)
            {
                animator.speed = 1f;
            }

            PlayIdle();
        }

        private void PlayTransient(string stateName)
        {
            PlayState(stateName);
            hasTransientState = true;
            transientStateEndTime = Time.time + ResolveClipLength(stateName);
        }

        private void PlayState(string stateName)
        {
            if (ResolveAnimator() == null || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            animator.speed = 1f;
            animator.Play(stateName, 0, 0f);
        }

        private IEnumerator FreezeDeathOnLastFrame(float deathLength)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, deathLength));
            if (ResolveAnimator() == null || string.IsNullOrWhiteSpace(deadState))
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
            var controller = ResolveAnimator() != null ? animator.runtimeAnimatorController : null;
            var clips = controller != null ? controller.animationClips : null;
            if (clips == null || clips.Length == 0)
            {
                return 0.67f;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (clip != null && string.Equals(clip.name, stateName, System.StringComparison.Ordinal))
                {
                    return Mathf.Max(0.01f, clip.length);
                }
            }

            return 0.67f;
        }

        private Animator ResolveAnimator()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            return animator;
        }
    }
}
