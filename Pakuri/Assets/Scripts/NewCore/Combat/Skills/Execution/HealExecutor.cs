using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class HealExecutor : SkillExecutor
    {
        public HealExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var targets = ResolveTargets(request);
            if (targets.Count == 0) return false;
            float amount = combat.CalculateRawValue(request.Caster, request.Skill);
            for (int index = 0; index < targets.Count; index++)
            {
                combat.Heal(request.Caster, targets[index], request.Skill, amount);
            }
            Actors.Register(new BuffActor(
                request.Skill,
                1f,
                CreateEffectAt(
                    request,
                    targets[0].Position,
                    default,
                    targets[0])));
            return true;
        }
    }
}
