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
            float duration = Math.Max(
                0.00001f,
                plan.ResolveDuration(
                    SkillTargeting.ReadFloat(
                        request.Skill,
                        "active_duration_seconds")));
            for (int index = 0; index < targets.Count; index++)
            {
                ApplyStatuses(combat, request, plan, targets[index]);
                if (string.Equals(
                    SkillTargeting.ReadString(
                        request.Skill,
                        "execution_profile"),
                    "ApplySelfIncomingDamageMultiplier",
                    StringComparison.Ordinal))
                {
                    targets[index].AddRuntimeModifier(
                        "StatusDamageTakenBonus",
                        SkillTargeting.ReadFloat(
                            request.Skill,
                            "incoming_damage_multiplier") - 1f,
                        null,
                        request.Caster,
                        duration);
                    request.RecordAppliedTarget(targets[index]);
                }
            }
            Actors.Register(new BuffActor(
                request.Skill,
                duration,
                CreateEffectAt(
                    request,
                    targets[0].Position,
                    default,
                    targets[0])));
            return true;
        }
    }
}
