using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class AreaAttackExecutor : SkillExecutor
    {
        public AreaAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint));
            if (ordered.Count == 0) return false;
            var center = request.TargetPoint ?? ordered[0].Position;
            float radius = plan.ResolveRadius(SkillTargeting.ReadFloat(request.Skill, "radius"));
            var targets = Targeting.InRadius(ordered, center, radius);
            if (targets.Count == 0) return false;
            float duration = plan.ResolveDuration(
                SkillTargeting.ReadFloat(request.Skill, "active_duration_seconds"));
            float interval = SkillTargeting.ReadFloat(request.Skill, "shot_interval_seconds")
                * plan.ResolveShotIntervalMultiplier();
            int count = interval > 0f
                ? Math.Max(1, (int)Math.Ceiling(duration / interval))
                : 1;
            count += plan.ResolveRepeatCount();
            Actors.Register(new ScheduledSkillActor(
                request.Skill,
                count,
                interval,
                tick =>
                {
                    var currentOrdered = plan.FilterTargets(
                        Targeting.ResolveOrderedAll(
                            request.Caster,
                            request.Skill,
                            request.RegisteredUnits,
                            request.TargetPoint));
                    var currentTargets = Targeting.InRadius(
                        currentOrdered,
                        center,
                        radius);
                    float repeatMultiplier = tick == 0
                        ? 1f
                        : plan.ResolveRepeatDamageMultiplier();
                    for (int index = 0;
                        index < currentTargets.Count;
                        index++)
                    {
                        if (!currentTargets[index].IsAlive) continue;
                        ApplyDamageWithNodes(
                            combat,
                            request,
                            plan,
                            currentTargets[index],
                            plan.ResolveDamageMultiplier(
                                currentTargets[index],
                                tick,
                                tick == count - 1,
                                request.HitZone) * repeatMultiplier,
                            tick,
                            tick == count - 1);
                        ApplyStatuses(
                            combat,
                            request,
                            plan,
                            currentTargets[index]);
                        CompleteHit(request, currentTargets[index]);
                    }
                },
                CreateEffect(request, targets[0])));
            return true;
        }
    }
}
