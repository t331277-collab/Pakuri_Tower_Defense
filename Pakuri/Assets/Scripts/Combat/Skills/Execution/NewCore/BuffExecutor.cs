using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class BuffExecutor : SkillExecutor
    {
        public BuffExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var targets = ResolveTargets(request);
            if (targets.Count == 0) return false;
            for (int index = 0; index < targets.Count; index++)
            {
                ApplyStatuses(combat, request, plan, targets[index]);
            }
            Actors.Register(new BuffActor(request.Skill, Math.Max(0.00001f, plan.ResolveDuration(SkillTargeting.ReadFloat(request.Skill, "active_duration_seconds"))), CreateEffect(request, targets[0])));
            return true;
        }
    }
}
