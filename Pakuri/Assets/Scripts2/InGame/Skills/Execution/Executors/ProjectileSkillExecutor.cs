using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public sealed class ProjectileSkillExecutor : TypedSkillExecutor<ProjectileSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ProjectileSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillExecutionUtility.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                if (!context.HasManualAimDirection)
                {
                    return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
                }

                direction = Vector2.right;
            }

            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var currentBurstProjectileIndex = context.Runtime != null
                ? context.Runtime.ResolveCurrentBurstProjectileIndex()
                : 1;
            var prefab = skill.Projectile != null ? skill.Projectile.ProjectilePrefab : null;
            if (prefab == null)
            {
                var effects = context.CombatManager.Effects;
                prefab = effects != null ? effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId) : null;
                if (prefab == null && snapshot != null && snapshot.SkillEffectPrefab != null && !skill.HasImpactArea)
                {
                    prefab = snapshot.SkillEffectPrefab;
                }
            }

            var baseStatusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            var onHitEffects = ResolveTimedEffects(context, snapshot, planEffects, SkillMultiEffectTiming.OnHit);
            var onExpireEffects = ResolveTimedEffects(context, snapshot, planEffects, SkillMultiEffectTiming.OnExpire);
            var projectile = skill.Projectile;
            var burstProjectileCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var requiresProjectileActor = skill.StopOnFirstHit
                || skill.HasImpactArea
                || skill.ImpactDelaySeconds > 0f
                || onHitEffects.Length > 0
                || onExpireEffects.Length > 0;
            if (prefab == null && !requiresProjectileActor)
            {
                if (target != null)
                {
                    var directStatusSpec = ResolveBurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                    ApplyDirectProjectileHit(context, skill, snapshot, target, directStatusSpec, damage, attribute);
                    return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
                }

                return new SkillExecutionResult(
                    context.HasManualAimDirection ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected,
                    skill.SkillId,
                    GetType().Name);
            }

            var speed = projectile != null ? projectile.ProjectileSpeed : 0f;
            var pierce = projectile != null ? projectile.PierceCount : 0;
            var projectileCount = projectile != null ? Math.Max(1, projectile.ProjectilesPerShot) : 1;
            if (snapshot != null)
            {
                pierce += snapshot.PierceBonus;
                if (burstProjectileCount <= 1)
                {
                    projectileCount += snapshot.AdditionalProjectileBonus;
                }
            }

            projectileCount = Math.Max(1, projectileCount);
            pierce = Math.Max(0, pierce);
            var burstDamageMultiplier = ResolveBurstDamageMultiplier(
                skill,
                snapshot,
                currentBurstProjectileIndex,
                burstProjectileCount);
            var launchDamage = damage * burstDamageMultiplier;
            var isMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var lifetime = SkillExecutionUtility.ResolveProjectileLifetime(skill);
            var boundary = context.CombatManager.ResolveProjectileDestroyBoundaryX();
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ResolveProjectileSpreadDirection(direction, i, projectileCount);
                var effects = context.CombatManager.Effects;
                if (effects == null)
                {
                    if (target != null)
                    {
                        var directStatusSpec = ResolveBurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                        ApplyDirectProjectileHit(context, skill, snapshot, target, directStatusSpec, launchDamage, attribute);
                    }

                    continue;
                }

                var projectileLaunchIndex = context.Runtime != null
                    ? context.Runtime.AdvanceProjectileLaunchCount()
                    : 0;
                var branchSpec = ResolveBranchSpec(snapshot, prefab, projectileLaunchIndex);
                var instance = prefab != null
                    ? effects.InstantiateSkillPrefab(prefab, origin, SkillExecutionUtility.ResolveRotation(spreadDirection))
                    : new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameProjectile" : $"InGameProjectile_{skill.SkillId}");
                if (instance == null)
                {
                    if (target != null)
                    {
                        var directStatusSpec = ResolveBurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                        ApplyDirectProjectileHit(context, skill, snapshot, target, directStatusSpec, launchDamage, attribute);
                    }

                    continue;
                }

                var actor = instance.GetComponent<InGameProjectileActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<InGameProjectileActor>();
                }

                var statusSpec = ResolveBurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                actor.Initialize(
                    context.CombatManager,
                    context.Caster,
                    spreadDirection,
                    speed,
                    launchDamage,
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    statusSpec,
                    branchSpec,
                    SkillStatusSpecUtility.ResolveStatusSpec(skill.ImpactStatus, snapshot),
                    onHitEffects,
                    onExpireEffects,
                    skill.ContactDamageEnabled,
                    skill.StopOnFirstHit,
                    ResolveImpactDelay(skill, snapshot),
                    skill.ImpactEffectPrefab,
                    skill.HasImpactArea,
                    SkillAreaUtility.ResolveRadius(skill.ImpactArea != null ? skill.ImpactArea.Radius : 0f, snapshot),
                    launchDamage,
                    context.Runtime,
                    snapshot,
                    null,
                    skill.SkillId,
                    isMagazineLastProjectile,
                    skill.Damage != null && skill.Damage.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
            }

            TryScheduleFollowUpProjectile(
                context,
                snapshot,
                skill,
                prefab,
                baseStatusSpec,
                onHitEffects,
                onExpireEffects,
                origin,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                boundary,
                lifetime,
                burstProjectileCount,
                currentBurstProjectileIndex);

            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        private static Vector2 ResolveProjectileSpreadDirection(Vector2 direction, int index, int count)
        {
            if (count <= 1)
            {
                return direction;
            }

            const float angleStep = 10f;
            var offset = (index - (count - 1) * 0.5f) * angleStep;
            return RotateDirection(direction, offset);
        }

        private static Vector2 RotateDirection(Vector2 direction, float degrees)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.right;
            }

            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }
        private static ProjectileBranchHitSpec ResolveBranchSpec(SkillExecutionSnapshot snapshot, GameObject prefab, int projectileLaunchIndex)
        {
            if (snapshot == null || !snapshot.HasBranchBehavior || prefab == null)
            {
                return null;
            }

            var chance = ResolveBranchChance(snapshot, projectileLaunchIndex);
            var count = snapshot.HasBranchCount ? snapshot.BranchCount : chance > 0f ? 1 : 0;
            var radius = snapshot.HasBranchSearchRadius ? snapshot.BranchSearchRadius : 4.5f;
            if (chance <= 0f || count <= 0 || radius <= 0f)
            {
                return null;
            }

            return new ProjectileBranchHitSpec
            {
                Enabled = true,
                ProjectilePrefab = prefab,
                Chance = Mathf.Clamp01(chance),
                Count = Math.Max(1, count),
                DamageMultiplier = snapshot.HasBranchDamageMultiplier ? Mathf.Max(0f, snapshot.BranchDamageMultiplier) : 1f,
                SearchRadius = Mathf.Max(0f, radius)
            };
        }

        private static float ResolveBranchChance(SkillExecutionSnapshot snapshot, int projectileLaunchIndex)
        {
            var chance = snapshot.HasBranchChanceSet ? snapshot.BranchChanceSet : snapshot.BranchChanceBonus;
            if (snapshot.HasBranchLaunchTrigger
                && projectileLaunchIndex > 0
                && projectileLaunchIndex % snapshot.BranchLaunchPeriod == 0)
            {
                chance = snapshot.BranchLaunchChanceSet;
            }

            return chance;
        }

        private static float ResolveBurstDamageMultiplier(
            ProjectileSkillData skill,
            SkillExecutionSnapshot snapshot,
            int projectileIndex,
            int burstProjectileCount)
        {
            var multiplier = 1f;
            var projectile = skill != null ? skill.Projectile : null;
            if (projectile != null
                && projectile.BurstDamageMultiplier > 0f
                && MatchesBurstProjectileIndex(projectile.BurstDamageProjectileIndex, projectileIndex, burstProjectileCount))
            {
                multiplier *= projectile.BurstDamageMultiplier;
            }

            if (snapshot != null)
            {
                multiplier *= snapshot.ResolveBurstDamageMultiplier(projectileIndex, burstProjectileCount);
            }

            return Mathf.Max(0f, multiplier);
        }

        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex > 0 && configuredIndex == projectileIndex;
        }

        private static void TryScheduleFollowUpProjectile(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillData skill,
            GameObject prefab,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitEffects,
            SkillEffectDefinition[] onExpireEffects,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float baseDamage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime,
            int burstProjectileCount,
            int currentBurstProjectileIndex)
        {
            if (context == null
                || context.CombatManager == null
                || skill == null
                || snapshot == null
                || !snapshot.HasFollowUpProjectile
                || prefab == null
                || currentBurstProjectileIndex < burstProjectileCount)
            {
                return;
            }

            context.CombatManager.StartCoroutine(ExecuteFollowUpProjectilesAfterDelay(
                context,
                snapshot,
                skill,
                prefab,
                statusSpec,
                onHitEffects,
                onExpireEffects,
                origin,
                direction,
                speed,
                baseDamage,
                attribute,
                pierce,
                boundary,
                lifetime));
        }

        private static IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillData skill,
            GameObject prefab,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitEffects,
            SkillEffectDefinition[] onExpireEffects,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float baseDamage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime)
        {
            if (snapshot.FollowUpProjectileDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(snapshot.FollowUpProjectileDelaySeconds);
            }
            else
            {
                yield return null;
            }

            if (context == null || context.CombatManager == null || skill == null || prefab == null)
            {
                yield break;
            }

            var count = Math.Max(1, snapshot.FollowUpProjectileCount);
            for (var i = 0; i < count; i++)
            {
                SpawnProjectileActor(
                    context,
                    snapshot,
                    skill,
                    prefab,
                    statusSpec,
                    onHitEffects,
                    onExpireEffects,
                    origin,
                    direction,
                    speed,
                    baseDamage * Mathf.Max(0f, snapshot.FollowUpProjectileDamageMultiplier),
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    false);
            }
        }

        private static void SpawnProjectileActor(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillData skill,
            GameObject prefab,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitEffects,
            SkillEffectDefinition[] onExpireEffects,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float damage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime,
            bool isMagazineLastProjectile)
        {
            if (context == null || context.CombatManager == null || skill == null || prefab == null)
            {
                return;
            }

            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return;
            }

            var projectileLaunchIndex = context.Runtime != null
                ? context.Runtime.AdvanceProjectileLaunchCount()
                : 0;
            var branchSpec = ResolveBranchSpec(snapshot, prefab, projectileLaunchIndex);
            var instance = effects.InstantiateSkillPrefab(prefab, origin, SkillExecutionUtility.ResolveRotation(direction));
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.Caster,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                boundary,
                lifetime,
                statusSpec,
                branchSpec,
                SkillStatusSpecUtility.ResolveStatusSpec(skill.ImpactStatus, snapshot),
                onHitEffects,
                onExpireEffects,
                skill.ContactDamageEnabled,
                skill.StopOnFirstHit,
                ResolveImpactDelay(skill, snapshot),
                skill.ImpactEffectPrefab,
                skill.HasImpactArea,
                SkillAreaUtility.ResolveRadius(skill.ImpactArea != null ? skill.ImpactArea.Radius : 0f, snapshot),
                damage,
                context.Runtime,
                snapshot,
                null,
                skill.SkillId,
                isMagazineLastProjectile,
                skill.Damage != null && skill.Damage.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
        }

        private static void TryApplyDirectStatus(
            InGameCombatManager combatManager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(combatManager, target, statusSpec, source);
        }

        private static void ApplyDirectProjectileHit(
            SkillExecutionContext context,
            ProjectileSkillData skill,
            SkillExecutionSnapshot snapshot,
            UnitRosterEntry target,
            ProjectileStatusHitSpec statusSpec,
            float damage,
            DamageAttribute attribute)
        {
            if (context == null || skill == null || target == null || target.Model == null)
            {
                return;
            }

            var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
            var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
            if (context.Runtime != null && snapshot != null)
            {
                resolvedDamage *= context.Runtime.ResolveConsecutiveHitDamageMultiplier(target.Model, snapshot);
            }

            resolvedDamage = Mathf.Max(0f, resolvedDamage);
            context.CombatManager.ApplyDamage(
                target.Model,
                resolvedDamage,
                attribute,
                context.Caster,
                skill.Damage != null && skill.Damage.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                skill.SkillId);
            TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
            SkillOnHitAdditionalDamageUtility.TryApply(
                context.CombatManager,
                context.Roster,
                context.Runtime,
                snapshot,
                context.CasterEntry,
                context.Caster,
                skill.SkillId,
                target,
                hitPosition,
                resolvedDamage);
        }

        private static float ResolveImpactDelay(ProjectileSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var delay = skill != null ? skill.ImpactDelaySeconds : 0f;
            if (snapshot != null)
            {
                delay *= Mathf.Max(0f, snapshot.DamageDelayMultiplier);
            }

            return Mathf.Max(0f, delay);
        }

        private static ProjectileStatusHitSpec ResolveBurstStatusSpec(
            ProjectileStatusHitSpec baseStatusSpec,
            SkillExecutionSnapshot snapshot,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (baseStatusSpec == null || snapshot == null)
            {
                return baseStatusSpec;
            }

            var stacksBonus = snapshot.ResolveBurstStatusStacksBonus(projectileIndex, burstProjectileCount);
            if (stacksBonus == 0)
            {
                return baseStatusSpec;
            }

            return CloneStatusSpecWithStacks(baseStatusSpec, Mathf.Max(1, baseStatusSpec.Stacks + stacksBonus));
        }

        private static ProjectileStatusHitSpec CloneStatusSpecWithStacks(ProjectileStatusHitSpec source, int stacks)
        {
            if (source == null)
            {
                return null;
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = source.Enabled,
                Kind = source.Kind,
                StatusData = source.StatusData,
                Chance = source.Chance,
                Stacks = stacks,
                DurationSeconds = source.DurationSeconds,
                MaxStacks = source.MaxStacks,
                Permanent = source.Permanent,
                RefreshDuration = source.RefreshDuration,
                ThresholdSourceStatusId = source.ThresholdSourceStatusId,
                ThresholdSourceMinStacks = source.ThresholdSourceMinStacks,
                ThresholdStatusSpec = source.ThresholdStatusSpec
            };
        }

        private static SkillEffectDefinition[] ResolveTimedEffects(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            SkillMultiEffectTiming timing)
        {
            if (effects == null || effects.Length == 0)
            {
                return Array.Empty<SkillEffectDefinition>();
            }

            var resolved = new List<SkillEffectDefinition>();
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || effect.EffectTiming != timing
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }
    }
}


