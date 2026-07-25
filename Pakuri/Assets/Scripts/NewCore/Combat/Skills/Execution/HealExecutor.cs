using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;

/* 회복 스킬의 대상 선정과 체력 회복 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class HealExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 회복 실행기에 연결한다. */
        public HealExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 선정 대상에 계산된 회복량을 적용하고 표시용 버프 Actor를 등록한다. */
        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            var targets = ResolveTargets(request);
            if (targets.Count == 0) return false;
            float amount = combat.CalculateRawValue(request.Caster, request.Skill);
            for (int index = 0; index < targets.Count; index++)
            {
                combat.Heal(request.Caster, targets[index], request.Skill, amount);
            }
            Actors.Register(new BuffActor(
                request.Skill,
                1f,
                CreateEffectAt(
                    request,
                    targets[0].Position,
                    default,
                    targets[0])));
            return true;
        }
    }
}
