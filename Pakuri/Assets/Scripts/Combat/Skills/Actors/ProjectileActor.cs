using System;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class ProjectileActor : SkillActor
    {
        private readonly System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets;
        private readonly float speed;
        private readonly float? lifetime;
        private readonly Action<UnitBaseModel> hit;
        private readonly EffectManager effectManager;
        private CombatVector2 position;
        private int targetIndex;

        public ProjectileActor(
            ProjectileDefinition definition,
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets,
            CombatVector2 start,
            float speed,
            float? lifetime,
            Action<UnitBaseModel> hit,
            EffectHandle effect,
            EffectManager effectManager)
            : base(definition, effect)
        {
            this.targets = targets ?? throw new ArgumentNullException(nameof(targets));
            if (targets.Count == 0)
            {
                throw new ArgumentException("Projectile requires at least one target.", nameof(targets));
            }
            if (speed <= 0f || (lifetime.HasValue && lifetime.Value <= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            this.speed = speed;
            this.lifetime = lifetime;
            this.hit = hit ?? throw new ArgumentNullException(nameof(hit));
            this.effectManager =
                effectManager ?? throw new ArgumentNullException(nameof(effectManager));
            position = start;
        }

        public CombatVector2 Position => position;

        protected override void TickActor(float deltaTime)
        {
            if (lifetime.HasValue && ElapsedSeconds >= lifetime.Value)
            {
                IsComplete = true;
                return;
            }

            while (targetIndex < targets.Count && !targets[targetIndex].IsAlive)
            {
                targetIndex++;
            }
            if (targetIndex >= targets.Count)
            {
                IsComplete = true;
                return;
            }

            UnitBaseModel target = targets[targetIndex];
            CombatVector2 offset = target.Position - position;
            float travel = speed * deltaTime;
            if (offset.Magnitude <= travel)
            {
                position = target.Position;
                hit(target);
                targetIndex++;
                IsComplete = targetIndex >= targets.Count;
            }
            else
            {
                position += offset.Normalized * travel;
            }

            effectManager.TryUpdate(Effect, position, offset.Normalized);
        }
    }
}
