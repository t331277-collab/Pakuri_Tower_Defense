using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 투사체 스킬의 발사 수, 분기, 연속 발사와 후속 투사체를 구성한다.
 * 스킬 Snapshot에서 피해와 상태 효과를 해석해 Projectile Actor에 전달하고
 * 직접 적중 형식과 지연 충돌, 발사체별 보정도 함께 처리한다.
 */
namespace Pakuri.InGame
{

    public sealed class ProjectileSkillExecutor : TypedSkillExecutor<ProjectileSkillRuntimeData>
    {
        /*
         * 요청받은 투사체 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as ProjectileSkillRuntimeData : null;
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
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = effects != null && effects.HasVisual(runtimeVisual);

            var baseStatusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            var onHitEffects = ResolveTimedEffects(context, snapshot, planEffects, SkillMultiEffectTiming.OnHit);
            var onExpireEffects = ResolveTimedEffects(context, snapshot, planEffects, SkillMultiEffectTiming.OnExpire);
            var projectile = skill.Projectile;
            var burstProjectileCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var requiresProjectileActor = skill.StopOnFirstHit
                || skill.HasImpactArea
                || skill.ImpactDelaySeconds > 0f
                || hasRuntimeVisual
                || onHitEffects.Length > 0
                || onExpireEffects.Length > 0;
            if (!hasRuntimeVisual && !requiresProjectileActor)
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
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ResolveProjectileSpreadDirection(direction, i, projectileCount);
                var boundary = ProjectileSkillActor.ResolveDestroyBoundaryX(
                    origin,
                    spreadDirection,
                    speed,
                    lifetime);
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
                var branchSpec = ResolveBranchDamageSpec(snapshot, projectileLaunchIndex);
                var rotation = SkillExecutionUtility.ResolveRotation(spreadDirection);
                var instance = effects.CreateEffectObject(
                    runtimeVisual,
                    null,
                    string.IsNullOrWhiteSpace(skill.SkillId)
                        ? "Projectile"
                        : $"Projectile_{skill.SkillId}",
                    origin,
                    rotation,
                    createEmptyObject: true,
                    hitboxIsTrigger: true);
                if (instance == null)
                {
                    if (target != null)
                    {
                        var directStatusSpec = ResolveBurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                        ApplyDirectProjectileHit(context, skill, snapshot, target, directStatusSpec, launchDamage, attribute);
                    }

                    continue;
                }

                var actor = instance.GetComponent<ProjectileSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ProjectileSkillActor>();
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
                    skill.ImpactRuntimeVisual,
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
                runtimeVisual,
                baseStatusSpec,
                onHitEffects,
                onExpireEffects,
                origin,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                ProjectileSkillActor.ResolveDestroyBoundaryX(
                    origin,
                    direction,
                    speed,
                    lifetime),
                lifetime,
                burstProjectileCount,
                currentBurstProjectileIndex);

            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        /*
         * 투사체 확산 방향을 결정한다.
         */
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

        /*
         * 기준 방향을 지정한 각도만큼 회전한다.
         */
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
        /*
         * 분기 피해 설정을 결정한다.
         */
        private static ProjectileBranchDamageSpec ResolveBranchDamageSpec(
            SkillExecutionSnapshot snapshot,
            int projectileLaunchIndex)
        {
            if (snapshot == null || !snapshot.HasBranchBehavior)
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

            return new ProjectileBranchDamageSpec
            {
                Enabled = true,
                Chance = Mathf.Clamp01(chance),
                Count = Math.Max(1, count),
                DamageMultiplier = snapshot.HasBranchDamageMultiplier ? Mathf.Max(0f, snapshot.BranchDamageMultiplier) : 1f,
                SearchRadius = Mathf.Max(0f, radius)
            };
        }

        /*
         * 분기 확률을 결정한다.
         */
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

        /*
         * 연속 발사 피해 배율을 결정한다.
         */
        private static float ResolveBurstDamageMultiplier(
            ProjectileSkillRuntimeData skill,
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

        /*
         * 현재 투사체가 연속 발사 보정 대상 순번인지 확인한다.
         */
        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex > 0 && configuredIndex == projectileIndex;
        }

        /*
         * 후속 투사체를 예약하고 성공 여부를 반환한다.
         */
        private static void TryScheduleFollowUpProjectile(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillRuntimeData skill,
            RuntimeSkillVisualSpec runtimeVisual,
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
                || context.CombatManager.Effects == null
                || skill == null
                || snapshot == null
                || !snapshot.HasFollowUpProjectile
                || !context.CombatManager.Effects.HasVisual(runtimeVisual)
                || currentBurstProjectileIndex < burstProjectileCount)
            {
                return;
            }

            context.CombatManager.StartCoroutine(ExecuteFollowUpProjectilesAfterDelay(
                context,
                snapshot,
                skill,
                runtimeVisual,
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

        /*
         * 지연시간 후 후속 투사체를 발사한다.
         */
        private static IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillRuntimeData skill,
            RuntimeSkillVisualSpec runtimeVisual,
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

            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || skill == null
                || !context.CombatManager.Effects.HasVisual(runtimeVisual))
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
                    runtimeVisual,
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

        /*
         * 투사체를 생성한다.
         */
        private static void SpawnProjectileActor(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            ProjectileSkillRuntimeData skill,
            RuntimeSkillVisualSpec runtimeVisual,
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
            if (context == null
                || context.CombatManager == null
                || skill == null
                || context.CombatManager.Effects == null
                || !context.CombatManager.Effects.HasVisual(runtimeVisual))
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
            var branchSpec = ResolveBranchDamageSpec(snapshot, projectileLaunchIndex);
            var rotation = SkillExecutionUtility.ResolveRotation(direction);
            var instance = effects.CreateEffectObject(
                runtimeVisual,
                null,
                string.IsNullOrWhiteSpace(skill.SkillId)
                    ? "Projectile"
                    : $"Projectile_{skill.SkillId}",
                origin,
                rotation,
                hitboxIsTrigger: true);
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<ProjectileSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillActor>();
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
                skill.ImpactRuntimeVisual,
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

        /*
         * 직접 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyDirectStatus(
            InGameCombatManager combatManager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(combatManager, target, statusSpec, source);
        }

        /*
         * 직접 투사체 적중을 적용한다.
         */
        private static void ApplyDirectProjectileHit(
            SkillExecutionContext context,
            ProjectileSkillRuntimeData skill,
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

        /*
         * 충돌 지연을 결정한다.
         */
        private static float ResolveImpactDelay(ProjectileSkillRuntimeData skill, SkillExecutionSnapshot snapshot)
        {
            var delay = skill != null ? skill.ImpactDelaySeconds : 0f;
            if (snapshot != null)
            {
                delay *= Mathf.Max(0f, snapshot.DamageDelayMultiplier);
            }

            return Mathf.Max(0f, delay);
        }

        /*
         * 연속 발사 상태 설정을 결정한다.
         */
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

        /*
         * 상태 설정 포함 중첩을 복사본을 생성한다.
         */
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

        /*
         * 시간 기반 효과를 결정한다.
         */
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


