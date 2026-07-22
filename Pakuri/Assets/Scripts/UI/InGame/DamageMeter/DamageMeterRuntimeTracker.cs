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

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
        private void Awake()
        {
            Active = this;
            ResetMeter();
        }

        /*
         * 컴포넌트가 활성화될 때 이벤트와 표시 상태를 연결한다.
         */
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

        /*
         * 컴포넌트가 비활성화될 때 연결된 이벤트를 해제한다.
         */
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

        /*
         * ResolveCombatManager에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveCombatManager()
        {
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<InGameCombatManager>();
            }
        }

        /*
         * ResetMeter 작업을 수행한다.
         */
        public void ResetMeter()
        {
            records.Clear();
            Version++;
        }

        /*
         * TryGetRecord 작업을 시도하고 성공 여부를 반환한다.
         */
        public bool TryGetRecord(string monsterId /* 몬스터 식별자 */, out MonsterDamageRecord record /* 읽거나 갱신할 기록 */)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                record = null;
                return false;
            }

            return records.TryGetValue(monsterId, out record);
        }

        /*
         * Record 작업을 수행한다.
         */
        private void Record(DamageApplicationOptions options /* 처리에 사용할 추가 설정 */, InGameResourceChangeResult result /* 처리 결과 */)
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

        /*
         * MonsterDamageRecord에 필요한 값을 초기화한다.
         */
        public MonsterDamageRecord(string monsterId /* 몬스터 식별자 */)
        {
            MonsterId = monsterId;
        }

        public string MonsterId { get; }
        public float TotalDamage { get; private set; }
        public IReadOnlyList<SkillDamageRecord> OrderedSources => orderedSources;

        /*
         * AddDamage 작업을 수행한다.
         */
        public void AddDamage(string sourceId /* 효과를 발생시킨 대상의 식별자 */, float amount /* 적용할 수치 */)
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
        /*
         * SkillDamageRecord에 필요한 값을 초기화한다.
         */
        public SkillDamageRecord(string sourceId /* 효과를 발생시킨 대상의 식별자 */)
        {
            SourceId = sourceId;
        }

        public string SourceId { get; }
        public float Damage { get; private set; }

        /*
         * AddDamage 작업을 수행한다.
         */
        public void AddDamage(float amount /* 적용할 수치 */)
        {
            Damage += amount;
        }
    }
}
