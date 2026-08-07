/*
 * 역할: 전투 결과와 분리하여 효과 GameObject를 생성·부착·추적·제거·일괄 정리한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<StatusRuntimeInstance, GameObject> statusEffectVisuals = new Dictionary<StatusRuntimeInstance, GameObject>();
        private readonly HashSet<GameObject> targetAttachedEffects = new HashSet<GameObject>();
        private readonly HashSet<GameObject> activeEffects = new HashSet<GameObject>();
        private readonly List<StatusRuntimeInstance> staleStatuses = new List<StatusRuntimeInstance>();
        private readonly Dictionary<GameObject, Vector3> prefabBaseScales = new Dictionary<GameObject, Vector3>();
        private readonly RuntimeObjectPool<EffectPoolKey> effectPool = new RuntimeObjectPool<EffectPoolKey>();

        /// 이펙트를 넣을 오브젝트를 생성한다.

        private GameObject CreateObject(
            EffectCreateRequest request,
            Vector3 position)
        {
            if (request.Visual != null && request.Visual.HasVisual())
            {
                return CreateRuntimeObject(request.ObjectName, position, request.Rotation);
            }

            if (request.Prefab != null)
            {
                var instance = Instantiate(request.Prefab, position, request.Rotation, runtimeSkillRoot);
                prefabBaseScales[instance] = instance.transform.localScale;
                return instance;
            }

            if (request.CreateEmptyActor)
            {
                return CreateRuntimeObject(request.ObjectName, position, request.Rotation);
            }

            return null;
        }

        private void PrepareObject(GameObject instance, EffectCreateRequest request)
        {
            instance.transform.SetParent(runtimeSkillRoot, false);
            instance.transform.SetPositionAndRotation(request.Position, request.Rotation);

            if (request.Prefab != null
                && prefabBaseScales.TryGetValue(instance, out var baseScale))
            {
                instance.transform.localScale = baseScale;
            }
            else
            {
                instance.transform.localScale = Vector3.one;
            }

            ResetComponents(instance);

            if (request.Visual != null && request.Visual.HasVisual())
            {
                EffectVisualBuilder.Configure(instance, request.Visual, request.HitboxIsTrigger, request.IncludeHitbox);
            }
        }

        private static void ResetComponents(GameObject instance)
        {
            var colliders = instance.GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = true;
                }
            }

            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                {
                    animators[i].enabled = true;
                    animators[i].Rebind();
                    animators[i].Update(0f);
                }
            }
        }

        /// 스킬 적용 대상의 자식으로 이펙트를 적용시켜 이펙트가 타겟에 붙는다.

        private static void AttachVisualToTarget(GameObject instance, Transform targetTransform)
        {
            instance.transform.SetParent(targetTransform, true);
        }

        private GameObject CreateRuntimeObject(
            string objectName,
            Vector3 position,
            Quaternion rotation)
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(runtimeSkillRoot, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /// 이펙트 생성

        public GameObject CreateEffect(EffectCreateRequest request)
        {
            GameObject instance = null;
            var created = false;
            if (request.PersistentStatus != null)
            {
                statusEffectVisuals.TryGetValue(request.PersistentStatus, out instance);
                if (instance == null)
                {
                    statusEffectVisuals.Remove(request.PersistentStatus);
                }
            }

            if (instance == null)
            {
                var key = new EffectPoolKey(request);
                instance = effectPool.Get(
                    key,
                    () => CreateObject(request, request.Position));
                if (instance == null)
                {
                    return null;
                }

                PrepareObject(instance, request);
                created = true;
                activeEffects.Add(instance);

                if (request.PersistentStatus != null)
                {
                    statusEffectVisuals[request.PersistentStatus] = instance;
                }

                if (request.TargetTransform != null)
                {
                    AttachVisualToTarget(instance, request.TargetTransform);
                    targetAttachedEffects.RemoveWhere(effectObject => effectObject == null);
                    targetAttachedEffects.Add(instance);
                }
            }

            if (!created && request.TargetTransform != null)
            {
                AttachVisualToTarget(instance, request.TargetTransform);
            }
            return instance;
        }

        public void RemoveEffect(
            GameObject instance = null,
            StatusRuntimeInstance status = null)
        {
            if (status != null)
            {
                if (instance == null)
                {
                    statusEffectVisuals.TryGetValue(status, out instance);
                }

                statusEffectVisuals.Remove(status);
            }

            if (instance != null)
            {
                staleStatuses.Clear();
                foreach (var pair in statusEffectVisuals)
                {
                    if (pair.Value == instance)
                    {
                        staleStatuses.Add(pair.Key);
                    }
                }

                for (var i = 0; i < staleStatuses.Count; i++)
                {
                    statusEffectVisuals.Remove(staleStatuses[i]);
                }
            }

            targetAttachedEffects.Remove(instance);
            if (instance == null)
            {
                return;
            }

            activeEffects.Remove(instance);
            instance.transform.SetParent(runtimeSkillRoot, false);
            effectPool.Release(instance);
        }

        public void RemoveEffectsAttachedTo(Transform targetRoot)
        {
            if (targetRoot == null)
            {
                return;
            }

            var effects = new List<GameObject>();
            foreach (var effect in activeEffects)
            {
                if (effect != null
                    && (effect.transform == targetRoot || effect.transform.IsChildOf(targetRoot)))
                {
                    effects.Add(effect);
                }
            }

            for (var i = 0; i < effects.Count; i++)
            {
                RemoveEffect(effects[i]);
            }
        }

        public void ClearEffects()
        {
            var effects = new List<GameObject>(activeEffects);
            for (var i = 0; i < effects.Count; i++)
            {
                RemoveEffect(effects[i]);
            }

            activeEffects.Clear();
            targetAttachedEffects.Clear();
            statusEffectVisuals.Clear();
        }

        private readonly struct EffectPoolKey : IEquatable<EffectPoolKey>
        {
            private readonly GameObject prefab;
            private readonly RuntimeSkillVisualSpec visual;
            private readonly string objectName;
            private readonly bool hitboxIsTrigger;
            private readonly bool includeHitbox;
            private readonly bool createEmptyActor;

            public EffectPoolKey(EffectCreateRequest request)
            {
                prefab = request.Prefab;
                visual = request.Visual;
                objectName = request.ObjectName ?? string.Empty;
                hitboxIsTrigger = request.HitboxIsTrigger;
                includeHitbox = request.IncludeHitbox;
                createEmptyActor = request.CreateEmptyActor;
            }

            public bool Equals(EffectPoolKey other)
            {
                return ReferenceEquals(prefab, other.prefab)
                    && ReferenceEquals(visual, other.visual)
                    && string.Equals(objectName, other.objectName, StringComparison.Ordinal)
                    && hitboxIsTrigger == other.hitboxIsTrigger
                    && includeHitbox == other.includeHitbox
                    && createEmptyActor == other.createEmptyActor;
            }

            public override bool Equals(object obj)
            {
                return obj is EffectPoolKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = prefab != null ? prefab.GetInstanceID() : 0;
                    hash = (hash * 397) ^ (visual?.GetHashCode() ?? 0);
                    hash = (hash * 397) ^ objectName.GetHashCode();
                    hash = (hash * 397) ^ hitboxIsTrigger.GetHashCode();
                    hash = (hash * 397) ^ includeHitbox.GetHashCode();
                    return (hash * 397) ^ createEmptyActor.GetHashCode();
                }
            }
        }
    }
}
