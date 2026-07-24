using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class PassiveExecutor : SkillExecutor
    {
        public PassiveExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            foreach (string statusId in plan.AdditionalStatusIds())
            {
                combat.ApplyStatus(
                    request.Caster,
                    request.Caster,
                    Catalog.GetStatus(statusId),
                    null,
                    null,
                    request.Skill.skill_id);
            }
            ApplyPlanStatusModifiers(
                request,
                plan,
                request.Caster,
                Math.Max(
                    0.00001f,
                    plan.ResolveDuration(
                        SkillTargeting.ReadFloat(
                            request.Skill,
                            "active_duration_seconds"))));
            return true;
        }
    }
}
