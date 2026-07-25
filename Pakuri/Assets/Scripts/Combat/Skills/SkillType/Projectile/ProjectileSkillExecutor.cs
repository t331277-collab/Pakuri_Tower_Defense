using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 투사체 스킬의 발사 수, 분기, 연속 발사와 후속 투사체를 구성한다.
 * 스킬 실행 데이터에서 피해와 상태 효과를 해석해 ProjectileSkillActor에 전달하고
 * 직접 적중 형식과 지연 충돌, 발사체별 보정도 함께 처리한다.
 */
namespace Pakuri.InGame
{

    internal static class ProjectileSkillExecutor
    {
        // 발사 수, 확산, 연속 발사, 후속 투사체를 조립하고 Actor 생성을 구현.
        /*
         * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
         */
        internal static bool ExecuteAdditionalEffects(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition[] effects /* 적용할 추가 효과 목록 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool requireTiming /* 특정 실행 시점만 처리할지 여부 */,
            SkillMultiEffectTiming timing /* 처리할 실행 시점 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */,
            int hitCount = 0 /* 현재 적중 횟수 */,
            UnitCombatState eventTarget = null /* 현재 적중 대상 */,
            bool useEventTarget = false /* 적중 대상을 문맥에 넣을지 여부 */)
        {
            if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
            {
                return false;
            }

            var effectContext = context;
            if (useEventTarget)
            {
                effectContext = new SkillExecutionContext(
                    context.CombatManager,
                    context.Roster,
                    context.CasterEntry,
                    context.Runtime,
                    eventTarget,
                    context.HasManualAimDirection,
                    context.ManualAimDirection,
                    context.HasManualTargetPoint,
                    context.ManualTargetPoint,
                    context.RecastGeneration);
            }

            var applied = false;
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!SkillRequirement.CanRunEffect(effectContext, effect))
                {
                    continue;
                }
                if (requireTiming)
                {
                    if (effect.EffectTiming != timing)
                    {
                        continue;
                    }
                }
                else if (effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || effect.EffectTiming == SkillMultiEffectTiming.OnDeploymentCast
                    || effect.EffectTiming == SkillMultiEffectTiming.OnExpire
                    || effect.EffectTiming == SkillMultiEffectTiming.OnHitCount)
                {
                    continue;
                }
                if (!SkillRequirement.MatchesEffectHitCount(effect, hitCount))
                {
                    continue;
                }

                if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
                {
                    effectContext.CombatManager.StartCoroutine(ApplyAdditionalEffectAfterDelay(
                        effectContext,
                        skillData,
                        effect,
                        defaultCenter,
                        scaleStatusDuration));
                    applied = true;
                }
                else
                {
                    applied = ApplyAdditionalEffect(
                        effectContext,
                        skillData,
                        effect,
                        defaultCenter,
                        scaleStatusDuration) || applied;
                }
            }
            return applied;
        }

        /*
         * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
         */
        private static IEnumerator ApplyAdditionalEffectAfterDelay(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
        {
            var delay = Mathf.Max(0f, effect.DelaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
            ApplyAdditionalEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
        }

        /*
         * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
         */
        private static bool ApplyAdditionalEffect(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return false;
            }

            if (effect.EffectKind == SkillMultiEffectKind.Damage)
            {
                return ZoneSkillExecutor.ApplyAdditionalDamageEffect(context, skillData, effect, defaultCenter);
            }
            if (effect.EffectKind == SkillMultiEffectKind.Status)
            {
                return SkillStatus.ApplyEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
            }
            if (effect.EffectKind == SkillMultiEffectKind.ExtendStatusDuration)
            {
                return SkillStatus.ExtendEffectDuration(context, effect);
            }
            if (effect.EffectKind == SkillMultiEffectKind.RecastZone)
            {
                return ZoneSkillExecutor.ExecuteRecast(context, skillData, effect, defaultCenter);
            }
            return false;
        }

        private static bool applyingHitEnhancement;

        /*
         * 적중 후 추가 피해, 연쇄 피해, 재장전 감소 강화 효과를 적용한다.
         */
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitRegistry roster /* 전투 유닛 목록 */,
            SkillUseState runtime /* 실행 중인 스킬 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            CombatUnitEntry sourceEntry /* 시전자 등록 정보 */,
            UnitCombatState source /* 시전자 */,
            string sourceSkillId /* 원본 스킬 식별자 */,
            CombatUnitEntry hitTarget /* 최초 적중 대상 */,
            Vector2 hitPosition /* 최초 적중 위치 */,
            float primaryBaseDamage /* 최초 적중 기본 피해 */)
        {
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
                        primaryBaseDamage * skillData.OnHitAdditionalDamageMultiplier,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ResolveChainTargets(
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
                                primaryBaseDamage * skillData.OnHitChainDamageMultiplier,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /*
         * 요청받은 투사체 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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

            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage, snapshot.BaseDamageBonus, snapshot.DamageMultiplier);
            var attribute = skill.Damage != null ? skill.Damage.Element : skill.Element;
            var currentBurstProjectileIndex = context.Runtime != null
                ? context.Runtime.ResolveCurrentBurstProjectileIndex()
                : 1;
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();

            var baseStatusSpec = SkillStatus.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var planEffects = skill.MultiEffects;
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
            var burstDamageMultiplier = ResolveBurstDamageMultiplier(
                skill,
                snapshot,
                currentBurstProjectileIndex,
                burstProjectileCount);
            var launchDamage = damage * burstDamageMultiplier;
            var isMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var lifetime = ResolveProjectileLifetime(skill);
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
                var rotation = EffectVisualBuilder.ResolveRotation(spreadDirection);
                var objectName = "Projectile";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    objectName = "Projectile_" + skill.SkillId;
                }

                var instance = effects.CreateEffect(
                    runtimeVisual,
                    null,
                    objectName,
                    origin,
                    rotation,
                    hitboxIsTrigger: true);
                if (instance == null)
                {
                    instance = effects.CreateSkillActorObject(objectName, origin, rotation);
                }

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
                    launchDamage,
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    statusSpec,
                    branchSpec,
                    SkillStatus.ResolveStatusSpec(skill.ImpactStatus, snapshot),
                    onHitEffects,
                    onExpireEffects,
                    skill.ContactDamageEnabled,
                    skill.StopOnFirstHit,
                    ResolveImpactDelay(skill, snapshot),
                    skill.ImpactRuntimeVisual,
                    skill.HasImpactArea,
                    SkillTargeting.ResolveRadius(
                        impactRadius,
                        snapshot.RadiusMultiplier,
                        snapshot.RadiusBonus),
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

            return true;
        }

        /*
         * 투사체 확산 방향을 결정한다.
         */
        private static Vector2 ResolveProjectileSpreadDirection(Vector2 direction /* 진행하거나 발사할 방향 */, int index /* 목록에서의 순서 번호 */, int count /* 처리할 개수 */)
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
        private static Vector2 RotateDirection(Vector2 direction /* 진행하거나 발사할 방향 */, float degrees /* 각도 */)
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
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            int projectileLaunchIndex /* 투사체 발사 순서 번호 */)
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
        private static float ResolveBranchChance(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, int projectileLaunchIndex /* 투사체 발사 순서 번호 */)
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
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            int projectileIndex /* 투사체 순서 번호 */,
            int burstProjectileCount /* 연속 발사 투사체 개수 */)
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
                multiplier *= SkillExecutionRuleResolver.ResolveBurstDamageMultiplier(snapshot, projectileIndex, burstProjectileCount);
            }

            return Mathf.Max(0f, multiplier);
        }

        /*
         * 현재 투사체가 연속 발사 보정 대상 순번인지 확인한다.
         */
        private static bool MatchesBurstProjectileIndex(int configuredIndex /* 설정된 순서 번호 */, int projectileIndex /* 투사체 순서 번호 */, int burstProjectileCount /* 연속 발사 투사체 개수 */)
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */,
            ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */,
            SkillEffectDefinition[] onHitEffects /* 발생 시 적중 효과 목록 */,
            SkillEffectDefinition[] onExpireEffects /* 발생 시 만료 효과 목록 */,
            Vector2 origin /* 시작 위치 */,
            Vector2 direction /* 진행하거나 발사할 방향 */,
            float speed /* 속도 */,
            float baseDamage /* 방어 계산 전 기본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            int pierce /* 관통 */,
            float boundary /* 경계 */,
            float lifetime /* 유지 시간 */,
            int burstProjectileCount /* 연속 발사 투사체 개수 */,
            int currentBurstProjectileIndex /* 현재 연속 발사 투사체 순서 번호 */)
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */,
            ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */,
            SkillEffectDefinition[] onHitEffects /* 발생 시 적중 효과 목록 */,
            SkillEffectDefinition[] onExpireEffects /* 발생 시 만료 효과 목록 */,
            Vector2 origin /* 시작 위치 */,
            Vector2 direction /* 진행하거나 발사할 방향 */,
            float speed /* 속도 */,
            float baseDamage /* 방어 계산 전 기본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            int pierce /* 관통 */,
            float boundary /* 경계 */,
            float lifetime /* 유지 시간 */)
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */,
            ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */,
            SkillEffectDefinition[] onHitEffects /* 발생 시 적중 효과 목록 */,
            SkillEffectDefinition[] onExpireEffects /* 발생 시 만료 효과 목록 */,
            Vector2 origin /* 시작 위치 */,
            Vector2 direction /* 진행하거나 발사할 방향 */,
            float speed /* 속도 */,
            float damage /* 적용하거나 전달할 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            int pierce /* 관통 */,
            float boundary /* 경계 */,
            float lifetime /* 유지 시간 */,
            bool isMagazineLastProjectile /* 여부 탄창 마지막 투사체 여부 */)
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
            var branchSpec = ResolveBranchDamageSpec(snapshot, projectileLaunchIndex);
            var rotation = EffectVisualBuilder.ResolveRotation(direction);
            var objectName = "Projectile";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "Projectile_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(
                runtimeVisual,
                null,
                objectName,
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
                SkillStatus.ResolveStatusSpec(skill.ImpactStatus, snapshot),
                onHitEffects,
                onExpireEffects,
                skill.ContactDamageEnabled,
                skill.StopOnFirstHit,
                ResolveImpactDelay(skill, snapshot),
                skill.ImpactRuntimeVisual,
                skill.HasImpactArea,
                SkillTargeting.ResolveRadius(
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

        /*
         * 직접 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyDirectStatus(
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusSpec, source);
        }

        /*
         * 직접 투사체 적중을 적용한다.
         */
        private static void ApplyDirectProjectileHit(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */,
            ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */,
            float damage /* 적용하거나 전달할 피해량 */,
            DamageAttribute attribute /* 피해 속성 */)
        {
            if (context == null || skill == null || target == null || target.Model == null)
            {
                return;
            }

            var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
            var resolvedDamage = damage;
            if (snapshot != null)
            {
                resolvedDamage *= SkillExecutionRuleResolver.ResolveConditionalDamageMultiplier(snapshot, target.Model);
            }
            if (context.Runtime != null && snapshot != null)
            {
                resolvedDamage *= context.Runtime.ResolveConsecutiveHitDamageMultiplier(target.Model, snapshot);
            }

            resolvedDamage = Mathf.Max(0f, resolvedDamage);
            var damageResult = context.CombatManager.ApplyDamage(
                target.Model,
                resolvedDamage,
                attribute,
                context.Caster,
                skill.Damage != null && skill.Damage.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                skill.SkillId);
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

        /*
         * 충돌 지연을 결정한다.
         */
        private static float ResolveImpactDelay(ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
            ProjectileStatusHitSpec baseStatusSpec /* 기본 상태 효과 설정 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            int projectileIndex /* 투사체 순서 번호 */,
            int burstProjectileCount /* 연속 발사 투사체 개수 */)
        {
            if (baseStatusSpec == null || snapshot == null)
            {
                return baseStatusSpec;
            }

            var stacksBonus = SkillExecutionRuleResolver.ResolveBurstStatusStacksBonus(snapshot, projectileIndex, burstProjectileCount);
            if (stacksBonus == 0)
            {
                return baseStatusSpec;
            }

            return CloneStatusSpecWithStacks(baseStatusSpec, Mathf.Max(1, baseStatusSpec.Stacks + stacksBonus));
        }

        /*
         * 상태 설정 포함 중첩을 복사본을 생성한다.
         */
        private static ProjectileStatusHitSpec CloneStatusSpecWithStacks(ProjectileStatusHitSpec source /* 효과를 발생시킨 원본 */, int stacks /* 중첩 수 */)
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

        /*
         * 시간 기반 효과를 결정한다.
         */
        private static SkillEffectDefinition[] ResolveTimedEffects(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            SkillEffectDefinition[] effects /* 실행할 효과 목록 */,
            SkillMultiEffectTiming timing /* 실행 시점 */)
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
                    || !SkillRequirement.CanRunEffect(context, effect))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }

        /*
         * ResolveProjectileLifetime 결과를 계산해 반환한다.
         */
        private static float ResolveProjectileLifetime(ProjectileSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
