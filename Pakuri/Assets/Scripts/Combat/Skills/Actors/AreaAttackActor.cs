using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class AreaAttackActor : TimedSkillActor
    {
        public AreaAttackActor(
            AreaAttackDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, duration, effect)
        {
        }
    }
}
