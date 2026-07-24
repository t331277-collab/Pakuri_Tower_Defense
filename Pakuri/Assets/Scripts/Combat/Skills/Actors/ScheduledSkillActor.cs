using System;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class ScheduledSkillActor : SkillActor
    {
        private readonly int executionCount;
        private readonly float intervalSeconds;
        private readonly float initialDelaySeconds;
        private readonly Action<int> execute;
        private int executed;

        public ScheduledSkillActor(
            SkillDefinition definition,
            int executionCount,
            float intervalSeconds,
            Action<int> execute,
            EffectHandle effect,
            float initialDelaySeconds = 0f)
            : base(definition, effect)
        {
            if (executionCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(executionCount));
            }

            if (intervalSeconds < 0f
                || float.IsNaN(intervalSeconds)
                || float.IsInfinity(intervalSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
            }

            if (initialDelaySeconds < 0f
                || float.IsNaN(initialDelaySeconds)
                || float.IsInfinity(initialDelaySeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelaySeconds));
            }

            this.executionCount = executionCount;
            this.intervalSeconds = intervalSeconds;
            this.initialDelaySeconds = initialDelaySeconds;
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override void TickActor(float deltaTime)
        {
            while (executed < executionCount
                && ElapsedSeconds + 0.00001f
                    >= initialDelaySeconds + (intervalSeconds * executed))
            {
                execute(executed);
                executed++;
            }

            IsComplete = executed >= executionCount;
        }
    }
}
