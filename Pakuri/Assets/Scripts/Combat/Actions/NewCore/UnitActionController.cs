using System;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Actions
{
    public abstract class UnitActionController
    {
        protected UnitActionController(
            UnitBaseModel model,
            InGameCombatManager combatManager)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            CombatManager =
                combatManager ?? throw new ArgumentNullException(nameof(combatManager));
        }

        public UnitBaseModel Model { get; }

        protected InGameCombatManager CombatManager { get; }

        protected bool CanUse(string skillId)
        {
            if (!Model.IsAlive || !Model.CanAct)
            {
                return false;
            }

            return Model is MonsterModel monster
                ? monster.SkillBucket.GetCooldown(skillId).CanUse()
                : Model is EnemyModel enemy
                    && enemy.SkillBucket.GetCooldown(skillId).CanUse();
        }

        protected bool Execute(SkillExecutionRequest request)
        {
            return CanUse(request.Skill.skill_id)
                && CombatManager.TryExecuteSkill(request);
        }
    }
}
