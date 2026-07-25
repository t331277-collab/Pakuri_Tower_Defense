using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 투사체 스킬의 이동 Actor 생성과 충돌 효과 적용을 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal class ProjectileExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 투사체 실행기에 연결한다. */
        public ProjectileExecutor(
            GameDefinitionCatalog catalog,
            SkillTargeting targeting,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue)
        {
        }

        /* 조준·속도·수명·burst·관통을 계산해 기본 및 후속 투사체 Actor를 등록한다. */
        public override bool Execute(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            var ordered = plan.FilterTargets(Targeting.ResolveOrderedAll(
                request.Caster,
                request.Skill,
                request.RegisteredUnits));
            CombatVector2 direction = default;
            if (request.AimDirection.HasValue)
            {
                direction = request.AimDirection.Value.Normalized;
            }
            else if (ordered.Count > 0)
            {
                direction =
                    (ordered[0].Position - request.Caster.Position).Normalized;
            }
            if (direction.SqrMagnitude <= 0.0001f)
            {
                return false;
            }

            ProjectileDefinition definition = (ProjectileDefinition)request.Skill;
            float speed = definition.projectile_speed.GetValueOrDefault();
            float lifetime = Math.Max(0.25f, (31f / speed) + 0.5f);
            if (definition.Columns.TryGetValue(
                    "projectile_lifetime",
                    out object lifetimeValue)
                && lifetimeValue is float seconds
                && seconds > 0f)
            {
                lifetime = seconds;
            }
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

        /* 충돌 지점의 단일 또는 범위 대상에 피해·상태·적중 완료 처리를 적용한다. */
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
                new[] { collisionTarget };
            if (impactRadius > 0f)
            {
                targets = Targeting.InRadius(
                    eligibleUnits,
                    collisionPosition,
                    impactRadius);
            }
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
                        request.HitZone)
                        * damageMultiplier,
                    projectileIndex);
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

        /* 시전자의 해당 스킬 탄창이 비어 있는지 확인한다. */
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
