using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class LineAttackExecutor : SkillExecutor
    {
        public LineAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint));
            if (ordered.Count == 0) return false;
            CombatVector2 direction = (
                request.AimDirection
                ?? (ordered[0].Position - request.Caster.Position)).Normalized;
            float width = plan.ResolveRadius(
                SkillTargeting.ReadFloat(request.Skill, "radius"));
            var targets = ResolveLineTargets(
                request.Caster,
                ordered,
                direction,
                width);
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
                    var currentTargets = ResolveLineTargets(
                        request.Caster,
                        currentOrdered,
                        direction,
                        width);
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

        private static System.Collections.Generic.IReadOnlyList<UnitBaseModel>
            ResolveLineTargets(
                UnitBaseModel caster,
                System.Collections.Generic.IReadOnlyList<UnitBaseModel> candidates,
                CombatVector2 direction,
                float width)
        {
            var result =
                new System.Collections.Generic.List<UnitBaseModel>();
            float halfWidth = Math.Max(0f, width * 0.5f);
            for (int index = 0; index < candidates.Count; index++)
            {
                UnitBaseModel candidate = candidates[index];
                CombatVector2 offset = candidate.Position - caster.Position;
                float forward =
                    (offset.X * direction.X) + (offset.Y * direction.Y);
                float perpendicular = Math.Abs(
                    (offset.X * direction.Y) - (offset.Y * direction.X));
                if (forward >= 0f && perpendicular <= halfWidth)
                {
                    result.Add(candidate);
                }
            }
            return result.AsReadOnly();
        }
    }
}
