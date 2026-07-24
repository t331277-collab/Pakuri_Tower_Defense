using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class SingleAttackExecutor : SkillExecutor
    {
        public SingleAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var targets = ResolveTargets(request, plan);
            if (targets.Count == 0) return false;
            for (int targetIndex = 0;
                targetIndex < targets.Count;
                targetIndex++)
            {
                UnitBaseModel target = targets[targetIndex];
                ApplyDamageWithNodes(
                    combat,
                    request,
                    plan,
                    target,
                    plan.ResolveDamageMultiplier(
                        target,
                        0,
                        true,
                        request.HitZone),
                    0,
                    true);
                ApplyStatuses(combat, request, plan, target);
                CompleteHit(request, target);
                int repeatCount = plan.ResolveRepeatCount();
                if (repeatCount > 0)
                {
                    Actors.Register(new ScheduledSkillActor(
                        request.Skill,
                        repeatCount,
                        plan.ResolveRepeatInterval(),
                        repeatIndex =>
                        {
                            if (!target.IsAlive) return;
                            ApplyDamageWithNodes(
                                combat,
                                request,
                                plan,
                                target,
                                plan.ResolveDamageMultiplier(
                                    target,
                                    repeatIndex + 1,
                                    repeatIndex == repeatCount - 1,
                                    request.HitZone)
                                    * plan.ResolveRepeatDamageMultiplier(),
                                repeatIndex + 1,
                                repeatIndex == repeatCount - 1);
                            ApplyStatuses(combat, request, plan, target);
                            CompleteHit(request, target);
                        },
                        null,
                        plan.ResolveRepeatInterval()));
                }
            }
            Actors.Register(new SingleAttackActor(
                (SingleAttackDefinition)request.Skill,
                0.00001f,
                CreateEffect(request, targets[0])));
            return true;
        }
    }
}
