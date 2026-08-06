/*
 * 역할: 일정 공간에 남는 공격의 실제 판정을 진행한다.
 * 범위 충돌과 주기 피해, 상태, 재시전 스킬, 표현 수명과 종료 사건을 처리한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 영역이 유지되는 동안 주기적인 적중 결과를 전투에 반영한다.
    public class ZoneSkillActor : MonoBehaviour
    {

        private InGameCombatManager combatManager;
        private CombatUnitEntry casterEntry;
        private UnitSpawnManager roster;
        private SkillTargetingSpec targeting;
        private Vector2 center;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private StatusApplicationSpec statusSpec;
        private SkillExecutionState runtime;
        private SkillExecutionState snapshot;
        private UnitCombatState sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private int recastGeneration;
        private static bool applyingHitEnhancement;

        /// 확정된 영역과 적중 기준으로 첫 주기를 시작한다.
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onTickStatus,
            SkillExecutionState sourceRuntime,
            SkillExecutionState executionData,
            UnitCombatState source,
            bool allowCritical,
            float criticalChanceBonus,
            float criticalDamageBonus,
            int generation = 0)
        {
            combatManager = manager;
            casterEntry = sourceEntry;
            roster = unitRoster;
            targeting = targetingSpec;
            center = areaCenter;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.05f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onTickStatus;
            runtime = sourceRuntime;
            snapshot = executionData;
            sourceModel = source;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            recastGeneration = Mathf.Max(0, generation);
            prefabHitboxColliders = GetComponentsInChildren<Collider2D>();
            ApplyCurrentAreaTick();
        }

        /// 다음 적용 주기와 영역 종료 시점을 진행한다.
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
            tickRemaining -= deltaTime;
            while (remainingDuration > 0f && tickRemaining <= 0f)
            {
                tickRemaining += tickInterval;
                ApplyCurrentAreaTick();
            }

            if (remainingDuration <= 0f)
            {
                TryExecuteExpireEffects();
                combatManager.Effects.RemoveEffect(gameObject);
            }
        }

        /// 영역의 마지막 위치와 재시전 세대를 후속 반응에 알린다.
        private void TryExecuteExpireEffects()
        {
            if (combatManager != null && casterEntry != null && roster != null)
            {
                var lifecycleContext = new SkillExecutionContext(
                    combatManager,
                    roster,
                    casterEntry,
                    runtime,
                    recastGeneration: recastGeneration);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillExecutionContext(
                        casterEntry.Model,
                        SourceSkillName(snapshot, runtime),
                        null,
                        center,
                        0f,
                        0,
                        snapshot,
                        lifecycleContext));
            }
        }

        /// 물리 충돌 영역과 겹친 대상을 이번 주기 결과로 확정한다.
        private bool ApplyCurrentAreaTick()
        {
            return ApplyColliderAreaTick(
                combatManager,
                casterEntry,
                roster,
                targeting,
                center,
                prefabHitboxColliders,
                damage,
                attribute,
                statusSpec,
                sourceModel,
                SourceSkillName(snapshot, runtime),
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                snapshot);
        }

        /// 실제 충돌 영역과 겹친 대상만 이번 주기 결과로 확정한다.
        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            Collider2D[] hitboxColliders,
            float damagePerTick,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillName,
            SkillExecutionState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionState executionData)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(sourceEntry, unitRoster, targetingSpec);
            var eligibleTargets = new List<CombatUnitEntry>();
            UnitCollisionResolver.CollectTargets(
                unitRoster,
                candidates,
                hitboxColliders,
                Vector2.zero,
                eligibleTargets);

            if (executionData != null && executionData.PullDistancePerTick > 0f)
            {
                for (var i = 0; i < eligibleTargets.Count; i++)
                {
                    var target = eligibleTargets[i];
                    if (target == null || !target.IsAlive || target.Transform == null)
                    {
                        continue;
                    }

                    target.Transform.position = Vector2.MoveTowards(
                        target.Transform.position,
                        areaCenter,
                        executionData.PullDistancePerTick);
                }
            }

            var routed = ApplyResolvedTargets(
                manager,
                sourceEntry,
                unitRoster,
                eligibleTargets,
                damagePerTick,
                damageAttribute,
                onHitStatus,
                source,
                sourceSkillName,
                sourceRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
            return routed;
        }

        /// 충돌이 확정된 모든 대상에 피해, 상태, 후속 사건을 같은 순서로 적용한다.
        internal static bool ApplyResolvedTargets(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            IReadOnlyList<CombatUnitEntry> eligibleTargets,
            float damage,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillName,
            SkillExecutionState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionState executionData)
        {
            if (manager == null
                || eligibleTargets == null
                || eligibleTargets.Count == 0)
            {
                return false;
            }

            var routed = false;
            for (var i = 0; i < eligibleTargets.Count; i++)
            {
                var target = eligibleTargets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                var hitPosition = target.Transform != null
                    ? (Vector2)target.Transform.position
                    : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier =
                    SkillExecutionRules.ResolveHitDamageMultiplier(
                        executionData,
                        target.Model);
                var resolvedCritChance = critChanceBonus;
                var resolvedCritDamage = critDamageBonus;
                SkillExecutionRules.ResolveHitCritModifiers(executionData, target.Model, manager.Units, ref resolvedCritChance, ref resolvedCritDamage);
                var result = manager.ApplyDamage(
                    target.Model,
                    resolvedDamage,
                    damageAttribute,
                    source,
                    criticalAllowed,
                    resolvedCritChance,
                    resolvedCritDamage,
                    sourceSkillName,
                    false,
                    false,
                    null,
                    finalDamageMultiplier,
                    SkillExecutionRules.ResolveHitFinalDamageModifier(executionData, target.Model, manager.Units),
                    executionData != null ? executionData.CriticalFinalDamageModifier : 1f,
                    isTrigger: executionData != null && executionData.IsTrigger);
                if (!result.IsDead)
                {
                    StatusCombatRules.ApplyStatus(
                        manager,
                        target.Model,
                        onHitStatus,
                        source);
                }
                PublishHitOutcome(
                    manager,
                    sourceRuntime != null ? unitRoster : null,
                    sourceRuntime,
                    executionData,
                    sourceEntry,
                    source,
                    sourceSkillName,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        /// 물리적 적중을 반응과 추가 피해, 연쇄 피해의 공통 출발점으로 삼는다.
        internal static void PublishHitOutcome(
            InGameCombatManager manager,
            UnitSpawnManager unitRoster,
            SkillExecutionState runtime,
            SkillExecutionState skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillName,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager != null
                && unitRoster != null
                && source != null
                && hitTarget != null
                && hitTarget.Model != null)
            {
                var actionExecutionContext = new SkillExecutionContext(
                    manager,
                    unitRoster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model,
                    publishSkillLifecycleEvents: runtime != null,
                    sourceSkillName: sourceSkillName);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnHit,
                    new SkillExecutionContext(
                        source,
                        sourceSkillName,
                        hitTarget.Model,
                        hitPosition,
                        primaryBaseDamage,
                        1,
                        skillData,
                        actionExecutionContext));
            }

            if (manager == null
                || unitRoster == null
                || skillData == null
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingHitEnhancement)
            {
                return;
            }

            var hasReloadReduction =
                !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillName)
                && skillData.ReloadReduceSecondsPerHit > 0f;
            if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
            {
                return;
            }

            var hitIndex = runtime != null
                ? SkillExecution.AdvanceSkillHitCount(runtime)
                : 0;
            applyingHitEnhancement = true;
            try
            {
                if (hasReloadReduction
                    && runtime != null
                    && runtime.Owner != null
                    && runtime.Owner.Skills != null)
                {
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillName(
                        skillData.ReloadReduceTargetSkillName);
                    if (reloadSkill != null && reloadSkill.IsReloading)
                    {
                        SkillExecution.ReduceReloadRemaining(
                            reloadSkill,
                            skillData.ReloadReduceSecondsPerHit);
                    }
                }

                var targetsHitUnit = string.IsNullOrWhiteSpace(
                        skillData.OnHitAdditionalDamageTarget)
                    || string.Equals(
                        skillData.OnHitAdditionalDamageTarget,
                        "HitTarget",
                        StringComparison.OrdinalIgnoreCase);
                if (skillData.HasOnHitAdditionalDamage
                    && skillData.OnHitAdditionalDamageMultiplier > 0f
                    && targetsHitUnit
                    && hitTarget.IsAlive
                    && UnityEngine.Random.value <= Mathf.Clamp01(
                        skillData.OnHitAdditionalDamageChance))
                {
                    manager.ApplyDamage(
                        hitTarget.Model,
                        primaryBaseDamage,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillName,
                        true,
                        false,
                        null,
                        damageMultiplier: skillData.OnHitAdditionalDamageMultiplier
                            * SkillExecutionRules.ResolveHitDamageMultiplier(skillData, hitTarget.Model),
                        finalDamageModifier: SkillExecutionRules.ResolveHitFinalDamageModifier(
                            skillData,
                            hitTarget.Model,
                            manager.Units),
                        isTrigger: skillData.IsTrigger);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ChainTargets(
                        unitRoster,
                        sourceEntry,
                        source,
                        hitTarget,
                        hitPosition,
                        skillData.OnHitChainSearchRadius);
                    var targetCount = Mathf.Min(
                        skillData.OnHitChainTargetCount,
                        chainTargets.Count);
                    for (var i = 0; i < targetCount; i++)
                    {
                        var chainTarget = chainTargets[i];
                        if (chainTarget != null
                            && chainTarget.IsAlive
                            && chainTarget.Model != null)
                        {
                            manager.ApplyDamage(
                                chainTarget.Model,
                                primaryBaseDamage,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillName,
                                true,
                                false,
                                null,
                                damageMultiplier: skillData.OnHitChainDamageMultiplier
                                    * SkillExecutionRules.ResolveHitDamageMultiplier(skillData, chainTarget.Model),
                                finalDamageModifier: SkillExecutionRules.ResolveHitFinalDamageModifier(
                                    skillData,
                                    chainTarget.Model,
                                    manager.Units),
                                isTrigger: skillData.IsTrigger);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /// 후속 사건이 원래 시전자를 추적할 식별자를 고른다.
        private static string SourceSkillName(SkillExecutionState executionData, SkillExecutionState sourceRuntime)
        {
            if (sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillName))
            {
                return sourceRuntime.SkillName;
            }

            if (executionData != null)
            {
                return executionData.SkillName;
            }

            return string.Empty;
        }
    }
}
