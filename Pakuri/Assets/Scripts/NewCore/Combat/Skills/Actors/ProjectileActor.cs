using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 투사체의 이동, 교차 대상 적중, 관통 한도 생명주기를 표현한다. */
namespace Pakuri.NewCore.Combat.Skills.Actors
{
    public sealed class ProjectileActor : SkillActor
    {
        private readonly UnitBaseModel source;
        private readonly IReadOnlyList<UnitBaseModel> registeredUnits;
        private readonly SkillTargeting targeting;
        private readonly CombatVector2 direction;
        private readonly float speed;
        private readonly float lifetime;
        private readonly int hitBudget;
        private readonly Action<UnitBaseModel, CombatVector2> hit;
        private readonly EffectManager effectManager;
        private readonly HashSet<UnitBaseModel> hitTargets =
            new HashSet<UnitBaseModel>();
        private CombatVector2 position;

        /* 이동 방향·속도·수명·관통 한도와 적중 콜백·이펙트를 구성한다. */
        public ProjectileActor(
            ProjectileDefinition definition,
            UnitBaseModel source,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            SkillTargeting targeting,
            CombatVector2 start,
            CombatVector2 direction,
            float speed,
            float lifetime,
            int hitBudget,
            Action<UnitBaseModel, CombatVector2> hit,
            EffectHandle effect,
            EffectManager effectManager)
            : base(definition, effect)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.registeredUnits = registeredUnits
                ?? throw new ArgumentNullException(nameof(registeredUnits));
            this.targeting = targeting
                ?? throw new ArgumentNullException(nameof(targeting));
            if (direction.SqrMagnitude <= 0.0001f)
            {
                throw new ArgumentException(
                    "Projectile requires a non-zero direction.",
                    nameof(direction));
            }
            if (speed <= 0f || lifetime <= 0f || hitBudget <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            this.direction = direction.Normalized;
            this.speed = speed;
            this.lifetime = lifetime;
            this.hitBudget = hitBudget;
            this.hit = hit ?? throw new ArgumentNullException(nameof(hit));
            this.effectManager =
                effectManager ?? throw new ArgumentNullException(nameof(effectManager));
            position = start;
        }

        public CombatVector2 Position => position;

        public CombatVector2 Direction => direction;

        public int HitCount => hitTargets.Count;

        /* 투사체를 이동시키며 새 교차 대상을 적중시키고 수명·관통 한도에서 종료한다. */
        protected override void TickActor(float deltaTime)
        {
            if (ElapsedSeconds >= lifetime)
            {
                IsComplete = true;
                return;
            }

            CombatVector2 start = position;
            CombatVector2 end = start + (direction * (speed * deltaTime));
            IReadOnlyList<ProjectileIntersection> intersections =
                targeting.ResolveProjectileIntersections(
                    source,
                    Definition,
                    registeredUnits,
                    start,
                    end,
                    hitTargets);
            for (int index = 0; index < intersections.Count; index++)
            {
                ProjectileIntersection intersection = intersections[index];
                if (!intersection.Target.IsAlive
                    || !hitTargets.Add(intersection.Target))
                {
                    continue;
                }

                hit(intersection.Target, intersection.Position);
                if (hitTargets.Count >= hitBudget)
                {
                    position = intersection.Position;
                    IsComplete = true;
                    effectManager.TryUpdate(Effect, position, direction);
                    return;
                }
            }

            position = end;
            effectManager.TryUpdate(Effect, position, direction);
        }
    }
}
