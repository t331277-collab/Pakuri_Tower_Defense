using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 런타임 스킬 비주얼과 프리팹의 생성 위치, 제거를 관리한다.
     */
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
         * 시전자의 몬스터 ID와 스킬 ID에 등록된 효과 프리팹을 찾는다.
         */
        public GameObject ResolveMonsterSkillEffectPrefab(BaseUnitRuntimeModel caster, string skillId)
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
         * 적용된 상태의 런타임 비주얼 또는 프리팹을 생성하고 대상에게 붙인다.
         */
        public void SpawnOrRefreshStatusVisual(
            BaseUnitRuntimeModel target,
            Transform targetTransform,
            StatusEffectData statusData,
            UnitStatusRuntime status)
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
        public void RemoveStatusVisual(BaseUnitRuntimeModel target, UnitStatusRuntime status)
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
        public float DestroyAfterAnimation(GameObject instance, float minimumLifetimeSeconds)
        {
            var lifetime = EffectVisualUtility.ResolveLifetime(instance, minimumLifetimeSeconds);
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
            var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(target, durationSeconds, offset);
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
