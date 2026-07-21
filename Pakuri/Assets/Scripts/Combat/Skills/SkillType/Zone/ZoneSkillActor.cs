using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 지속 범위 스킬의 위치, 충돌, 수명 주기를 처리한다.
 */
namespace Pakuri.InGame
{
    public class ZoneSkillActor : MonoBehaviour
    {
        private InGameCombatManager combatManager;
        private CombatUnitEntry casterEntry;
        private CombatUnitRegistry roster;
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
        private SkillRuntimeInstance runtime;
        private SkillSnapshot snapshot;
        private SkillEffectDefinition[] onExpireEffects;
        private UnitCombatState sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private bool usePrefabHitbox;
        private int recastGeneration;

        /*
         * 인게임 지속 범위 스킬 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
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
            SkillRuntimeInstance sourceRuntime,
            SkillSnapshot executionSnapshot,
            SkillEffectDefinition[] expireEffects,
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
            snapshot = executionSnapshot;
            onExpireEffects = expireEffects;
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

        /*
         * 범위 주기를 적용한다.
         */
        public static bool ApplyAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargetsPerTick = int.MaxValue,
            SkillSnapshot executionSnapshot = null)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var candidates = SkillTargeting.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            if (!areaCoversAll && areaRadius <= 0f)
            {
                var target = SkillTargeting.FindNearestTarget(sourceEntry, unitRoster, targetingSpec);
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = ResolveDamageAgainstTarget(damagePerTick, executionSnapshot, target.Model);
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(manager, target.Model, onHitStatus, source);
                }
                SkillOnHitEffect.TryApply(
                    manager,
                    sourceRuntime != null ? unitRoster : null,
                    sourceRuntime,
                    executionSnapshot,
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
                executionSnapshot);
        }

        /*
         * 인게임 지속 범위 스킬의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
         */
        private void Update()
        {
            if (combatManager == null)
            {
                TryExecuteExpireEffects();
                Destroy(gameObject);
                return;
            }

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
                Destroy(gameObject);
            }
        }

        /*
         * 종료 효과를 실행하고 성공 여부를 반환한다.
         */
        private void TryExecuteExpireEffects()
        {
            if (onExpireEffects == null || onExpireEffects.Length == 0 || combatManager == null || casterEntry == null || roster == null)
            {
                return;
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                casterEntry,
                runtime,
                recastGeneration: recastGeneration);
            SkillEffect.ExecuteOnExpire(context, snapshot, onExpireEffects, center);
            onExpireEffects = null;
        }

        /*
         * 지속 범위 비주얼과 히트박스 크기를 설정한다.
         */
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

        /*
         * 현재 범위 주기를 적용한다.
         */
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
                    ResolveSourceSkillId(snapshot, runtime),
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
                ResolveSourceSkillId(snapshot, runtime),
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                maxHitTargetCount,
                snapshot);
        }

        /*
         * 콜라이더 범위 주기를 적용한다.
         */
        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec,
            Collider2D[] hitboxColliders,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillSnapshot executionSnapshot)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var candidates = SkillTargeting.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                var overlapped = UnitHitboxOverlap.IsTargetInsideHitbox(hitboxColliders, target);
                if (!overlapped)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null;
                if (!string.IsNullOrWhiteSpace(unitId) && !hitUnitIds.Add(unitId))
                {
                    continue;
                }

                eligibleTargets.Add(target);
            }

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
                executionSnapshot);
            return routed;
        }

        /*
         * 이번 주기에 결정된 적중 결과를 적용한다.
         */
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
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillSnapshot executionSnapshot)
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
                var resolvedDamage = ResolveDamageAgainstTarget(damagePerTick, executionSnapshot, target.Model);
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(manager, target.Model, onHitStatus, source);
                }
                SkillOnHitEffect.TryApply(
                    manager,
                    sourceRuntime != null ? manager.UnitRegistry : null,
                    sourceRuntime,
                    executionSnapshot,
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

        /*
         * 대상 대상 주기를 선택한다.
         */
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

        /*
         * 대상의 방어와 상태를 반영한 최종 피해를 계산한다.
         */
        private static float ResolveDamageAgainstTarget(
            float baseDamage,
            SkillSnapshot executionSnapshot,
            UnitCombatState target)
        {
            return DamageCalculator.ResolveDamageAgainstTarget(baseDamage, executionSnapshot, target);
        }

        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source)
        {
            StatusCombatRules.ApplyStatus(manager, target, status, source);
        }

        /*
         * 출처 스킬 ID를 결정한다.
         */
        private static string ResolveSourceSkillId(SkillSnapshot executionSnapshot, SkillRuntimeInstance sourceRuntime)
        {
            if (sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillId))
            {
                return sourceRuntime.SkillId;
            }

            return executionSnapshot != null ? executionSnapshot.SkillId : string.Empty;
        }
    }
}
