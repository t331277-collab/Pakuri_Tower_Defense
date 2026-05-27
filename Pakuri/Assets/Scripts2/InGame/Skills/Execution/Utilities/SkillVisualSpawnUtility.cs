using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillVisualSpawnUtility
    {
        public static GameObject SpawnTransient(
            EffectManager effects,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            float durationSeconds)
        {
            if (effects == null || prefab == null)
            {
                return null;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, position, rotation);
            if (instance != null)
            {
                Object.Destroy(instance, Mathf.Max(0.01f, durationSeconds));
            }

            return instance;
        }

        public static GameObject SpawnAttached(
            EffectManager effects,
            GameObject prefab,
            Transform target,
            float durationSeconds,
            Vector3 offset)
        {
            if (effects == null || prefab == null || target == null)
            {
                return null;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, target.position, Quaternion.identity);
            if (instance == null)
            {
                return null;
            }

            var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(target, Mathf.Max(0.1f, durationSeconds), offset);
            return instance;
        }

        public static float ResolveVisualLifetime(GameObject instance, float minimumLifetimeSeconds = 1f)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            var animationLength = ResolveAnimationLength(instance);
            return Mathf.Max(minimum, animationLength > 0f ? animationLength : 1f);
        }

        private static float ResolveAnimationLength(GameObject instance)
        {
            if (instance == null)
            {
                return 0f;
            }

            var maxLength = 0f;
            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var controller = animators[i] != null ? animators[i].runtimeAnimatorController : null;
                var clips = controller != null ? controller.animationClips : null;
                if (clips == null)
                {
                    continue;
                }

                for (var j = 0; j < clips.Length; j++)
                {
                    var clip = clips[j];
                    if (clip != null)
                    {
                        maxLength = Mathf.Max(maxLength, clip.length);
                    }
                }
            }

            var legacyAnimations = instance.GetComponentsInChildren<Animation>(true);
            for (var i = 0; i < legacyAnimations.Length; i++)
            {
                var legacyAnimation = legacyAnimations[i];
                if (legacyAnimation == null)
                {
                    continue;
                }

                foreach (AnimationState state in legacyAnimation)
                {
                    if (state != null && state.clip != null)
                    {
                        maxLength = Mathf.Max(maxLength, state.clip.length);
                    }
                }
            }

            return maxLength;
        }
    }
}
