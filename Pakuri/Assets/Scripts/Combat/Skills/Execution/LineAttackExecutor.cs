using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 선형 공격 범위의 대상 선정과 피해 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal class LineAttackExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 선형 공격 실행기에 연결한다. */
        public LineAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 조준 방향·폭·반복 시간을 계산하고 선형 범위 대상을 재평가하는 Actor를 등록한다. */
        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint));
            CombatVector2 direction = default;
            if (request.AimDirection.HasValue)
            {
                direction = request.AimDirection.Value.Normalized;
            }
            else if (ordered.Count > 0)
            {
                direction =
                    (ordered[0].Position - request.Caster.Position).Normalized;
            }
            if (direction.SqrMagnitude <= 0.0001f) return false;
            float width = plan.ResolveRadius(
                SkillTargeting.ReadFloat(request.Skill, "radius"));
            float duration = plan.ResolveDuration(
                SkillTargeting.ReadFloat(request.Skill, "active_duration_seconds"));
            float interval = SkillTargeting.ReadFloat(request.Skill, "shot_interval_seconds")
                * plan.ResolveShotIntervalMultiplier();
            int count = 1;
            if (interval > 0f)
            {
                count = Math.Max(1, (int)Math.Ceiling(duration / interval));
            }
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
                    float repeatMultiplier = plan.ResolveRepeatDamageMultiplier();
                    if (tick == 0)
                    {
                        repeatMultiplier = 1f;
                    }
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
                                request.HitZone) * repeatMultiplier,
                            tick);
                        ApplyStatuses(
                            combat,
                            request,
                            plan,
                            currentTargets[index]);
                        CompleteHit(request, currentTargets[index]);
                    }
                },
                CreateEffectAt(
                    request,
                    request.Caster.Position,
                    direction)));
            return true;
        }

        /* 시전자 전방에서 지정 폭 안에 있는 후보를 입력 순서대로 반환한다. */
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
