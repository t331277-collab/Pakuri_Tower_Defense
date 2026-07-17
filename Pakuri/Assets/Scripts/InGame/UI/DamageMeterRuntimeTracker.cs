using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class DamageMeterRuntimeTracker : MonoBehaviour
    {
        private readonly Dictionary<string, MonsterDamageRecord> records = new Dictionary<string, MonsterDamageRecord>(StringComparer.OrdinalIgnoreCase);

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
        }

        private void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        public static void RecordDamage(DamageApplicationOptions options, InGameResourceChangeResult result)
        {
            if (Active == null)
            {
                return;
            }

            Active.Record(options, result);
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
            var source = options.Source;
            var identity = source != null ? source.Identity : null;
            if (identity == null
                || identity.Side != UnitSide.Player
                || !(source is MonsterUnitRuntimeModel)
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

            var displayName = !string.IsNullOrWhiteSpace(options.DamageMeterDisplayName)
                ? options.DamageMeterDisplayName
                : string.Empty;

            if (!records.TryGetValue(identity.DefinitionId, out var record))
            {
                record = new MonsterDamageRecord(identity.DefinitionId);
                records.Add(identity.DefinitionId, record);
            }

            record.AddDamage(sourceId, displayName, actualDamage);
            Version++;
        }
    }

    public sealed class MonsterDamageRecord
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

        public void AddDamage(string sourceId, string displayName, float amount)
        {
            if (!sources.TryGetValue(sourceId, out var source))
            {
                source = new SkillDamageRecord(sourceId);
                sources.Add(sourceId, source);
                orderedSources.Add(source);
            }

            source.AddDamage(displayName, amount);
            TotalDamage += amount;
        }
    }

    public sealed class SkillDamageRecord
    {
        public SkillDamageRecord(string sourceId)
        {
            SourceId = sourceId;
        }

        public string SourceId { get; }
        public string DisplayName { get; private set; }
        public float Damage { get; private set; }

        public void AddDamage(string displayName, float amount)
        {
            if (string.IsNullOrWhiteSpace(DisplayName) && !string.IsNullOrWhiteSpace(displayName))
            {
                DisplayName = displayName;
            }

            Damage += amount;
        }
    }
}
