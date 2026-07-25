using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;

/* 보호막 스킬의 대상 선정과 보호막 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal class ShieldExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 보호막 실행기에 연결한다. */
        public ShieldExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 대상별 보호막·상태를 적용하고 지속시간 뒤 같은 application만 제거한다. */
        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var targets = ResolveTargets(request);
            if (targets.Count == 0) return false;
            float amount = combat.CalculateRawValue(request.Caster, request.Skill)
                * plan.ResolveShieldMultiplier();
            float flat = SkillTargeting.ReadFloat(request.Skill, "flat_value");
            float authoredDuration = SkillTargeting.ReadFloat(
                request.Skill,
                "active_duration_seconds");
            if (authoredDuration <= 0f)
            {
                authoredDuration = SkillTargeting.ReadFloat(
                    request.Skill,
                    "status_duration_seconds");
            }
            float duration =
                plan.ResolveDuration(authoredDuration);
            var applicationVersions = new long[targets.Count];
            string mergePolicy = SkillTargeting.ReadString(
                request.Skill,
                "status_merge_policy");
            string amountRefreshPolicy = SkillTargeting.ReadString(
                request.Skill,
                "shield_amount_refresh_policy");
            for (int index = 0; index < targets.Count; index++)
            {
                float targetAmount = amount;
                if (flat > 0f && flat <= 1f && amount == flat)
                {
                    targetAmount = targets[index].MaximumHealth * flat;
                }
                combat.AddShield(
                    request.Caster,
                    targets[index],
                    request.Skill,
                    targetAmount,
                    request.Skill.skill_id,
                    mergePolicy,
                    amountRefreshPolicy,
                    out applicationVersions[index]);
                ApplyStatuses(combat, request, plan, targets[index]);
            }
            Actors.Register(new ScheduledSkillActor(
                request.Skill,
                1,
                0f,
                _ =>
                {
                    for (int index = 0; index < targets.Count; index++)
                    {
                        float expired = targets[index].RemoveShield(
                                request.Caster,
                                request.Skill.skill_id,
                                applicationVersions[index]);
                        if (expired > 0f)
                        {
                            combat.NotifyShieldExpired(
                                request.Caster,
                                request.Skill,
                                targets[index],
                                expired);
                        }
                    }
                },
                CreateEffectAt(
                    request,
                    targets[0].Position,
                    default,
                    targets[0]),
                Math.Max(0.00001f, duration)));
            return true;
        }
    }
}
