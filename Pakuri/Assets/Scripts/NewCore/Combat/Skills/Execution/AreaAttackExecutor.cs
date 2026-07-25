using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;

/* 범위 공격과 지속 필드의 대상 재평가 및 피해 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class AreaAttackExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 범위 공격 실행기에 연결한다. */
        public AreaAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 범위 중심·반경·반복 시간을 계산하고 매 tick 대상을 재평가하는 Actor를 등록한다. */
        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint));
            if (!request.TargetPoint.HasValue && ordered.Count == 0) return false;
            var center = request.TargetPoint ?? ordered[0].Position;
            float radius = plan.ResolveRadius(SkillTargeting.ReadFloat(request.Skill, "radius"));
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
                CreateEffectAt(
                    request,
                    center,
                    default)));
            return true;
        }
    }
}
