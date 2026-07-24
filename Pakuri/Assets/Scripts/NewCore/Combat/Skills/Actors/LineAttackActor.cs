using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class LineAttackActor : TimedSkillActor
    {
        public LineAttackActor(
            LineAttackDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, duration, effect)
        {
        }
    }
}
