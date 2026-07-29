using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 지속 범위 공격을 준비하고 생성한 오브젝트의 처리를 ZoneSkillActor에 맡긴다.
 */
namespace Pakuri.InGame
{

    internal static class ZoneSkillExecutor
    {
        // 범위 중심, 반지름, 배치 수, 지속시간을 조립하고 Actor 생성을 구현.
        /*
         * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
         */

        /*
         * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
         */

        /*
         * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
         */

        /*
         * 노드 피해 효과를 대상, 범위 또는 지속 영역에 적용한다.
         */


        /*
         * 추가 피해의 런타임 비주얼 충돌체를 사용해 범위 피해를 적용한다.
         */

        /*
         * 지속 피해 추가 효과용 Zone Actor를 생성하고 실행 정보를 전달한다.
         */

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

        /*
         * 재시전을 실행한다.
         */
        internal static bool ExecuteRecast(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData inheritedData /* 앞 실행에서 이어받은 스킬 강화 정보 */,
            RecastZoneNodeOp operation /* 재시전 노드 작업 */,
            Vector2 center /* 효과가 적용될 중심 위치 */)
        {
            var skill = context != null && context.Runtime != null
                ? context.Runtime.Data as ZoneSkillDefinition
                : null;
            if (skill == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || context.CasterEntry == null
                || context.Roster == null
                || (!string.IsNullOrWhiteSpace(operation.SourceSkillId)
                    && !string.Equals(operation.SourceSkillId, skill.SkillId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var maxGeneration = Math.Max(1, operation.MaxGeneration);
            if (context.RecastGeneration >= maxGeneration)
            {
                return false;
            }

            var snapshot = operation.InheritSnapshot
                ? inheritedData
                : new SkillExecutionData(skill);
            var radius = Radius(skill, snapshot) * Mathf.Max(0f, operation.RadiusMultiplier);
            var duration = Mathf.Max(0.05f, operation.DurationSeconds);
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
                operation.RadiusMultiplier);

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

        /*
         * 요청받은 지속 범위 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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

        /*
         * 배치 횟수를 결정한다.
         */
        private static int DeploymentCount(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            return 1 + (snapshot != null && snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
        }

        /*
         * 적중 대상 횟수를 결정한다.
         */
        private static int HitTargetCount(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            if (skill == null || skill.HitAllTargets || !skill.UsesHitTargetCount)
            {
                return int.MaxValue;
            }

            var baseCount = Math.Max(1, skill.HitTargetCount);
            var bonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Math.Max(1, baseCount + bonus);
        }

        /*
         * 범위 중심점을 결정한다.
         */
        private static List<Vector2> AreaCenters(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */,
            int deploymentCount /* 배치 개수 */)
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

        /*
         * 범위 중심점을 결정한다.
         */
        private static Vector2 AreaCenter(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */)
        {
            return SkillTargeting.AreaCenter(context, targeting, area);
        }

        /*
         * 반경을 결정한다.
         */
        private static float Radius(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillTargeting.Radius(
                SkillTargeting.BaseRadius(targeting, area),
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus);
        }

        /*
         * 지속시간을 결정한다.
         */
        private static float Duration(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 주기 간격을 결정한다.
         */
        private static float TickInterval(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 종료 효과를 결정한다.
         */

    }
}
