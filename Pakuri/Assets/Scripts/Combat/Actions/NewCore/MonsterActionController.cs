using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Actions
{
    public sealed class MonsterActionController : UnitActionController
    {
        public MonsterActionController(
            MonsterModel model,
            InGameCombatManager combatManager)
            : base(model, combatManager)
        {
            Monster = model;
        }

        public MonsterModel Monster { get; }

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
