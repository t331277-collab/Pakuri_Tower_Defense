using System;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

/* 적 유닛의 정의, 스킬 버킷, 넥서스 접촉 상태를 소유한다. */
namespace Pakuri.NewCore.Units.Models
{
    public sealed class EnemyModel : UnitBaseModel
    {
        /* 기본 체력 배율로 적 정의와 고정 스킬 슬롯을 구성한다. */
        public EnemyModel(
            EnemyDefinition definition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill)
            : this(
                definition,
                slotASkill,
                slotBSkill,
                passiveSkill,
                1f)
        {
        }

        /* 지정 체력 배율과 적 정의·스킬 슬롯으로 전투 모델을 구성한다. */
        public EnemyModel(
            EnemyDefinition definition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill,
            float maximumHealthMultiplier)
            : base(
                definition ?? throw new ArgumentNullException(nameof(definition)),
                RequiredMaximumHealth(
                    definition,
                    maximumHealthMultiplier))
        {
            EnemyDefinition = definition;
            SkillBucket = new EnemySkillBucket(
                definition,
                slotASkill,
                slotBSkill,
                passiveSkill);
            AutoAttackEnabled = true;
        }

        public EnemyDefinition EnemyDefinition { get; }

        public EnemySkillBucket SkillBucket { get; }

        public bool AutoAttackEnabled { get; private set; }

        public bool HasContactedNexus { get; private set; }

        /* 적의 자동 공격 사용 여부를 설정한다. */
        public void SetAutoAttackEnabled(bool enabled)
        {
            AutoAttackEnabled = enabled;
        }

        /* 적이 넥서스에 접촉해 추가 행동이 끝났음을 기록한다. */
        public void MarkNexusContact()
        {
            HasContactedNexus = true;
        }

        /* 다음 day를 위해 생명·상태·쿨다운·자동 공격·접촉 여부를 초기화한다. */
        public void ResetForNextDay()
        {
            ResetVitalsAndStatuses();
            SkillBucket.ResetRuntimeState();
            AutoAttackEnabled = true;
            HasContactedNexus = false;
        }

        /* 적 정의의 최대 체력과 양수 배율을 검증해 적용 체력을 반환한다. */
        private static float RequiredMaximumHealth(
            EnemyDefinition definition,
            float multiplier)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (multiplier <= 0f
                || float.IsNaN(multiplier)
                || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }

            return (definition.max_health
                ?? throw new ArgumentException(
                    "Enemy definition has no max_health.",
                    nameof(definition)))
                * multiplier;
        }
    }
}
