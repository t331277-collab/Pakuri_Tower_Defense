using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;

/* 패시브 스킬의 즉시 효과 실행 경계를 제공한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class PassiveExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 패시브 실행기에 연결한다. */
        public PassiveExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* plan의 추가 상태와 modifier를 시전자 자신에게 즉시 적용한다. */
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
