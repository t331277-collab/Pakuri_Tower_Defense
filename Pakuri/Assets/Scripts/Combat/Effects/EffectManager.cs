using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 효과 오브젝트의 생성, 상태 비주얼 연결, 제거를 한곳에서 관리한다.
 * 대상 부착 효과는 대상의 자식으로 연결하고, 각 스킬 Actor는 수명이 끝나면 제거를 요청한다.
 */
namespace Pakuri.InGame
{
    public readonly struct EffectCreateRequest
    {
        public EffectCreateRequest(
            RuntimeSkillVisualSpec visual /* 런타임 시각 효과 설정 */,
            GameObject prefab /* 생성할 프리팹 */,
            string objectName /* 게임 오브젝트 이름 */,
            Vector3 position /* 배치할 위치 */,
            Quaternion rotation /* 배치할 회전값 */,
            Transform targetTransform /* 비주얼을 붙일 대상 */,
            float durationSeconds /* 표시 시간 */,
            StatusRuntimeInstance persistentStatus /* 중복을 막고 수명을 소유할 상태 */,
            bool hitboxIsTrigger /* 피격 판정 트리거 여부 */,
            bool includeHitbox /* 피격 판정 포함 여부 */,
            bool createEmptyActor /* 비주얼 없이 Actor를 붙일 빈 오브젝트 생성 여부 */)
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

    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<StatusRuntimeInstance, GameObject> statusEffectVisuals = new Dictionary<StatusRuntimeInstance, GameObject>();
        private readonly HashSet<GameObject> targetAttachedEffects = new HashSet<GameObject>();

        /*
         * 런타임 비주얼 또는 프리팹으로 효과 오브젝트만 생성
         */
        private GameObject CreateObject(
            EffectCreateRequest request /* 효과 생성 요청 */,
            Vector3 position /* 생성 위치 */)
        {
            if (request.Visual != null && request.Visual.HasVisual()) // 런타임 비주얼 생성
            {
                var instance = CreateRuntimeObject(request.ObjectName, position, request.Rotation);
                EffectVisualBuilder.Configure(instance, request.Visual, request.HitboxIsTrigger, request.IncludeHitbox);
                return instance;
            }

            if (request.Prefab != null) // 프리팹 생성 Rin - D, E 전용
            {
                return Instantiate(request.Prefab, position, request.Rotation, runtimeSkillRoot);
            }

            if (request.CreateEmptyActor)
            {
                return CreateRuntimeObject(request.ObjectName, position, request.Rotation);
            }

            return null;
        }

        /*
         * 생성한 비주얼(버프, 상태이상)을 대상에 자식으로 붙인다.
         */
        private static void AttachVisualToTarget(GameObject instance, Transform targetTransform)
        {
            instance.transform.SetParent(targetTransform, true);
        }

        /*
         * 비주얼이나 프리팹이 없을 때 스킬 Actor를 붙일 빈 오브젝트를 생성
         */
        private GameObject CreateRuntimeObject(
            string objectName /* 게임 오브젝트 이름 */,
            Vector3 position /* 배치할 위치 */,
            Quaternion rotation /* 배치할 회전값 */)
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(runtimeSkillRoot, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /*
         * 이펙트 생성기
         */
        public GameObject CreateEffect(EffectCreateRequest request /* 효과 생성 요청 */)
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

        /*
         * 효과 등록을 해제하고 오브젝트를 삭제한다.
         */
        public void RemoveEffect(
            GameObject instance /* 제거할 효과 오브젝트 */,
            StatusRuntimeInstance status = null /* 연결된 상태 효과 */)
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

        /*
         * 스킬 루트 아래의 모든 효과를 RemoveEffect를 통해 정리한다.
         */
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
