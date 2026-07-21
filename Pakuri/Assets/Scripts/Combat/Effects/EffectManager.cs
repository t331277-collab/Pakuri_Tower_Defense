using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 중 사용하는 스킬과 상태 효과의 시각 오브젝트를 관리하는 컴포넌트.
 * 몬스터·스킬별 프리팹을 찾고 효과를 생성하거나 대상에 부착하며
 * 애니메이션 수명, 상태 효과 갱신, 전투 종료 시 일괄 제거를 처리한다.
 */
namespace Pakuri.InGame
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;
        private readonly Dictionary<string, GameObject> statusEffectVisuals = new Dictionary<string, GameObject>();

        /*
         * 런타임 비주얼에 생성할 외형이 있는지 확인한다.
         */
        public static bool HasVisual(RuntimeSkillVisualSpec visual)
        {
            return visual != null && visual.HasVisual();
        }

        /*
         * 방향 벡터를 효과 회전값으로 바꾼다.
         */
        public static Quaternion ResolveRotation(Vector2 direction)
        {
            return EffectVisualBuilder.ResolveRotation(direction);
        }

        /*
         * 코드로 정의한 런타임 비주얼을 스킬 루트 아래 생성한다.
         */
        public GameObject CreateRuntimeVisual(
            RuntimeSkillVisualSpec visual,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            bool hitboxIsTrigger = false,
            bool includeHitbox = true)
        {
            if (!HasVisual(visual))
            {
                return null;
            }

            var instance = CreateRuntimeSkillObject(objectName, position, rotation);
            EffectVisualBuilder.Configure(instance, visual, hitboxIsTrigger, includeHitbox);
            return instance;
        }

        /*
         * 효과 프리팹을 스킬 루트 아래 생성한다.
         */
        public GameObject InstantiateSkillPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position, rotation, runtimeSkillRoot);
        }

        /*
         * 비주얼이 없는 스킬 액터용 빈 오브젝트를 스킬 루트 아래 생성한다.
         */
        public GameObject CreateRuntimeSkillObject(string objectName, Vector3 position, Quaternion rotation)
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(runtimeSkillRoot, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /*
         * 런타임 비주얼 또는 프리팹으로 효과 오브젝트를 생성한다.
         */
        public GameObject CreateEffectObject(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            bool createEmptyObject = false,
            bool hitboxIsTrigger = false,
            bool includeHitbox = true)
        {
            if (HasVisual(visual))
            {
                return CreateRuntimeVisual(
                    visual,
                    objectName,
                    position,
                    rotation,
                    hitboxIsTrigger,
                    includeHitbox);
            }

            if (prefab != null)
            {
                return InstantiateSkillPrefab(prefab, position, rotation);
            }

            if (createEmptyObject)
            {
                return CreateRuntimeSkillObject(objectName, position, rotation);
            }

            return null;
        }

        /*
         * 범위 효과 오브젝트를 생성하고 실제 비주얼이나 프리팹의 크기를 설정한다.
         */
        public GameObject CreateAreaEffectObject(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            float baseRadius,
            SkillSnapshot snapshot,
            bool createEmptyObject = false,
            float radiusMultiplier = 1f,
            bool requireHitbox = false)
        {
            var hasEffectObject = HasVisual(visual) || prefab != null;
            var instance = CreateEffectObject(
                visual,
                prefab,
                objectName,
                position,
                Quaternion.identity,
                createEmptyObject);
            if (hasEffectObject)
            {
                ConfigureAreaEffect(
                    instance,
                    baseRadius,
                    snapshot,
                    radiusMultiplier,
                    requireHitbox);
            }

            return instance;
        }

        /*
         * 범위 효과를 생성하고 지정한 시간이 지나면 제거한다.
         */
        public GameObject SpawnAreaEffect(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            float baseRadius,
            SkillSnapshot snapshot,
            float durationSeconds,
            bool requireHitbox = false)
        {
            var instance = CreateAreaEffectObject(
                visual,
                prefab,
                objectName,
                position,
                baseRadius,
                snapshot,
                requireHitbox: requireHitbox);
            DestroyAfter(instance, durationSeconds);
            return instance;
        }

        /*
         * 효과를 생성하고 애니메이션 또는 기본 수명에 맞춰 제거한다.
         */
        public GameObject SpawnAnimatedEffect(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            float minimumLifetimeSeconds)
        {
            var instance = CreateEffectObject(visual, prefab, objectName, position, rotation);
            if (instance != null)
            {
                DestroyAfterAnimation(instance, minimumLifetimeSeconds);
            }

            return instance;
        }

        /*
         * 런타임 비주얼 또는 프리팹을 생성하고 지정한 시간이 지나면 제거한다.
         */
        public GameObject SpawnTransient(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            float durationSeconds)
        {
            var instance = CreateEffectObject(visual, prefab, objectName, position, rotation);
            DestroyAfter(instance, durationSeconds);
            return instance;
        }

        /*
         * 스킬 추가 효과 비주얼을 생성하고 지정한 시간이 지나면 제거한다.
         */
        public GameObject SpawnEffectVisual(
            SkillEffectDefinition effect,
            Vector3 position,
            float durationSeconds)
        {
            if (effect == null)
            {
                return null;
            }

            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
            {
                objectName = $"SkillEffectVisual_{effect.EffectId}";
            }
            return SpawnTransient(
                effect.RuntimeVisual,
                effect.SkillEffectPrefab,
                objectName,
                position,
                Quaternion.identity,
                durationSeconds);
        }

        /*
         * 스킬 추가 효과 비주얼을 여러 대상에게 붙인다.
         */
        public void SpawnEffectVisualOnTargets(
            SkillEffectDefinition effect,
            IReadOnlyList<CombatUnitEntry> targets,
            float durationSeconds)
        {
            if (effect == null || targets == null)
            {
                return;
            }

            var lifetime = Mathf.Max(0.1f, durationSeconds);
            var objectName = "SkillEffectVisual";
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
            {
                objectName = $"SkillEffectVisual_{effect.EffectId}";
            }
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                SpawnAttachedEffect(
                    effect.RuntimeVisual,
                    effect.SkillEffectPrefab,
                    objectName,
                    target.Transform,
                    lifetime,
                    Vector3.zero);
            }
        }

        /*
         * 적용된 상태의 런타임 비주얼 또는 프리팹을 생성하고 대상에게 붙인다.
         */
        public void SpawnOrRefreshStatusVisual(
            UnitCombatState target,
            Transform targetTransform,
            StatusRuntimeData statusData,
            StatusRuntimeInstance status)
        {
            if (target == null || targetTransform == null || statusData == null || status == null)
            {
                return;
            }

            var hasRuntimeVisual = HasVisual(statusData.RuntimeVisual);
            if (!hasRuntimeVisual && statusData.StatusEffectPrefab == null)
            {
                return;
            }

            var sourceId = ResolveStatusSourceId(status, statusData);
            var key = CreateStatusVisualKey(target, statusData, status, sourceId, hasRuntimeVisual);

            // Unity에서 이미 파괴된 참조는 조회표에서도 제거한다.
            if (statusEffectVisuals.TryGetValue(key, out var instance) && instance == null)
            {
                statusEffectVisuals.Remove(key);
                instance = null;
            }

            var lifetime = 0f;
            if (!status.Permanent)
            {
                lifetime = Mathf.Max(0.1f, status.DurationRemaining);
            }

            if (instance == null)
            {
                if (hasRuntimeVisual)
                {
                    var objectName = "RuntimeStatusVisual";
                    if (!string.IsNullOrWhiteSpace(sourceId))
                    {
                        objectName = $"RuntimeStatusVisual_{sourceId}";
                    }

                    instance = CreateRuntimeVisual(
                        statusData.RuntimeVisual,
                        objectName,
                        targetTransform.position,
                        Quaternion.identity,
                        includeHitbox: false);
                }
                else
                {
                    instance = InstantiateSkillPrefab(
                        statusData.StatusEffectPrefab,
                        targetTransform.position,
                        Quaternion.identity);
                }

                statusEffectVisuals[key] = instance;
            }

            AttachToTarget(instance, targetTransform, lifetime, Vector3.zero);
        }

        // 조건이 끝난 영구 패시브 상태의 연결 비주얼을 출처 키로 즉시 정리한다.
        public void RemoveStatusVisual(UnitCombatState target, StatusRuntimeInstance status)
        {
            if (target == null || status == null)
            {
                return;
            }

            var statusData = status.SourceData;
            if (statusData == null)
            {
                return;
            }

            var hasRuntimeVisual = HasVisual(statusData.RuntimeVisual);
            if (!hasRuntimeVisual && statusData.StatusEffectPrefab == null)
            {
                return;
            }

            var sourceId = ResolveStatusSourceId(status, statusData);
            var key = CreateStatusVisualKey(target, statusData, status, sourceId, hasRuntimeVisual);
            if (!statusEffectVisuals.TryGetValue(key, out var instance))
            {
                return;
            }

            statusEffectVisuals.Remove(key);
            if (instance != null)
            {
                Destroy(instance);
            }
        }

        private static string ResolveStatusSourceId(StatusRuntimeInstance status, StatusRuntimeData statusData)
        {
            if (!string.IsNullOrWhiteSpace(status.SourceSkillId))
            {
                return status.SourceSkillId;
            }

            return statusData.SourceSkillId;
        }

        private static string CreateStatusVisualKey(
            UnitCombatState target,
            StatusRuntimeData statusData,
            StatusRuntimeInstance status,
            string sourceId,
            bool hasRuntimeVisual)
        {
            var unitId = string.Empty;
            if (target.Identity != null)
            {
                unitId = target.Identity.UnitId;
            }

            var visualId = 0;
            if (hasRuntimeVisual)
            {
                visualId = statusData.RuntimeVisual.GetHashCode();
            }
            else
            {
                visualId = statusData.StatusEffectPrefab.GetInstanceID();
            }

            return $"{unitId}:{status.Kind}:{sourceId}:{visualId}";
        }

        /*
         * 실제 애니메이션 길이를 기준으로 비주얼 제거를 예약하고 사용한 수명을 반환한다.
         */
        public float DestroyAfterAnimation(
            GameObject instance,
            float minimumLifetimeSeconds)
        {
            var lifetime = EffectVisualBuilder.ResolveLifetime(instance, minimumLifetimeSeconds);
            DestroyAfter(instance, lifetime);
            return lifetime;
        }

        /*
         * 스킬 루트 아래에 생성된 모든 런타임 효과를 제거한다.
         */
        public void ClearRuntimeSkillObjects()
        {
            // 자식을 뒤에서부터 순회해 삭제 중 형제 인덱스가 바뀌는 영향을 피한다.
            for (var i = runtimeSkillRoot.childCount - 1; i >= 0; i--)
            {
                var child = runtimeSkillRoot.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            statusEffectVisuals.Clear();
        }

        /*
         * 대상 추적 액터를 붙여 생성한 비주얼이 대상을 따라가게 한다.
         */
        public void AttachToTarget(GameObject instance, Transform target, float durationSeconds, Vector3 offset)
        {
            var actor = instance.GetComponent<FollowEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<FollowEffectActor>();
            }

            actor.Initialize(target, durationSeconds, offset);
        }

        /*
         * 런타임 비주얼 또는 프리팹을 생성해 대상에게 붙인다.
         */
        public GameObject SpawnAttachedEffect(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Transform target,
            float durationSeconds,
            Vector3 offset)
        {
            if (target == null)
            {
                return null;
            }

            var instance = CreateEffectObject(
                visual,
                prefab,
                objectName,
                target.position,
                Quaternion.identity);
            if (instance != null)
            {
                AttachToTarget(instance, target, durationSeconds, offset);
            }

            return instance;
        }

        /*
         * 지원 스킬의 런타임 비주얼을 대상에게 붙인다.
         */
        public GameObject SpawnAttachedSkillEffect(
            SkillRuntimeData skill,
            Transform target,
            float durationSeconds)
        {
            if (skill == null)
            {
                return null;
            }

            return SpawnAttachedEffect(
                skill.RuntimeVisual,
                null,
                $"RuntimeSupportVisual_{skill.SkillId}",
                target,
                durationSeconds,
                Vector3.zero);
        }

        /*
         * 범위 효과 오브젝트의 크기를 설정한다.
         */
        public bool ConfigureAreaEffect(
            GameObject instance,
            float baseRadius,
            SkillSnapshot snapshot,
            float radiusMultiplier = 1f,
            bool requireHitbox = false)
        {
            if (instance == null
                || (requireHitbox && !EffectVisualBuilder.HasHitbox(instance)))
            {
                return false;
            }

            EffectVisualBuilder.ConfigureAreaScale(
                instance.transform,
                baseRadius,
                snapshot,
                radiusMultiplier);
            Physics2D.SyncTransforms();
            return true;
        }

        /*
         * 단일 공격의 직선형 다중 배치 비주얼을 설정한다.
         */
        public void ConfigureSingleLineEffect(
            GameObject instance,
            SkillExecutionContext context,
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            Vector2 center)
        {
            Transform effectTransform = null;
            if (instance != null)
            {
                effectTransform = instance.transform;
            }

            EffectVisualBuilder.ConfigureSingleAttackLine(
                effectTransform,
                context,
                skill,
                snapshot,
                center);
        }

        /*
         * 생성한 비주얼의 제거 시간을 예약한다.
         */
        public void DestroyAfter(GameObject instance, float durationSeconds)
        {
            Destroy(instance, Mathf.Max(0.01f, durationSeconds));
        }

    }

    class FollowEffectActor : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;
        private float lifetime;
        private bool hasLifetime;

        public void Initialize(Transform followTarget, float durationSeconds, Vector3 localOffset)
        {
            target = followTarget;
            offset = localOffset;
            hasLifetime = durationSeconds > 0f;
            if (hasLifetime)
            {
                lifetime = Mathf.Max(0.1f, durationSeconds);
            }

            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position + offset;
            if (!hasLifetime)
            {
                return;
            }

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
