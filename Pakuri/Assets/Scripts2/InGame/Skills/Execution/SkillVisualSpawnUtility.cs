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
    }
}
