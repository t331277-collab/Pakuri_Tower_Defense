using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

/* 범위 공격 이펙트가 유지되는 시간 제한 생명주기를 표현한다. */
namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class AreaAttackActor : TimedSkillActor
    {
        /* 범위 공격 정의·유지 시간·이펙트를 시간 제한 Actor에 연결한다. */
        public AreaAttackActor(
            AreaAttackDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, duration, effect)
        {
        }
    }
}
