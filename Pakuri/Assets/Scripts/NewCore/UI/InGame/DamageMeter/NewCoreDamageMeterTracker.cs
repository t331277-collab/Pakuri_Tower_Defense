using System;
using System.Collections.Generic;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* Monster별 누적 피해와 skill source별 피해 record를 combat event에서 집계한다. */
namespace Pakuri.NewCore.UI.InGame.DamageMeter
{
    public class NewCoreDamageMeterTracker : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;

        private readonly Dictionary<string, DamageRecord> records =
            new Dictionary<string, DamageRecord>(StringComparer.Ordinal);

        public int Version { get; private set; }

        /* combat runtime을 찾고 피해 적용 event 구독을 시작한다. */
        private void Start()
        {
            if (combatManager == null)
            {
                combatManager =
                    FindFirstObjectByType<GameBootstrap>();
            }

            if (combatManager != null && combatManager.Combat != null)
            {
                combatManager.Combat.DamageApplied += Record;
            }
        }

        /* tracker 제거 시 combat 피해 적용 event 구독을 해제한다. */
        private void OnDestroy()
        {
            if (combatManager != null && combatManager.Combat != null)
            {
                combatManager.Combat.DamageApplied -= Record;
            }
        }

        /* Monster id에 대응하는 누적 피해 record를 조회한다. */
        public bool TryGet(
            string monsterId,
            out DamageRecord record)
        {
            return records.TryGetValue(monsterId, out record);
        }

        /* 누적 피해를 비우고 표시 갱신 version을 올린다. */
        public void ResetMeter()
        {
            records.Clear();
            Version++;
        }

        /* Monster가 가한 유효 피해를 source별 record에 누적한다. */
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

    public class DamageRecord
    {
        private readonly Dictionary<string, DamageSourceRecord> sources =
            new Dictionary<string, DamageSourceRecord>(StringComparer.Ordinal);
        private readonly List<DamageSourceRecord> orderedSources =
            new List<DamageSourceRecord>();

        /* 지정 Monster가 소유하는 피해 record를 만든다. */
        internal DamageRecord(string monsterId)
        {
            MonsterId = monsterId;
        }

        public string MonsterId { get; }
        public float TotalDamage { get; private set; }
        public IReadOnlyList<DamageSourceRecord> OrderedSources =>
            orderedSources;

        /* skill source 피해와 Monster 총 피해를 함께 누적한다. */
        internal void Add(string skillId, float amount)
        {
            string key = skillId;
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "Direct";
            }

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

    public class DamageSourceRecord
    {
        /* 지정 source id의 피해 record를 만든다. */
        internal DamageSourceRecord(string sourceId)
        {
            SourceId = sourceId;
        }

        public string SourceId { get; }

        public float Damage { get; private set; }

        /* source 누적 피해에 양을 더한다. */
        internal void Add(float amount)
        {
            Damage += amount;
        }
    }
}
