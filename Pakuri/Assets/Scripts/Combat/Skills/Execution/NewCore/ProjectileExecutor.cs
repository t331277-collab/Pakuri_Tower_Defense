using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;

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
            var targets = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint));
            if (targets.Count == 0)
            {
                return false;
            }

            ProjectileDefinition definition = (ProjectileDefinition)request.Skill;
            float speed = definition.projectile_speed
                ?? throw new InvalidOperationException(
                    $"Projectile '{definition.skill_id}' has no projectile_speed.");
            float? lifetime = definition.Columns.TryGetValue(
                    "projectile_lifetime",
                    out object lifetimeValue)
                && lifetimeValue is float seconds
                && seconds > 0f
                    ? seconds
                    : (float?)null;
            var target = targets[0];
            int projectileCount = Math.Max(
                1,
                definition.projectile_burst_count ?? 1)
                + plan.ResolveAdditionalProjectiles();
            int targetCount = Math.Min(
                targets.Count,
                Math.Max(1, (definition.pierce_count ?? 0) + plan.ResolvePierceBonus() + 1));
            var hitTargets = new System.Collections.Generic.List<Units.Models.UnitBaseModel>();
            for (int index = 0; index < targetCount; index++)
            {
                hitTargets.Add(targets[index]);
            }
            float interval = Math.Max(
                0f,
                SkillTargeting.ReadFloat(request.Skill, "burst_interval_seconds")
                * plan.ResolveShotIntervalMultiplier());
            float damageDelay = Math.Max(
                0f,
                SkillTargeting.ReadFloat(request.Skill, "damage_delay_seconds")
                * plan.ResolveDamageDelayMultiplier());
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
                        hitTargets,
                        request.Caster.Position,
                        speed,
                        lifetime,
                        hitTarget =>
                        {
                            ApplyDamageWithNodes(
                                combat,
                                request,
                                plan,
                                hitTarget,
                                plan.ResolveDamageMultiplier(
                                    hitTarget,
                                    projectileIndex,
                                    isLastProjectile,
                                    request.HitZone),
                                projectileIndex,
                                isLastProjectile);
                            ApplyStatuses(combat, request, plan, hitTarget);
                            CompleteHit(request, hitTarget);
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
                        CreateEffect(request, target),
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
                        hitTargets,
                        request.Caster.Position,
                        speed,
                        lifetime,
                        hitTarget =>
                        {
                            ApplyDamageWithNodes(
                                combat,
                                request,
                                plan,
                                hitTarget,
                                plan.ResolveDamageMultiplier(
                                    hitTarget,
                                    projectileCount + followUpIndex,
                                    followUpIndex == followUpCount - 1,
                                    request.HitZone)
                                    * followUpMultiplier,
                                projectileCount + followUpIndex,
                                followUpIndex == followUpCount - 1);
                            ApplyStatuses(combat, request, plan, hitTarget);
                            CompleteHit(request, hitTarget);
                        },
                        CreateEffect(request, target),
                        Effects)),
                    null,
                    damageDelay + plan.ResolveFollowUpProjectileDelay()));
            }
            return true;
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
