/*
 * 역할: 투사체 스킬 전달 조정.
 * 책임: 투사체 시전·연사·비주얼·Actor를 구성하고 충돌 결과를 스킬 실행에 전달한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>ProjectileSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class ProjectileSkillExecutor
    {

        private static bool applyingHitEnhancement;

        /// <summary>전달된 런타임 입력값을 사용해 <c>HitEnhancements</c>를 적용한다.</summary>
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager,
            UnitSpawnManager roster,
            SkillUseState runtime,
            SkillExecutionData skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillId,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager != null && roster != null && source != null && hitTarget != null && hitTarget.Model != null)
            {
                var actionExecutionContext = new SkillExecutionContext(
                    manager,
                    roster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnHit,
                    new SkillActionContext(
                        source,
                        sourceSkillId,
                        hitTarget.Model,
                        hitPosition,
                        primaryBaseDamage,
                        1,
                        skillData,
                        actionExecutionContext));
            }

            if (manager == null
                || roster == null
                || skillData == null
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingHitEnhancement)
            {
                return;
            }

            var hasReloadReduction = !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
                && skillData.ReloadReduceSecondsPerHit > 0f;
            if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
            {
                return;
            }

            var hitIndex = 0;
            if (runtime != null)
            {
                hitIndex = runtime.AdvanceSkillHitCount();
            }

            applyingHitEnhancement = true;
            try
            {
                if (hasReloadReduction && runtime != null && runtime.Owner != null && runtime.Owner.Skills != null)
                {
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillId(skillData.ReloadReduceTargetSkillId);
                    if (reloadSkill != null && reloadSkill.IsReloading)
                    {
                        reloadSkill.ReduceReloadRemaining(skillData.ReloadReduceSecondsPerHit);
                    }
                }

                var targetsHitUnit = string.IsNullOrWhiteSpace(skillData.OnHitAdditionalDamageTarget)
                    || string.Equals(skillData.OnHitAdditionalDamageTarget, "HitTarget", StringComparison.OrdinalIgnoreCase);
                if (skillData.HasOnHitAdditionalDamage
                    && skillData.OnHitAdditionalDamageMultiplier > 0f
                    && targetsHitUnit
                    && hitTarget.IsAlive
                    && UnityEngine.Random.value <= Mathf.Clamp01(skillData.OnHitAdditionalDamageChance))
                {
                    manager.ApplyDamage(
                        hitTarget.Model,
                        primaryBaseDamage,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier: skillData.OnHitAdditionalDamageMultiplier);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ChainTargets(
                        roster,
                        sourceEntry,
                        source,
                        hitTarget,
                        hitPosition,
                        skillData.OnHitChainSearchRadius);
                    var targetCount = Mathf.Min(skillData.OnHitChainTargetCount, chainTargets.Count);
                    for (var i = 0; i < targetCount; i++)
                    {
                        var chainTarget = chainTargets[i];
                        if (chainTarget != null && chainTarget.IsAlive && chainTarget.Model != null)
                        {
                            manager.ApplyDamage(
                                chainTarget.Model,
                                primaryBaseDamage,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true,
                                finalDamageMultiplier: skillData.OnHitChainDamageMultiplier);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill)
        {
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillTargeting.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                if (!context.HasManualAimDirection)
                {
                    return false;
                }

                direction = Vector2.right;
            }

            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
            var attribute = skill.Damage != null ? skill.Damage.Element : skill.Element;
            var currentBurstProjectileIndex = context.Runtime != null
                ? context.Runtime.CurrentBurstProjectileIndex()
                : 1;
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();

            var baseStatusSpec = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
            var projectile = skill.Projectile;
            var burstProjectileCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var requiresProjectileActor = skill.StopOnFirstHit
                || skill.HasImpactArea
                || skill.ImpactDelaySeconds > 0f
                || hasRuntimeVisual;
            if (!hasRuntimeVisual && !requiresProjectileActor)
            {
                if (target != null)
                {
                    var directStatusSpec = BurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                    ApplyDirectProjectileHit(context, skill, snapshot, target, directStatusSpec, damage, attribute);
                    return true;
                }

                return context.HasManualAimDirection;
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
            var burstDamageMultiplier = BurstDamageMultiplier(
                skill,
                snapshot,
                currentBurstProjectileIndex,
                burstProjectileCount);
            var launchSnapshot = snapshot.CopyWithDamageMultiplier(burstDamageMultiplier);
            var isMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var lifetime = ProjectileLifetime(skill);
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ProjectileSpreadDirection(direction, i, projectileCount);
                var boundary = ProjectileSkillActor.DestroyBoundaryX(
                    origin,
                    spreadDirection,
                    speed,
                    lifetime);
                if (effects == null)
                {
                    if (target != null)
                    {
                        var directStatusSpec = BurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                        ApplyDirectProjectileHit(context, skill, launchSnapshot, target, directStatusSpec, damage, attribute);
                    }

                    continue;
                }

                var projectileLaunchIndex = context.Runtime != null
                    ? context.Runtime.AdvanceProjectileLaunchCount()
                    : 0;
                var branchSpec = BranchDamageSpec(snapshot, projectileLaunchIndex);
                var rotation = EffectVisualBuilder.Rotation(spreadDirection);
                var objectName = "Projectile";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    objectName = "Projectile_" + skill.SkillId;
                }

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    runtimeVisual,
                    null,
                    objectName,
                    origin,
                    rotation,
                    null,
                    0f,
                    null,
                    true,
                    true,
                    true));

                if (instance == null)
                {
                    if (target != null)
                    {
                        var directStatusSpec = BurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                        ApplyDirectProjectileHit(context, skill, launchSnapshot, target, directStatusSpec, damage, attribute);
                    }

                    continue;
                }

                var actor = instance.GetComponent<ProjectileSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ProjectileSkillActor>();
                }

                var statusSpec = BurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                var impactRadius = 0f;
                if (skill.ImpactArea != null)
                {
                    impactRadius = skill.ImpactArea.Radius;
                }
                actor.Initialize(
                    context.CombatManager,
                    context.Caster,
                    spreadDirection,
                    speed,
                    damage,
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    statusSpec,
                    branchSpec,
                    SkillStatus.StatusSpec(skill.ImpactStatus, snapshot),
                    skill.ContactDamageEnabled,
                    skill.StopOnFirstHit,
                    ImpactDelay(skill, snapshot),
                    skill.ImpactRuntimeVisual,
                    skill.HasImpactArea,
                    SkillTargeting.Radius(
                        impactRadius,
                        snapshot.RadiusMultiplier,
                        snapshot.RadiusBonus),
                    damage,
                    context.Runtime,
                    launchSnapshot,
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
                origin,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                ProjectileSkillActor.DestroyBoundaryX(
                    origin,
                    direction,
                    speed,
                    lifetime),
                lifetime,
                burstProjectileCount,
                currentBurstProjectileIndex);

            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ProjectileSpreadDirection</c> 결과값을 생성해 반환한다.</summary>
        private static Vector2 ProjectileSpreadDirection(Vector2 direction, int index, int count)
        {
            if (count <= 1)
            {
                return direction;
            }

            const float angleStep = 10f;
            var offset = (index - (count - 1) * 0.5f) * angleStep;
            return RotateDirection(direction, offset);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>RotateDirection</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>BranchDamageSpec</c> 결과값을 생성해 반환한다.</summary>
        private static ProjectileBranchDamageSpec BranchDamageSpec(
            SkillExecutionData snapshot,
            int projectileLaunchIndex)
        {
            if (snapshot == null || !snapshot.HasBranchBehavior)
            {
                return null;
            }

            var chance = BranchChance(snapshot, projectileLaunchIndex);
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>BranchChance</c> 결과값을 생성해 반환한다.</summary>
        private static float BranchChance(SkillExecutionData snapshot, int projectileLaunchIndex)
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>BurstDamageMultiplier</c> 결과값을 생성해 반환한다.</summary>
        private static float BurstDamageMultiplier(
            ProjectileSkillDefinition skill,
            SkillExecutionData snapshot,
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
                multiplier *= SkillExecutionRuleResolver.BurstDamageMultiplier(snapshot, projectileIndex, burstProjectileCount);
            }

            return Mathf.Max(0f, multiplier);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesBurstProjectileIndex</c> 조건을 평가하고 결과를 반환한다.</summary>
        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex > 0 && configuredIndex == projectileIndex;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ScheduleFollowUpProjectile</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static void TryScheduleFollowUpProjectile(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
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
                || runtimeVisual == null
                || !runtimeVisual.HasVisual()
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
                origin,
                direction,
                speed,
                baseDamage,
                attribute,
                pierce,
                boundary,
                lifetime));
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>FollowUpProjectilesAfterDelay</c>를 실행한다.</summary>
        private static IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
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
                || runtimeVisual == null
                || !runtimeVisual.HasVisual())
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ProjectileActor</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        private static void SpawnProjectileActor(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
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
                || runtimeVisual == null
                || !runtimeVisual.HasVisual())
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
            var branchSpec = BranchDamageSpec(snapshot, projectileLaunchIndex);
            var rotation = EffectVisualBuilder.Rotation(direction);
            var objectName = "Projectile";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "Projectile_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                null,
                objectName,
                origin,
                rotation,
                null,
                0f,
                null,
                true,
                true,
                false));
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<ProjectileSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillActor>();
            }

            var impactRadius = 0f;
            if (skill.ImpactArea != null)
            {
                impactRadius = skill.ImpactArea.Radius;
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
                SkillStatus.StatusSpec(skill.ImpactStatus, snapshot),
                skill.ContactDamageEnabled,
                skill.StopOnFirstHit,
                ImpactDelay(skill, snapshot),
                skill.ImpactRuntimeVisual,
                skill.HasImpactArea,
                SkillTargeting.Radius(
                    impactRadius,
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus),
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplyDirectStatus</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static void TryApplyDirectStatus(
            InGameCombatManager combatManager,
            UnitCombatState target,
            ProjectileStatusHitSpec statusSpec,
            UnitCombatState source)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusSpec, source);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>DirectProjectileHit</c>를 적용한다.</summary>
        private static void ApplyDirectProjectileHit(
            SkillExecutionContext context,
            ProjectileSkillDefinition skill,
            SkillExecutionData snapshot,
            CombatUnitEntry target,
            ProjectileStatusHitSpec statusSpec,
            float damage,
            DamageAttribute attribute)
        {
            if (context == null || skill == null || target == null || target.Model == null)
            {
                return;
            }

            var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
            var resolvedDamage = Mathf.Max(0f, damage);
            var finalDamageMultiplier = snapshot != null
                ? Mathf.Max(0f, snapshot.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(snapshot, target.Model)
                : 1f;
            if (context.Runtime != null && snapshot != null)
            {
                finalDamageMultiplier *= context.Runtime.ConsecutiveHitDamageMultiplier(target.Model, snapshot);
            }

            var damageResult = context.CombatManager.ApplyDamage(
                target.Model,
                resolvedDamage,
                attribute,
                context.Caster,
                skill.Damage != null && skill.Damage.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                skill.SkillId,
                finalDamageMultiplier: finalDamageMultiplier);
            if (!damageResult.IsDead)
            {
                TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
            }
            ProjectileSkillExecutor.ApplyHitEnhancements(
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ImpactDelay</c> 결과값을 생성해 반환한다.</summary>
        private static float ImpactDelay(ProjectileSkillDefinition skill, SkillExecutionData snapshot)
        {
            var delay = skill != null ? skill.ImpactDelaySeconds : 0f;
            if (snapshot != null)
            {
                delay *= Mathf.Max(0f, snapshot.DamageDelayMultiplier);
            }

            return Mathf.Max(0f, delay);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>BurstStatusSpec</c> 결과값을 생성해 반환한다.</summary>
        private static ProjectileStatusHitSpec BurstStatusSpec(
            ProjectileStatusHitSpec baseStatusSpec,
            SkillExecutionData snapshot,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (baseStatusSpec == null || snapshot == null)
            {
                return baseStatusSpec;
            }

            var stacksBonus = SkillExecutionRuleResolver.BurstStatusStacksBonus(snapshot, projectileIndex, burstProjectileCount);
            if (stacksBonus == 0)
            {
                return baseStatusSpec;
            }

            return CloneStatusSpecWithStacks(baseStatusSpec, Mathf.Max(1, baseStatusSpec.Stacks + stacksBonus));
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>CloneStatusSpecWithStacks</c> 결과값을 생성해 반환한다.</summary>
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
                ThresholdSourceStatusKind = source.ThresholdSourceStatusKind,
                ThresholdSourceMinStacks = source.ThresholdSourceMinStacks,
                ThresholdStatusSpec = source.ThresholdStatusSpec
            };
        }

        /// <summary>전달된 <c>skill</c> 값을 사용해 <c>ProjectileLifetime</c> 결과값을 생성해 반환한다.</summary>
        private static float ProjectileLifetime(ProjectileSkillDefinition skill)
        {
            var projectile = skill.Projectile;
            if (projectile.LifetimeSeconds > 0f)
            {
                return projectile.LifetimeSeconds;
            }

            var speed = Mathf.Max(0.1f, projectile.ProjectileSpeed);
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
        }
    }
}
