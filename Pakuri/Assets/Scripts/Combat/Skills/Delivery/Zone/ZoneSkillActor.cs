/*
 * 역할: 지속 Zone 런타임 동작.
 * 책임: Zone의 수명과 주기를 추적하고 Collider 점유 대상을 판정해 유효 적중을 전달한다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>ZoneSkillActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
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
        private ProjectileStatusHitSpec statusSpec;
        private SkillUseState runtime;
        private SkillExecutionData snapshot;
        private UnitCombatState sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private bool usePrefabHitbox;
        private int recastGeneration;

        /// <summary>전달된 런타임 입력값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
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
            ProjectileStatusHitSpec onTickStatus,
            SkillUseState sourceRuntime,
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
            ConfigureVisual();
            ApplyCurrentAreaTick();
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>AreaTick</c>를 적용한다.</summary>
        public static bool ApplyAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillUseState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargetsPerTick = int.MaxValue,
            SkillExecutionData executionData = null)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(sourceEntry, unitRoster, targetingSpec);
            if (!areaCoversAll && areaRadius <= 0f)
            {
                var target = SkillTargeting.FindNearestTarget(sourceEntry, unitRoster, targetingSpec);
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damagePerTick);
                var finalDamageMultiplier = executionData != null
                    ? Mathf.Max(0f, executionData.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(executionData, target.Model)
                    : 1f;
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: finalDamageMultiplier);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(manager, target.Model, onHitStatus, source);
                }
                ZoneSkillExecutor.ApplyHitEnhancements(
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
                return true;
            }

            var radiusSq = Mathf.Max(0f, areaRadius) * Mathf.Max(0f, areaRadius);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null;
                if (!string.IsNullOrWhiteSpace(unitId) && !hitUnitIds.Add(unitId))
                {
                    continue;
                }

                if (!areaCoversAll)
                {
                    var offset = (Vector2)target.Transform.position - areaCenter;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                eligibleTargets.Add(target);
            }

            return ApplyResolvedHits(
                manager,
                sourceEntry,
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
        }

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
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

        /// <summary><c>ExecuteExpireEffects</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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
                    new SkillActionContext(
                        casterEntry.Model,
                        snapshot != null ? snapshot.SkillId : string.Empty,
                        null,
                        center,
                        0f,
                        0,
                        snapshot,
                        lifecycleContext));
            }
        }

        /// <summary><c>ConfigureVisual</c> 작업을 수행한다.</summary>
        private void ConfigureVisual()
        {
            transform.position = center;
            if (usePrefabHitbox)
            {
                return;
            }

            if (coverAll || radius <= 0f)
            {
                return;
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            var size = spriteRenderer.sprite.bounds.size;
            var scale = transform.localScale;
            var diameter = radius * 2f;
            if (size.x > 0.0001f)
            {
                scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (diameter / size.x);
            }

            if (size.y > 0.0001f)
            {
                scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (diameter / size.y);
            }

            transform.localScale = scale;
        }

        /// <summary><c>CurrentAreaTick</c>를 적용한다.</summary>
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

            return ApplyAreaTick(
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ColliderAreaTick</c>를 적용한다.</summary>
        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Collider2D[] hitboxColliders,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillUseState sourceRuntime,
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

            var selectedTargets = SelectTargetsForTick(eligibleTargets, maxTargetsPerTick);
            var routed = ApplyResolvedHits(
                manager,
                sourceEntry,
                selectedTargets,
                int.MaxValue,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolvedHits</c>를 적용한다.</summary>
        private static bool ApplyResolvedHits(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            List<CombatUnitEntry> eligibleTargets,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillUseState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
        {
            if (manager == null || eligibleTargets == null || eligibleTargets.Count == 0)
            {
                return false;
            }

            var selectedTargets = SelectTargetsForTick(eligibleTargets, maxTargetsPerTick);
            var routed = false;
            for (var i = 0; i < selectedTargets.Count; i++)
            {
                var target = selectedTargets[i];
                if (target == null || target.Model == null)
                {
                    continue;
                }

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damagePerTick);
                var finalDamageMultiplier = executionData != null
                    ? Mathf.Max(0f, executionData.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(executionData, target.Model)
                    : 1f;
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: finalDamageMultiplier);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(manager, target.Model, onHitStatus, source);
                }
                ZoneSkillExecutor.ApplyHitEnhancements(
                    manager,
                    sourceRuntime != null ? manager.Units : null,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>TargetsForTick</c>를 선택한다.</summary>
        private static List<CombatUnitEntry> SelectTargetsForTick(List<CombatUnitEntry> eligibleTargets, int maxTargetsPerTick)
        {
            if (eligibleTargets == null || eligibleTargets.Count == 0)
            {
                return new List<CombatUnitEntry>();
            }

            if (maxTargetsPerTick <= 0 || maxTargetsPerTick >= eligibleTargets.Count)
            {
                return new List<CombatUnitEntry>(eligibleTargets);
            }

            var selectedTargets = new List<CombatUnitEntry>(eligibleTargets);
            for (var i = 0; i < maxTargetsPerTick; i++)
            {
                var randomIndex = UnityEngine.Random.Range(i, selectedTargets.Count);
                (selectedTargets[i], selectedTargets[randomIndex]) = (selectedTargets[randomIndex], selectedTargets[i]);
            }

            selectedTargets.RemoveRange(maxTargetsPerTick, selectedTargets.Count - maxTargetsPerTick);
            return selectedTargets;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplyStatus</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static void TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source)
        {
            StatusCombatRules.ApplyStatus(manager, target, status, source);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>SourceSkillId</c> 결과값을 생성해 반환한다.</summary>
        private static string SourceSkillId(SkillExecutionData executionData, SkillUseState sourceRuntime)
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
