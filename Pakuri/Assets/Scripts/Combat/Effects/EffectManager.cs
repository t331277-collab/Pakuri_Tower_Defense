/*
 * 역할: 전투 결과와 분리하여 효과 GameObject를 생성·부착·추적·제거·일괄 정리한다.
 */

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

        /// 이펙트를 넣을 오브젝트를 생성한다.

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
            return instance;
        }

        /// 상태 런타임 종료를 해당 비주얼 Actor에 전달한다.
        public void SignalStatusEffectEnded(StatusRuntimeInstance status)
        {
            if (status == null)
            {
                return;
            }

            if (!statusEffectVisuals.TryGetValue(status, out var instance)
                || instance == null)
            {
                statusEffectVisuals.Remove(status);
                return;
            }

            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor != null)
            {
                actor.Complete();
                return;
            }

            RemoveEffect(instance, status);
        }

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
