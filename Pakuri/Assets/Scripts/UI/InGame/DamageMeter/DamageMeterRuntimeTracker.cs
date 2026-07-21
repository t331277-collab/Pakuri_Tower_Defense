using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 전투 피해 이벤트를 받아 몬스터와 스킬별 누적 피해량을 기록하는 컴포넌트.
 */
namespace Pakuri.InGame
{
    public class DamageMeterRuntimeTracker : MonoBehaviour
    {
        private readonly Dictionary<string, MonsterDamageRecord> records = new Dictionary<string, MonsterDamageRecord>(StringComparer.OrdinalIgnoreCase);

        [SerializeField] private InGameCombatManager combatManager;

        public static DamageMeterRuntimeTracker Active { get; private set; }
        public int Version { get; private set; }

        private void Awake()
        {
            Active = this;
            ResetMeter();
        }

        private void OnEnable()
        {
            Active = this;
            ResolveCombatManager();
            if (combatManager != null)
            {
                combatManager.DamageApplied -= Record;
                combatManager.DamageApplied += Record;
            }
        }

        private void OnDisable()
        {
            if (combatManager != null)
            {
                combatManager.DamageApplied -= Record;
            }

            if (Active == this)
            {
                Active = null;
            }
        }

        private void ResolveCombatManager()
        {
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<InGameCombatManager>();
            }
        }

        public void ResetMeter()
        {
            records.Clear();
            Version++;
        }

        public bool TryGetRecord(string monsterId, out MonsterDamageRecord record)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                record = null;
                return false;
            }

            return records.TryGetValue(monsterId, out record);
        }

        private void Record(DamageApplicationOptions options, InGameResourceChangeResult result)
        {
            // Code Builder: 피해 통계는 전투 결과 이벤트를 받아 이 Tracker가 기록한다.
            var source = options.Source;
            var identity = source != null ? source.Identity : null;
            if (identity == null
                || identity.Side != UnitSide.Player
                || identity.Role != UnitRole.Monster
                || string.IsNullOrWhiteSpace(identity.DefinitionId))
            {
                return;
            }

            var healthDamage = Mathf.Max(0f, result.PreviousHealth - result.CurrentHealth);
            var shieldDamage = Mathf.Max(0f, result.PreviousShield - result.CurrentShield);
            var actualDamage = healthDamage + shieldDamage;
            if (actualDamage <= 0f)
            {
                return;
            }

            var sourceId = !string.IsNullOrWhiteSpace(options.DamageMeterSourceId)
                ? options.DamageMeterSourceId
                : options.SourceSkillId;
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                sourceId = "Unknown";
            }

            if (!records.TryGetValue(identity.DefinitionId, out var record))
            {
                record = new MonsterDamageRecord(identity.DefinitionId);
                records.Add(identity.DefinitionId, record);
            }

            // Code Builder: 표시명은 UI가 sourceId로 해석하므로 런타임에는 피해량만 저장한다.
            record.AddDamage(sourceId, actualDamage);
            Version++;
        }
    }

    public class MonsterDamageRecord
    {
        private readonly Dictionary<string, SkillDamageRecord> sources = new Dictionary<string, SkillDamageRecord>(StringComparer.OrdinalIgnoreCase);
        private readonly List<SkillDamageRecord> orderedSources = new List<SkillDamageRecord>();

        public MonsterDamageRecord(string monsterId)
        {
            MonsterId = monsterId;
        }

        public string MonsterId { get; }
        public float TotalDamage { get; private set; }
        public IReadOnlyList<SkillDamageRecord> OrderedSources => orderedSources;

        public void AddDamage(string sourceId, float amount)
        {
            if (!sources.TryGetValue(sourceId, out var source))
            {
                source = new SkillDamageRecord(sourceId);
                sources.Add(sourceId, source);
                orderedSources.Add(source);
            }

            source.AddDamage(amount);
            TotalDamage += amount;
        }
    }

    public class SkillDamageRecord
    {
        public SkillDamageRecord(string sourceId)
        {
            SourceId = sourceId;
        }

        public string SourceId { get; }
        public float Damage { get; private set; }

        public void AddDamage(float amount)
        {
            Damage += amount;
        }
    }
}
