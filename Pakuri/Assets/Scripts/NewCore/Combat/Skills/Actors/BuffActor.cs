using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

/* 버프 이펙트가 유지되는 시간 제한 생명주기를 표현한다. */
namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public class BuffActor : TimedSkillActor
    {
        /* 스킬 정의·유지 시간·버프 이펙트를 시간 제한 Actor에 연결한다. */
        public BuffActor(
            SkillDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, duration, effect)
        {
        }
    }
}
