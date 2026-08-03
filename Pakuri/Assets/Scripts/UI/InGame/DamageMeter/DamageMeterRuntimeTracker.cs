/*
 * 역할: 런타임 Damage Meter 집계.
 * 책임: 발생원별 플레이어 피해를 누적하고 정렬된 Snapshot을 InGame Damage Meter에 제공한다.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{

    /// DamageMeterRuntimeTracker에 해당하는 누적 런타임 데이터를 추적한다.
    public class DamageMeterRuntimeTracker : MonoBehaviour
    {
        private readonly Dictionary<string, MonsterDamageRecord> records = new Dictionary<string, MonsterDamageRecord>(StringComparer.OrdinalIgnoreCase);

        [SerializeField] private InGameCombatManager combatManager;

        public static DamageMeterRuntimeTracker Active { get; private set; }
        public int Version { get; private set; }

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            Active = this;
            ResetMeter();
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
        private void OnEnable()
        {
            Active = this;
            if (combatManager != null)
            {
                combatManager.DamageApplied -= Record;
                combatManager.DamageApplied += Record;
            }
        }

        /// Unity가 컴포넌트를 비활성화할 때 구독과 임시 상태를 중단한다.
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

        /// Meter를 초기 런타임 상태로 되돌린다.
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

        private void Record(AttackRule attackRule, InGameResourceChangeResult result)
        {

            var source = attackRule.Source;
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

            var sourceId = !string.IsNullOrWhiteSpace(attackRule.DamageMeterSourceId)
                ? attackRule.DamageMeterSourceId
                : attackRule.SourceSkillId;
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                sourceId = "Unknown";
            }

            if (!records.TryGetValue(identity.DefinitionId, out var record))
            {
                record = new MonsterDamageRecord();
                records.Add(identity.DefinitionId, record);
            }

            record.AddDamage(sourceId, actualDamage);
            Version++;
        }
    }

    public class MonsterDamageRecord
    {
        private readonly Dictionary<string, SkillDamageRecord> sources = new Dictionary<string, SkillDamageRecord>(StringComparer.OrdinalIgnoreCase);
        private readonly List<SkillDamageRecord> orderedSources = new List<SkillDamageRecord>();

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
