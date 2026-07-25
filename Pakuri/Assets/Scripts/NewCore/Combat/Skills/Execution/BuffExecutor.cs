using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;

/* 버프 스킬의 대상 선정과 상태 효과 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class BuffExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 버프 실행기에 연결한다. */
        public BuffExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 선정 대상에 상태와 선택적 받는 피해 modifier를 적용하고 버프 Actor를 등록한다. */
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
