using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

/* 몬스터의 자동 행동과 수동 스킬 실행 조건을 중재한다. */
namespace Pakuri.NewCore.Combat.Actions
{
    public sealed class MonsterActionController : UnitActionController
    {
        /* 몬스터 모델과 공통 전투 실행 경계를 연결한다. */
        public MonsterActionController(
            MonsterModel model,
            InGameCombatManager combatManager)
            : base(model, combatManager)
        {
            Monster = model;
        }

        public MonsterModel Monster { get; }

        /* 자동 행동이 허용되면 학습 순서대로 사용 가능한 첫 스킬을 실행한다. */
        public bool TickAutomatic(IReadOnlyList<UnitBaseModel> registeredUnits)
        {
            if (!Monster.IsAlive
                || !Monster.AutoAttackEnabled
                || !Monster.AutoSkillEnabled)
            {
                return false;
            }

            for (int index = 0; index < Monster.SkillBucket.ActiveSkills.Count; index++)
            {
                var skill = Monster.SkillBucket.ActiveSkills[index];
                if (CanUse(skill.skill_id)
                    && Execute(new SkillExecutionRequest(Monster, skill, registeredUnits)))
                {
                    return true;
                }
            }

            return false;
        }

        /* 수동 모드에서 보유 스킬을 조준·목표 좌표와 함께 실행한다. */
        public bool TryExecuteManual(
            Definitions.Skills.SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2 aimDirection,
            CombatVector2 targetPoint)
        {
            if (Monster.AutoSkillEnabled || !ContainsSkill(skill))
            {
                return false;
            }

            return Execute(
                new SkillExecutionRequest(
                    Monster,
                    skill,
                    registeredUnits,
                    aimDirection,
                    targetPoint));
        }

        /* 수동 모드·보유 스킬·쿨다운 조건을 모두 만족하는지 확인한다. */
        public bool CanExecuteManual(
            Definitions.Skills.SkillDefinition skill)
        {
            return !Monster.AutoSkillEnabled
                && ContainsSkill(skill)
                && CanUse(skill.skill_id);
        }

        /* 전달된 스킬 인스턴스가 몬스터의 학습 액티브 목록에 있는지 확인한다. */
        private bool ContainsSkill(Definitions.Skills.SkillDefinition skill)
        {
            if (skill == null)
            {
                return false;
            }

            for (int index = 0; index < Monster.SkillBucket.ActiveSkills.Count; index++)
            {
                if (ReferenceEquals(Monster.SkillBucket.ActiveSkills[index], skill))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
