using System;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public abstract class SkillActor
    {
        protected SkillActor(SkillDefinition definition, EffectHandle effect)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Effect = effect;
        }

        public SkillDefinition Definition { get; }

        public EffectHandle Effect { get; }

        public float ElapsedSeconds { get; private set; }

        public bool IsComplete { get; protected set; }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (IsComplete)
            {
                return;
            }

            ElapsedSeconds += deltaTime;
            TickActor(deltaTime);
        }

        protected abstract void TickActor(float deltaTime);
    }
}
