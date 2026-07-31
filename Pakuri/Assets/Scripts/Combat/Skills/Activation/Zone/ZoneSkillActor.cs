/*
 * 역할: 일정 공간에 남는 공격의 실제 판정을 진행한다.
 * 책임: 범위 충돌과 주기 피해, 상태, 재시전 세대, 표현 수명과 종료 사건을 처리한다.
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
        private float radius;
        private bool coverAll;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private int maxHitTargetCount;
        private float damage;
        private DamageAttribute attribute;
        private StatusApplicationSpec statusSpec;
        private SkillExecutionData runtime;
        private SkillExecutionData snapshot;
        private UnitCombatState sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private bool usePrefabHitbox;
        private int recastGeneration;
        private static bool applyingHitEnhancement;

        /// 확정된 영역과 적중 기준으로 첫 주기를 시작한다.
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float durationSeconds,
            float tickIntervalSeconds,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onTickStatus,
            SkillExecutionData sourceRuntime,
            SkillExecutionData executionData,
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
            radius = Mathf.Max(0f, areaRadius);
            coverAll = areaCoversAll;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.05f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            maxHitTargetCount = maxTargetsPerTick <= 0 ? int.MaxValue : maxTargetsPerTick;
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
            usePrefabHitbox = !coverAll
                && prefabHitboxColliders != null
                && prefabHitboxColliders.Length > 0;
            EffectVisualBuilder.ConfigureZoneEffect(
                gameObject,
                center,
                radius,
                coverAll,
                usePrefabHitbox);
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
                var lifecycleContext = new SkillActionContext(
                    combatManager,
                    roster,
                    casterEntry,
                    runtime,
                    recastGeneration: recastGeneration);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillActionContext(
                        casterEntry.Model,
                        SourceSkillId(snapshot, runtime),
                        null,
                        center,
                        0f,
                        0,
                        snapshot,
                        lifecycleContext));
            }
        }

        /// 물리 충돌 영역 유무에 맞춰 이번 주기의 대상 판정을 고른다.
        private bool ApplyCurrentAreaTick()
        {
            if (usePrefabHitbox)
            {
                return ApplyColliderAreaTick(
                    combatManager,
                    casterEntry,
                    roster,
                    targeting,
                    prefabHitboxColliders,
                    maxHitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    sourceModel,
                    SourceSkillId(snapshot, runtime),
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    snapshot);
            }

            return ApplyAreaTargets(
                combatManager,
                casterEntry,
                roster,
                targeting,
                center,
                radius,
                coverAll,
                damage,
                attribute,
                statusSpec,
                sourceModel,
                SourceSkillId(snapshot, runtime),
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                maxHitTargetCount,
                snapshot);
        }

        /// 실제 충돌 영역과 겹친 대상만 이번 주기 결과로 확정한다.
        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Collider2D[] hitboxColliders,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
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

            var routed = ApplyResolvedTargets(
                manager,
                sourceEntry,
                unitRoster,
                eligibleTargets,
                maxTargetsPerTick,
                damagePerTick,
                damageAttribute,
                onHitStatus,
                source,
                sourceSkillId,
                sourceRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
            return routed;
        }

        /// 중심과 반경으로 걸러진 대상을 공통 적중 처리로 넘긴다.
        internal static bool ApplyAreaTargets(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 center,
            float radius,
            bool coverAll,
            float damage,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargets,
            SkillExecutionData executionData)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            if (!coverAll && radius <= 0f)
            {
                var target = SkillTargeting.FindNearestTarget(
                    sourceEntry,
                    unitRoster,
                    targetingSpec);
                return ApplyResolvedTargets(
                    manager,
                    sourceEntry,
                    unitRoster,
                    target != null ? new[] { target } : Array.Empty<CombatUnitEntry>(),
                    1,
                    damage,
                    damageAttribute,
                    onHitStatus,
                    source,
                    sourceSkillId,
                    sourceRuntime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    executionData);
            }

            var candidates = SkillTargeting.TargetList(
                sourceEntry,
                unitRoster,
                targetingSpec);
            var radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target == null
                    || !target.IsAlive
                    || target.Model == null
                    || target.Transform == null)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
                if (!string.IsNullOrWhiteSpace(unitId)
                    && !hitUnitIds.Add(unitId))
                {
                    continue;
                }
                if (!coverAll
                    && ((Vector2)target.Transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                eligibleTargets.Add(target);
            }

            return ApplyResolvedTargets(
                manager,
                sourceEntry,
                unitRoster,
                eligibleTargets,
                maxTargets,
                damage,
                damageAttribute,
                onHitStatus,
                source,
                sourceSkillId,
                sourceRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
        }

        /// 대상 제한을 반영한 뒤 피해, 상태, 후속 사건을 같은 순서로 적용한다.
        internal static bool ApplyResolvedTargets(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            IReadOnlyList<CombatUnitEntry> eligibleTargets,
            int maxTargets,
            float damage,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
        {
            if (manager == null
                || eligibleTargets == null
                || eligibleTargets.Count == 0)
            {
                return false;
            }

            var selectedTargets = new List<CombatUnitEntry>(eligibleTargets);
            if (maxTargets > 0 && maxTargets < selectedTargets.Count)
            {
                for (var i = 0; i < maxTargets; i++)
                {
                    var randomIndex = UnityEngine.Random.Range(i, selectedTargets.Count);
                    (selectedTargets[i], selectedTargets[randomIndex]) =
                        (selectedTargets[randomIndex], selectedTargets[i]);
                }
                selectedTargets.RemoveRange(
                    maxTargets,
                    selectedTargets.Count - maxTargets);
            }

            var routed = false;
            for (var i = 0; i < selectedTargets.Count; i++)
            {
                var target = selectedTargets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                var hitPosition = target.Transform != null
                    ? (Vector2)target.Transform.position
                    : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier =
                    SkillExecutionRuleResolver.ResolveHitDamageMultiplier(
                        executionData,
                        target.Model);
                var result = manager.ApplyDamage(
                    target.Model,
                    resolvedDamage,
                    damageAttribute,
                    source,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    sourceSkillId,
                    finalDamageMultiplier: finalDamageMultiplier);
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
                    sourceSkillId,
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
            SkillExecutionData runtime,
            SkillExecutionData skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillId,
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
                var actionExecutionContext = new SkillActionContext(
                    manager,
                    unitRoster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model,
                    publishSkillLifecycleEvents: runtime != null,
                    sourceSkillId: sourceSkillId);
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
                !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
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
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillId(
                        skillData.ReloadReduceTargetSkillId);
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
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier:
                            skillData.OnHitAdditionalDamageMultiplier);
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
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true,
                                finalDamageMultiplier:
                                    skillData.OnHitChainDamageMultiplier);
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
        private static string SourceSkillId(SkillExecutionData executionData, SkillExecutionData sourceRuntime)
        {
            if (sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillId))
            {
                return sourceRuntime.SkillId;
            }

            if (executionData != null)
            {
                return executionData.SkillId;
            }

            return string.Empty;
        }
    }
}
