using System;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public abstract class TimedSkillActor : SkillActor
    {
        private readonly float duration;

        protected TimedSkillActor(
            SkillDefinition definition,
            float duration,
            EffectHandle effect)
            : base(definition, effect)
        {
            if (duration < 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            this.duration = duration;
        }

        protected override void TickActor(float deltaTime)
        {
            IsComplete = ElapsedSeconds >= duration;
        }
    }
}
