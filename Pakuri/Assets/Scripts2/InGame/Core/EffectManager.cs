using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class EffectManager : MonoBehaviour
    {
        [Serializable]
        public sealed class MonsterSkillEffectEntry
        {
            public string SkillId;
            public GameObject Prefab;
        }

        [Serializable]
        public sealed class MonsterSkillEffectGroup
        {
            public string MonsterId;
            public List<MonsterSkillEffectEntry> SkillEffects = new List<MonsterSkillEffectEntry>();
        }

        [Serializable]
        public sealed class EnemySkillEffectEntry
        {
            public StageOneEnemySkillKind StageOneSkill;
            public GameObject Prefab;
        }

        [Serializable]
        public sealed class EnemySkillEffectGroup
        {
            public string EnemyId;
            public List<EnemySkillEffectEntry> SkillEffects = new List<EnemySkillEffectEntry>();
        }

        [SerializeField] private Transform runtimeSkillRoot;
        [SerializeField] private List<MonsterSkillEffectGroup> monsterSkillEffects = new List<MonsterSkillEffectGroup>();
        [SerializeField] private List<EnemySkillEffectGroup> enemySkillEffects = new List<EnemySkillEffectGroup>();

        private readonly Dictionary<string, Dictionary<string, GameObject>> monsterLookup =
            new Dictionary<string, Dictionary<string, GameObject>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, Dictionary<StageOneEnemySkillKind, GameObject>> enemyLookup =
            new Dictionary<string, Dictionary<StageOneEnemySkillKind, GameObject>>(StringComparer.OrdinalIgnoreCase);

        private bool lookupDirty = true;

        public GameObject ResolveMonsterSkillEffectPrefab(BaseUnitRuntimeModel caster, string skillId)
        {
            var monsterId = caster != null && caster.Identity != null
                ? caster.Identity.DefinitionId
                : null;
            return ResolveMonsterSkillEffectPrefab(monsterId, skillId);
        }

        public GameObject ResolveMonsterSkillEffectPrefab(string monsterId, string skillId)
        {
            if (string.IsNullOrWhiteSpace(monsterId) || string.IsNullOrWhiteSpace(skillId))
            {
                return null;
            }

            EnsureLookup();
            return monsterLookup.TryGetValue(NormalizeKey(monsterId), out var skillMap)
                   && skillMap.TryGetValue(NormalizeKey(skillId), out var prefab)
                ? prefab
                : null;
        }

        public GameObject ResolveEnemySkillEffectPrefab(EnemyUnitRuntimeModel enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            var enemyId = enemy.Identity != null ? enemy.Identity.DefinitionId : null;
            return ResolveEnemySkillEffectPrefab(enemyId, enemy.StageOneSkill);
        }

        public GameObject ResolveEnemySkillEffectPrefab(EnemyUnitRuntimeModel enemy, StageOneEnemySkillKind skillKind)
        {
            if (enemy == null)
            {
                return null;
            }

            var enemyId = enemy.Identity != null ? enemy.Identity.DefinitionId : null;
            return ResolveEnemySkillEffectPrefab(enemyId, skillKind);
        }

        public GameObject ResolveEnemySkillEffectPrefab(string enemyId, StageOneEnemySkillKind skillKind)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return null;
            }

            EnsureLookup();
            return enemyLookup.TryGetValue(NormalizeKey(enemyId), out var skillMap)
                   && skillMap.TryGetValue(skillKind, out var prefab)
                ? prefab
                : null;
        }

        public GameObject InstantiateSkillPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return prefab != null
                ? Instantiate(prefab, position, rotation, ResolveRuntimeSkillRoot())
                : null;
        }

        public GameObject CreateRuntimeSkillObject(string objectName, Vector3 position, Quaternion rotation)
        {
            var instance = new GameObject(string.IsNullOrWhiteSpace(objectName) ? "RuntimeSkillVisual" : objectName);
            var transform = instance.transform;
            transform.SetParent(ResolveRuntimeSkillRoot(), false);
            transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private void Awake()
        {
            lookupDirty = true;
        }

        private void OnValidate()
        {
            lookupDirty = true;
        }

        private void EnsureLookup()
        {
            if (!lookupDirty)
            {
                return;
            }

            lookupDirty = false;
            monsterLookup.Clear();
            enemyLookup.Clear();

            for (var i = 0; i < monsterSkillEffects.Count; i++)
            {
                var group = monsterSkillEffects[i];
                if (group == null || string.IsNullOrWhiteSpace(group.MonsterId))
                {
                    continue;
                }

                var monsterId = NormalizeKey(group.MonsterId);
                if (!monsterLookup.TryGetValue(monsterId, out var skillMap))
                {
                    skillMap = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
                    monsterLookup.Add(monsterId, skillMap);
                }

                if (group.SkillEffects == null)
                {
                    continue;
                }

                for (var j = 0; j < group.SkillEffects.Count; j++)
                {
                    var entry = group.SkillEffects[j];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.SkillId) || entry.Prefab == null)
                    {
                        continue;
                    }

                    skillMap[NormalizeKey(entry.SkillId)] = entry.Prefab;
                }
            }

            for (var i = 0; i < enemySkillEffects.Count; i++)
            {
                var group = enemySkillEffects[i];
                if (group == null || string.IsNullOrWhiteSpace(group.EnemyId))
                {
                    continue;
                }

                var enemyId = NormalizeKey(group.EnemyId);
                if (!enemyLookup.TryGetValue(enemyId, out var skillMap))
                {
                    skillMap = new Dictionary<StageOneEnemySkillKind, GameObject>();
                    enemyLookup.Add(enemyId, skillMap);
                }

                if (group.SkillEffects == null)
                {
                    continue;
                }

                for (var j = 0; j < group.SkillEffects.Count; j++)
                {
                    var entry = group.SkillEffects[j];
                    if (entry == null || entry.Prefab == null)
                    {
                        continue;
                    }

                    skillMap[entry.StageOneSkill] = entry.Prefab;
                }
            }
        }

        private Transform ResolveRuntimeSkillRoot()
        {
            if (runtimeSkillRoot != null)
            {
                return runtimeSkillRoot;
            }

            var root = GameObject.Find("RunTimeSkill");
            if (root != null)
            {
                runtimeSkillRoot = root.transform;
                return runtimeSkillRoot;
            }

            var created = new GameObject("RunTimeSkill");
            runtimeSkillRoot = created.transform;
            return runtimeSkillRoot;
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }
    }
}
