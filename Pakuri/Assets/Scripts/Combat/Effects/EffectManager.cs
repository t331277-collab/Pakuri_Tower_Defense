/*
 * 역할: 런타임 스킬 효과 오브젝트 소유.
 * 책임: 전투 결과와 분리하여 효과 GameObject를 생성·부착·추적·제거·일괄 정리한다.
 */

using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>EffectCreateRequest</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct EffectCreateRequest
    {

        /// <summary><c>EffectCreateRequest</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public EffectCreateRequest(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Transform targetTransform,
            float durationSeconds,
            StatusRuntimeInstance persistentStatus,
            bool hitboxIsTrigger,
            bool includeHitbox,
            bool createEmptyActor)
        {
            Visual = visual;
            Prefab = prefab;
            ObjectName = objectName;
            Position = position;
            Rotation = rotation;
            TargetTransform = targetTransform;
            DurationSeconds = durationSeconds;
            PersistentStatus = persistentStatus;
            HitboxIsTrigger = hitboxIsTrigger;
            IncludeHitbox = includeHitbox;
            CreateEmptyActor = createEmptyActor;
        }

        public RuntimeSkillVisualSpec Visual { get; }
        public GameObject Prefab { get; }
        public string ObjectName { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Transform TargetTransform { get; }
        public float DurationSeconds { get; }
        public StatusRuntimeInstance PersistentStatus { get; }
        public bool HitboxIsTrigger { get; }
        public bool IncludeHitbox { get; }
        public bool CreateEmptyActor { get; }
    }

    /// <summary><c>EffectManager</c>가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.</summary>
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<StatusRuntimeInstance, GameObject> statusEffectVisuals = new Dictionary<StatusRuntimeInstance, GameObject>();
        private readonly HashSet<GameObject> targetAttachedEffects = new HashSet<GameObject>();

        /// <summary>전달된 런타임 입력값을 사용해 <c>Object</c>를 생성한다.</summary>
        private GameObject CreateObject(
            EffectCreateRequest request,
            Vector3 position)
        {
            if (request.Visual != null && request.Visual.HasVisual())
            {
                var instance = CreateRuntimeObject(request.ObjectName, position, request.Rotation);
                EffectVisualBuilder.Configure(instance, request.Visual, request.HitboxIsTrigger, request.IncludeHitbox);
                return instance;
            }

            if (request.Prefab != null)
            {
                return Instantiate(request.Prefab, position, request.Rotation, runtimeSkillRoot);
            }

            if (request.CreateEmptyActor)
            {
                return CreateRuntimeObject(request.ObjectName, position, request.Rotation);
            }

            return null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>VisualToTarget</c>를 연결한다.</summary>
        private static void AttachVisualToTarget(GameObject instance, Transform targetTransform)
        {
            instance.transform.SetParent(targetTransform, true);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>RuntimeObject</c>를 생성한다.</summary>
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

        /// <summary>전달된 <c>request</c> 값을 사용해 <c>Effect</c>를 생성한다.</summary>
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
                instance = CreateObject(request, request.Position);
                if (instance == null)
                {
                    return null;
                }

                created = true;

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
            if (request.PersistentStatus == null && request.TargetTransform != null)
            {
                BuffSkillActor.Attach(instance).Initialize(this, request.DurationSeconds);
            }
            else if (request.PersistentStatus == null && request.DurationSeconds > 0f)
            {
                SingleSkillActor.Attach(instance).InitializeTimed(this, request.DurationSeconds);
            }

            return instance;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Effect</c>를 소유한 런타임 상태에서 제거한다.</summary>
        public void RemoveEffect(
            GameObject instance,
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

            targetAttachedEffects.Remove(instance);
            if (instance == null)
            {
                return;
            }

            Destroy(instance);
        }

        /// <summary><c>Effects</c>를 소유한 런타임 상태에서 비운다.</summary>
        public void ClearEffects()
        {
            var attachedEffects = new List<GameObject>(targetAttachedEffects);
            for (var i = 0; i < attachedEffects.Count; i++)
            {
                var attachedEffect = attachedEffects[i];
                if (attachedEffect != null)
                {
                    attachedEffect.SetActive(false);
                }

                RemoveEffect(attachedEffect);
            }

            targetAttachedEffects.Clear();
            statusEffectVisuals.Clear();
            for (var i = runtimeSkillRoot.childCount - 1; i >= 0; i--)
            {
                var child = runtimeSkillRoot.GetChild(i).gameObject;
                child.SetActive(false);
                RemoveEffect(child);
            }

        }
    }
}
