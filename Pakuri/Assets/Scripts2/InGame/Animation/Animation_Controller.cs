using System.Collections;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class Animation_Controller : MonoBehaviour
    {
        private const string AttackTriggerName = "Attack";
        private const string HitTriggerName = "Hit";
        private const string DeathTriggerName = "Death";
        private const string AttackIndexParameterName = "AttackIndex";

        [SerializeField] private Animator animator;
        [SerializeField] private string idleState = "Anim_Rin_Idle";
        [SerializeField] private string deadState = "Anim_Rin_Dead_1";
        [SerializeField] private int attackStateCount = 3;

        private bool dead;
        private Coroutine deathFreezeRoutine;

        private void Awake()
        {
            ResolveAnimator();
            PlayIdle();
        }

        public void PlayRandomAttack()
        {
            if (dead)
            {
                return;
            }

            var resolvedAnimator = ResolveAnimator();
            if (resolvedAnimator == null)
            {
                return;
            }

            resolvedAnimator.speed = 1f;
            SetIntegerIfPresent(resolvedAnimator, AttackIndexParameterName, Random.Range(0, Mathf.Max(1, attackStateCount)));
            ResetTriggerIfPresent(resolvedAnimator, HitTriggerName);
            SetTriggerIfPresent(resolvedAnimator, AttackTriggerName);
        }

        public void PlayHit()
        {
            if (dead)
            {
                return;
            }

            var resolvedAnimator = ResolveAnimator();
            if (resolvedAnimator == null)
            {
                return;
            }

            resolvedAnimator.speed = 1f;
            ResetTriggerIfPresent(resolvedAnimator, AttackTriggerName);
            SetTriggerIfPresent(resolvedAnimator, HitTriggerName);
        }

        public void PlayDeath()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            var deathLength = ResolveClipLength(deadState);
            var resolvedAnimator = ResolveAnimator();
            if (resolvedAnimator != null)
            {
                resolvedAnimator.speed = 1f;
                ResetTriggerIfPresent(resolvedAnimator, AttackTriggerName);
                ResetTriggerIfPresent(resolvedAnimator, HitTriggerName);
                SetTriggerIfPresent(resolvedAnimator, DeathTriggerName);
            }

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
            if (ResolveAnimator() != null)
            {
                animator.speed = 1f;
            }

            PlayIdle();
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

        private static void SetTriggerIfPresent(Animator targetAnimator, string parameterName)
        {
            if (HasParameter(targetAnimator, parameterName, AnimatorControllerParameterType.Trigger))
            {
                targetAnimator.SetTrigger(parameterName);
            }
        }

        private static void ResetTriggerIfPresent(Animator targetAnimator, string parameterName)
        {
            if (HasParameter(targetAnimator, parameterName, AnimatorControllerParameterType.Trigger))
            {
                targetAnimator.ResetTrigger(parameterName);
            }
        }

        private static void SetIntegerIfPresent(Animator targetAnimator, string parameterName, int value)
        {
            if (HasParameter(targetAnimator, parameterName, AnimatorControllerParameterType.Int))
            {
                targetAnimator.SetInteger(parameterName, value);
            }
        }

        private static bool HasParameter(Animator targetAnimator, string parameterName, AnimatorControllerParameterType parameterType)
        {
            if (targetAnimator == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            var parameters = targetAnimator.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.type == parameterType
                    && string.Equals(parameter.name, parameterName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
