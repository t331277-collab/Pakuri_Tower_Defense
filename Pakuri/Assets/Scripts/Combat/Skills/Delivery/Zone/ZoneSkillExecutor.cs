/*
 * 역할: Zone 스킬 전달 조정.
 * 책임: 설정된 Zone Actor와 비주얼을 생성하고 주기 적중을 스킬 실행에 전달한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>ZoneSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class ZoneSkillExecutor
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Recast</c>를 실행한다.</summary>
        internal static bool ExecuteRecast(
            SkillExecutionContext context,
            SkillExecutionData inheritedData,
            SkillTriggerCommand command,
            Vector2 center)
        {
            var skill = context != null && context.Runtime != null
                ? context.Runtime.Data as ZoneSkillDefinition
                : null;
            if (skill == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || context.CasterEntry == null
                || context.Roster == null
                || (!string.IsNullOrWhiteSpace(command.TargetId)
                    && !string.Equals(command.TargetId, skill.SkillId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var maxGeneration = Math.Max(1, command.MaxGeneration);
            if (context.RecastGeneration >= maxGeneration)
            {
                return false;
            }

            var snapshot = command.InheritSnapshot
                ? inheritedData
                : new SkillExecutionData(skill);
            var radius = Radius(skill, snapshot) * Mathf.Max(0f, command.RadiusMultiplier);
            var duration = Mathf.Max(0.05f, command.DurationSeconds);
            var tickInterval = TickInterval(skill, snapshot);
            var hitTargetCount = HitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnTickStatus, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            var objectName = "InGameRecastZone";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "InGameRecastZone_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                prefab,
                objectName,
                center,
                Quaternion.identity,
                null,
                0f,
                null,
                false,
                true,
                true));

            EffectVisualBuilder.ConfigureAreaEffect(
                instance,
                SkillTargeting.BaseRadius(skill.Targeting, skill.Area),
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus,
                command.RadiusMultiplier);

            var actor = instance.GetComponent<ZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ZoneSkillActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                center,
                radius,
                coverAll,
                duration,
                tickInterval,
                hitTargetCount,
                damage,
                attribute,
                statusSpec,
                context.Runtime,
                snapshot,
                context.Caster,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                context.RecastGeneration + 1);
            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill)
        {
            var deploymentCount = DeploymentCount(snapshot);
            var centers = AreaCenters(context, skill.Targeting, skill.Area, deploymentCount);
            var radius = Radius(skill, snapshot);
            var duration = Duration(skill, snapshot);
            var tickInterval = TickInterval(skill, snapshot);
            var hitTargetCount = HitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnTickStatus, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }

            var routed = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                var objectName = "ZoneSkill";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    objectName = "ZoneSkill_" + skill.SkillId;
                }

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    runtimeVisual,
                    prefab,
                    objectName,
                    center,
                    Quaternion.identity,
                    null,
                    0f,
                    null,
                    false,
                    true,
                    true));

                EffectVisualBuilder.ConfigureAreaEffect(
                    instance,
                    SkillTargeting.BaseRadius(skill.Targeting, skill.Area),
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus);

                var actor = instance.GetComponent<ZoneSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ZoneSkillActor>();
                }

                actor.Initialize(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    center,
                    radius,
                    coverAll,
                    duration,
                    tickInterval,
                    hitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    context.Runtime,
                    snapshot,
                    context.Caster,
                    skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
                routed = true;
            }

            return routed;
        }

        /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>DeploymentCount</c> 결과값을 생성해 반환한다.</summary>
        private static int DeploymentCount(SkillExecutionData snapshot)
        {
            return 1 + (snapshot != null && snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>HitTargetCount</c> 결과값을 생성해 반환한다.</summary>
        private static int HitTargetCount(ZoneSkillDefinition skill, SkillExecutionData snapshot)
        {
            if (skill == null || skill.HitAllTargets || !skill.UsesHitTargetCount)
            {
                return int.MaxValue;
            }

            var baseCount = Math.Max(1, skill.HitTargetCount);
            var bonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Math.Max(1, baseCount + bonus);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>AreaCenters</c> 결과값을 생성해 반환한다.</summary>
        private static List<Vector2> AreaCenters(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area,
            int deploymentCount)
        {
            var primaryCenter = AreaCenter(context, targeting, area);
            var coverAll = (area != null && area.CoverAll)
                || (targeting != null && targeting.CoverAll);
            return SkillTargeting.TargetAnchoredCenters(
                context,
                targeting,
                primaryCenter,
                deploymentCount,
                coverAll,
                SkillDeploymentRepeatMode.RandomExisting);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>AreaCenter</c> 결과값을 생성해 반환한다.</summary>
        private static Vector2 AreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            return SkillTargeting.AreaCenter(context, targeting, area);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Radius</c> 결과값을 생성해 반환한다.</summary>
        private static float Radius(ZoneSkillDefinition skill, SkillExecutionData snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillTargeting.Radius(
                SkillTargeting.BaseRadius(targeting, area),
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Duration</c> 결과값을 생성해 반환한다.</summary>
        private static float Duration(ZoneSkillDefinition skill, SkillExecutionData snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var duration = area != null && area.Duration > 0f
                ? area.Duration
                : timing != null ? timing.ActiveDuration : 0f;
            if (duration <= 0f)
            {
                duration = TickInterval(skill, snapshot);
            }

            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Interval</c>를 경과 시간 기준으로 갱신한다.</summary>
        private static float TickInterval(ZoneSkillDefinition skill, SkillExecutionData snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var interval = area != null && area.TickInterval > 0f
                ? area.TickInterval
                : timing != null && timing.TickInterval > 0f ? timing.TickInterval : 1f;
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

    }
}
