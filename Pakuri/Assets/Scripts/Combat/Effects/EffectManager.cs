using System;
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
        /*
         * 스킬 ID와 해당 효과 프리팹의 연결 정보를 보관한다.
         */
        [Serializable]
        private class MonsterSkillEffectEntry
        {
            public string SkillId = string.Empty;
            public GameObject Prefab = null;
        }

        /*
         * 몬스터 하나가 사용하는 스킬 효과 목록을 보관한다.
         */
        [Serializable]
        private class MonsterSkillEffectGroup
        {
            public string MonsterId = string.Empty;
            public List<MonsterSkillEffectEntry> SkillEffects = new List<MonsterSkillEffectEntry>();
        }

        [SerializeField] private Transform runtimeSkillRoot;
        [SerializeField] private List<MonsterSkillEffectGroup> monsterSkillEffects = new List<MonsterSkillEffectGroup>();

        private readonly Dictionary<string, Dictionary<string, GameObject>> monsterLookup =
            new Dictionary<string, Dictionary<string, GameObject>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> statusEffectVisuals = new Dictionary<string, GameObject>();

        private bool lookupDirty = true;

        /*
         * 런타임 비주얼에 생성할 외형이 있는지 확인한다.
         */
        public bool HasVisual(RuntimeSkillVisualSpec visual)
        {
            return EffectVisualUtility.HasVisual(visual);
        }

        /*
         * 시전자의 몬스터 ID와 스킬 ID에 등록된 효과 프리팹을 찾는다.
         */
        public GameObject ResolveMonsterSkillEffectPrefab(UnitCombatState caster, string skillId)
        {
            if (caster == null || caster.Identity == null)
            {
                return null;
            }

            return ResolveMonsterSkillEffectPrefab(caster.Identity.DefinitionId, skillId);
        }

        /*
         * 몬스터 ID와 스킬 ID에 등록된 효과 프리팹을 찾는다.
         */
        public GameObject ResolveMonsterSkillEffectPrefab(string monsterId, string skillId)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || string.IsNullOrWhiteSpace(skillId))
            {
                return null;
            }

            EnsureLookup();
            if (!monsterLookup.TryGetValue(monsterId.Trim(), out var skillMap))
            {
                return null;
            }

            skillMap.TryGetValue(skillId.Trim(), out var prefab);
            return prefab;
        }

        /*
         * 우선 프리팹, 몬스터 등록 프리팹, 후순위 프리팹 순서로 사용할 효과를 찾는다.
         */
        public GameObject ResolveSkillEffectPrefab(
            UnitCombatState caster,
            string skillId,
            GameObject preferredPrefab = null,
            GameObject fallbackPrefab = null)
        {
            if (preferredPrefab != null)
            {
                return preferredPrefab;
            }

            var registeredPrefab = ResolveMonsterSkillEffectPrefab(caster, skillId);
            if (registeredPrefab != null)
            {
                return registeredPrefab;
            }

            return fallbackPrefab;
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
            if (!EffectVisualUtility.HasVisual(visual))
            {
                return null;
            }

            var instance = CreateRuntimeSkillObject(objectName, position, rotation);
            EffectVisualUtility.Configure(instance, visual, hitboxIsTrigger, includeHitbox);
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
         * 코드 비주얼을 생성하고 지정한 시간이 지나면 제거한다.
         */
        public GameObject SpawnTransient(
            RuntimeSkillVisualSpec visual,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            float durationSeconds)
        {
            var instance = CreateRuntimeVisual(visual, objectName, position, rotation);
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
            float minimumLifetimeSeconds,
            float fallbackLifetimeSeconds)
        {
            var instance = CreateEffectObject(visual, prefab, objectName, position, rotation);
            if (instance != null)
            {
                DestroyAfterAnimation(
                    instance,
                    minimumLifetimeSeconds,
                    fallbackLifetimeSeconds);
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

            var objectName = string.IsNullOrWhiteSpace(effect.EffectId)
                ? "SkillEffectVisual"
                : $"SkillEffectVisual_{effect.EffectId}";
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
            var objectName = string.IsNullOrWhiteSpace(effect.EffectId)
                ? "SkillEffectVisual"
                : $"SkillEffectVisual_{effect.EffectId}";
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

            var hasRuntimeVisual = EffectVisualUtility.HasVisual(statusData.RuntimeVisual);
            if (!hasRuntimeVisual && statusData.StatusEffectPrefab == null)
            {
                return;
            }

            var unitId = target.Identity != null ? target.Identity.UnitId : string.Empty;
            var sourceId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                ? status.SourceSkillId
                : statusData.SourceSkillId;
            var visualId = hasRuntimeVisual
                ? statusData.RuntimeVisual.GetHashCode()
                : statusData.StatusEffectPrefab.GetInstanceID();
            // 대상·상태·출처·비주얼이 같으면 기존 인스턴스를 다시 사용한다.
            var key = $"{unitId}:{status.Kind}:{sourceId}:{visualId}";

            // Unity에서 이미 파괴된 참조는 조회표에서도 제거한다.
            if (statusEffectVisuals.TryGetValue(key, out var instance) && instance == null)
            {
                statusEffectVisuals.Remove(key);
                instance = null;
            }

            // 영구 상태는 상태 제거 시 직접 정리하므로 충분히 긴 추적 시간을 사용한다.
            var lifetime = status.Permanent ? 3600f : Mathf.Max(0.1f, status.DurationRemaining);
            if (instance == null)
            {
                if (hasRuntimeVisual)
                {
                    instance = CreateRuntimeVisual(
                        statusData.RuntimeVisual,
                        string.IsNullOrWhiteSpace(sourceId) ? "RuntimeStatusVisual" : $"RuntimeStatusVisual_{sourceId}",
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
            var statusData = status != null ? status.SourceData : null;
            if (target == null || status == null || statusData == null)
            {
                return;
            }

            var hasRuntimeVisual = EffectVisualUtility.HasVisual(statusData.RuntimeVisual);
            if (!hasRuntimeVisual && statusData.StatusEffectPrefab == null)
            {
                return;
            }

            var unitId = target.Identity != null ? target.Identity.UnitId : string.Empty;
            var sourceId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                ? status.SourceSkillId
                : statusData.SourceSkillId;
            var visualId = hasRuntimeVisual
                ? statusData.RuntimeVisual.GetHashCode()
                : statusData.StatusEffectPrefab.GetInstanceID();
            var key = $"{unitId}:{status.Kind}:{sourceId}:{visualId}";
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

        /*
         * 프리팹 비주얼을 생성하고 지정한 시간이 지나면 제거한다.
         */
        public GameObject SpawnTransient(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            float durationSeconds)
        {
            var instance = InstantiateSkillPrefab(prefab, position, rotation);
            DestroyAfter(instance, durationSeconds);
            return instance;
        }

        /*
         * 실제 애니메이션 길이를 기준으로 비주얼 제거를 예약하고 사용한 수명을 반환한다.
         */
        public float DestroyAfterAnimation(
            GameObject instance,
            float minimumLifetimeSeconds,
            float fallbackLifetimeSeconds = 0f)
        {
            var lifetime = EffectVisualUtility.ResolveLifetime(
                instance,
                minimumLifetimeSeconds,
                fallbackLifetimeSeconds);
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
         * Inspector의 등록값이 바뀌면 프리팹 조회표를 다시 만들도록 표시한다.
         */
        private void OnValidate()
        {
            lookupDirty = true;
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
                || (requireHitbox && !EffectVisualUtility.HasHitbox(instance)))
            {
                return false;
            }

            EffectVisualUtility.ConfigureAreaScale(
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
            EffectVisualUtility.ConfigureSingleAttackLine(
                instance != null ? instance.transform : null,
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

        /*
         * 몬스터, 스킬 프리팹 목록을 조회표로 만든다.
         */
        private void EnsureLookup()
        {
            if (!lookupDirty)
            {
                return;
            }

            lookupDirty = false;
            monsterLookup.Clear();

            for (var i = 0; i < monsterSkillEffects.Count; i++)
            {
                var group = monsterSkillEffects[i];
                if (group == null || string.IsNullOrWhiteSpace(group.MonsterId))
                {
                    continue;
                }

                var monsterId = group.MonsterId.Trim();
                if (!monsterLookup.TryGetValue(monsterId, out var skillMap))
                {
                    skillMap = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
                    monsterLookup.Add(monsterId, skillMap);
                }

                for (var j = 0; j < group.SkillEffects.Count; j++)
                {
                    var entry = group.SkillEffects[j];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.SkillId) || entry.Prefab == null)
                    {
                        continue;
                    }

                    // 같은 스킬 ID가 중복되면 Inspector의 마지막 등록값을 사용한다.
                    skillMap[entry.SkillId.Trim()] = entry.Prefab;
                }
            }
        }
    }
}
