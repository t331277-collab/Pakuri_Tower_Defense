using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Presentation.Scene;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreDamageMeterTracker : MonoBehaviour
    {
        [SerializeField] private NewCoreSceneRuntime combatManager;

        private readonly Dictionary<string, DamageRecord> records =
            new Dictionary<string, DamageRecord>(StringComparer.Ordinal);

        public int Version { get; private set; }

        private void Start()
        {
            if (combatManager == null)
            {
                combatManager =
                    FindFirstObjectByType<NewCoreSceneRuntime>();
            }

            if (combatManager != null && combatManager.Combat != null)
            {
                combatManager.Combat.DamageApplied += Record;
            }
        }

        private void OnDestroy()
        {
            if (combatManager != null && combatManager.Combat != null)
            {
                combatManager.Combat.DamageApplied -= Record;
            }
        }

        public bool TryGet(
            string monsterId,
            out DamageRecord record)
        {
            return records.TryGetValue(monsterId, out record);
        }

        public void ResetMeter()
        {
            records.Clear();
            Version++;
        }

        private void Record(CombatResult result)
        {
            if (!(result.Source is MonsterModel monster))
            {
                return;
            }

            var amount = result.DamageAmount;
            if (amount <= 0f)
            {
                return;
            }

            var monsterId = monster.MonsterDefinition.id;
            if (!records.TryGetValue(monsterId, out var record))
            {
                record = new DamageRecord(monsterId);
                records.Add(monsterId, record);
            }

            record.Add(result.SkillId, amount);
            Version++;
        }
    }

    public sealed class DamageRecord
    {
        private readonly Dictionary<string, DamageSourceRecord> sources =
            new Dictionary<string, DamageSourceRecord>(StringComparer.Ordinal);
        private readonly List<DamageSourceRecord> orderedSources =
            new List<DamageSourceRecord>();

        internal DamageRecord(string monsterId)
        {
            MonsterId = monsterId;
        }

        public string MonsterId { get; }
        public float TotalDamage { get; private set; }
        public IReadOnlyList<DamageSourceRecord> OrderedSources =>
            orderedSources;

        internal void Add(string skillId, float amount)
        {
            var key = string.IsNullOrWhiteSpace(skillId)
                ? "Direct"
                : skillId;
            if (!sources.TryGetValue(
                    key,
                    out DamageSourceRecord source))
            {
                source = new DamageSourceRecord(key);
                sources.Add(key, source);
                orderedSources.Add(source);
            }

            source.Add(amount);
            TotalDamage += amount;
        }
    }

    public sealed class DamageSourceRecord
    {
        internal DamageSourceRecord(string sourceId)
        {
            SourceId = sourceId;
        }

        public string SourceId { get; }

        public float Damage { get; private set; }

        internal void Add(float amount)
        {
            Damage += amount;
        }
    }
}
