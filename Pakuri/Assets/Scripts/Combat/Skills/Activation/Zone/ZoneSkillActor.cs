/*
 * 역할: 지속 Zone 런타임 동작.
 * 책임: Zone 배치·recast·주기·Collider 판정·피해·상태·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ZoneSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
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

        /// 영역의 범위, 지속과 적중 규칙을 시작한다.
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

        /// 프레임 경과에 따라 영역 틱과 수명을 갱신한다.
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

        /// 영역 종료 사건을 후속 효과에 전달한다.
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

        /// 현재 틱의 범위 적용을 실행한다.
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

        /// 충돌된 범위 대상에 피해를 적용한다.
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

        /// 범위 대상 목록을 물리 적중과 후속 사건으로 연결한다.
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

        /// 선택된 대상에 피해와 상태, 적중 사건을 적용한다.
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

        /// 적중 사건과 적중 후속 값을 같은 Actor 경로에서 처리한다.
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

        /// 실행에 사용할 원본 스킬 식별자를 고른다.
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
