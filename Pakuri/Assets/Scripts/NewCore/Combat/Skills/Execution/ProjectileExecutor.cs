using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class ProjectileExecutor : SkillExecutor
    {
        public ProjectileExecutor(
            GameDefinitionCatalog catalog,
            SkillTargeting targeting,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue)
        {
        }

        public override bool Execute(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits));
            CombatVector2 direction = request.AimDirection.HasValue
                ? request.AimDirection.Value.Normalized
                : ordered.Count > 0
                    ? (ordered[0].Position - request.Caster.Position).Normalized
                    : default;
            if (direction.SqrMagnitude <= 0.0001f)
            {
                return false;
            }

            ProjectileDefinition definition = (ProjectileDefinition)request.Skill;
            float speed = definition.projectile_speed
                ?? throw new InvalidOperationException(
                    $"Projectile '{definition.skill_id}' has no projectile_speed.");
            float lifetime = definition.Columns.TryGetValue(
                    "projectile_lifetime",
                    out object lifetimeValue)
                && lifetimeValue is float seconds
                && seconds > 0f
                    ? seconds
                    : Math.Max(0.25f, (31f / speed) + 0.5f);
            int projectileCount = Math.Max(
                1,
                definition.projectile_burst_count ?? 1)
                + plan.ResolveAdditionalProjectiles();
            int hitBudget = Math.Max(
                1,
                (definition.pierce_count ?? 0)
                    + plan.ResolvePierceBonus()
                    + 1);
            var eligibleUnits = plan.FilterTargets(request.RegisteredUnits);
            float interval = Math.Max(
                0f,
                SkillTargeting.ReadFloat(request.Skill, "burst_interval_seconds")
                * plan.ResolveShotIntervalMultiplier());
            float damageDelay = Math.Max(
                0f,
                SkillTargeting.ReadFloat(request.Skill, "damage_delay_seconds")
                * plan.ResolveDamageDelayMultiplier());
            float impactRadius = SkillTargeting.ReadFloat(
                request.Skill,
                "radius");
            if (impactRadius > 0f)
            {
                impactRadius = plan.ResolveRadius(impactRadius);
            }
            Actors.Register(new ScheduledSkillActor(
                definition,
                projectileCount,
                interval,
                projectileIndex =>
                {
                    bool isLastProjectile = projectileIndex == projectileCount - 1;
                    bool lastHitNotified = false;
                    Actors.Register(new ProjectileActor(
                        definition,
                        request.Caster,
                        eligibleUnits,
                        Targeting,
                        request.Caster.Position,
                        direction,
                        speed,
                        lifetime,
                        hitBudget,
                        (hitTarget, collisionPosition) =>
                        {
                            ApplyImpact(
                                combat,
                                request,
                                plan,
                                hitTarget,
                                projectileIndex,
                                isLastProjectile,
                                collisionPosition,
                                impactRadius,
                                eligibleUnits,
                                projectileCount,
                                1f);
                            RegisterImpactEffect(
                                request,
                                collisionPosition);
                            if (isLastProjectile
                                && !lastHitNotified
                                && IsMagazineEmpty(request))
                            {
                                lastHitNotified = true;
                                combat.NotifyMagazineLastProjectileHit(
                                    request.Caster,
                                    request.Skill,
                                    hitTarget);
                            }
                        },
                        CreateEffectAt(
                            request,
                            request.Caster.Position,
                            direction),
                        Effects));
                },
                null,
                damageDelay));
            int followUpCount = plan.ResolveFollowUpProjectileCount();
            if (followUpCount > 0)
            {
                float followUpMultiplier =
                    plan.ResolveFollowUpProjectileMultiplier();
                Actors.Register(new ScheduledSkillActor(
                    definition,
                    followUpCount,
                    0f,
                    followUpIndex => Actors.Register(new ProjectileActor(
                        definition,
                        request.Caster,
                        eligibleUnits,
                        Targeting,
                        request.Caster.Position,
                        direction,
                        speed,
                        lifetime,
                        hitBudget,
                        (hitTarget, collisionPosition) =>
                        {
                            ApplyImpact(
                                combat,
                                request,
                                plan,
                                hitTarget,
                                projectileCount + followUpIndex,
                                followUpIndex == followUpCount - 1,
                                collisionPosition,
                                impactRadius,
                                eligibleUnits,
                                projectileCount,
                                followUpMultiplier);
                            RegisterImpactEffect(
                                request,
                                collisionPosition);
                        },
                        CreateEffectAt(
                            request,
                            request.Caster.Position,
                            direction),
                        Effects)),
                    null,
                    damageDelay + plan.ResolveFollowUpProjectileDelay()));
            }
            return true;
        }

        private void ApplyImpact(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel collisionTarget,
            int projectileIndex,
            bool isLastProjectile,
            CombatVector2 collisionPosition,
            float impactRadius,
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> eligibleUnits,
            int burstProjectileCount,
            float damageMultiplier)
        {
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets =
                impactRadius > 0f
                    ? Targeting.InRadius(
                        eligibleUnits,
                        collisionPosition,
                        impactRadius)
                    : new[] { collisionTarget };
            for (var index = 0; index < targets.Count; index++)
            {
                UnitBaseModel target = targets[index];
                ApplyDamageWithNodes(
                    combat,
                    request,
                    plan,
                    target,
                    plan.ResolveDamageMultiplier(
                        target,
                        projectileIndex,
                        isLastProjectile,
                        request.HitZone)
                        * damageMultiplier,
                    projectileIndex,
                    isLastProjectile);
                ApplyStatuses(
                    combat,
                    request,
                    plan,
                    target,
                    projectileIndex,
                    burstProjectileCount);
                CompleteHit(request, target);
            }
        }

        private static bool IsMagazineEmpty(SkillExecutionRequest request)
        {
            if (request.Caster is Units.Models.MonsterModel monster
                && monster.SkillBucket.Cooldowns.TryGetValue(
                    request.Skill.skill_id,
                    out var monsterCooldown))
            {
                return monsterCooldown.CurrentMagazine == 0;
            }
            if (request.Caster is Units.Models.EnemyModel enemy
                && enemy.SkillBucket.Cooldowns.TryGetValue(
                    request.Skill.skill_id,
                    out var enemyCooldown))
            {
                return enemyCooldown.CurrentMagazine == 0;
            }
            return false;
        }
    }
}
