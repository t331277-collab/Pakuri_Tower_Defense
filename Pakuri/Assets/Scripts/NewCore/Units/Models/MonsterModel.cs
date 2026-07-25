using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

/* 아군 몬스터의 정의, 스킬 버킷, 자동 행동 상태를 소유한다. */
namespace Pakuri.NewCore.Units.Models
{
    public sealed class MonsterModel : UnitBaseModel
    {
        /* 몬스터 정의·기본 액티브·PassiveBase 목록으로 모델과 스킬 버킷을 구성한다. */
        public MonsterModel(
            MonsterDefinition definition,
            SkillDefinition defaultActiveSkill,
            IEnumerable<SkillChoiceDefinition> passiveBaseChoices,
            bool autoSkillEnabled)
            : base(
                definition ?? throw new ArgumentNullException(nameof(definition)),
                RequiredMaximumHealth(definition))
        {
            MonsterDefinition = definition;
            SkillBucket = new MonsterSkillBucket(
                definition,
                defaultActiveSkill,
                passiveBaseChoices);
            AutoAttackEnabled = true;
            AutoSkillEnabled = autoSkillEnabled;
        }

        public MonsterDefinition MonsterDefinition { get; }

        public MonsterSkillBucket SkillBucket { get; }

        public bool AutoAttackEnabled { get; private set; }

        public bool AutoSkillEnabled { get; private set; }

        /* 몬스터의 자동 기본 공격 사용 여부를 설정한다. */
        public void SetAutoAttackEnabled(bool enabled)
        {
            AutoAttackEnabled = enabled;
        }

        /* 몬스터의 자동 스킬 사용 여부를 설정한다. */
        public void SetAutoSkillEnabled(bool enabled)
        {
            AutoSkillEnabled = enabled;
        }

        /* 다음 day를 위해 생명·상태·쿨다운과 선택 여부에 따른 자동 스킬을 초기화한다. */
        public void ResetForNextDay(bool isSelectedMonster)
        {
            ResetVitalsAndStatuses();
            SkillBucket.ResetRuntimeState();
            AutoAttackEnabled = true;
            AutoSkillEnabled = !isSelectedMonster;
        }

        /* 몬스터 정의의 필수 최대 체력을 검증해 반환한다. */
        private static float RequiredMaximumHealth(MonsterDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return definition.max_health
                ?? throw new ArgumentException(
                    "Monster definition has no max_health.",
                    nameof(definition));
        }
    }
}
