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
        // 생성된 지속 범위의 대상 판정, 주기 피해, 상태, 만료 효과를 구현.
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
        private SkillUseState runtime;
        private SkillExecutionData snapshot;
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
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Vector2 areaCenter /* 범위 중심 위치 */,
            float areaRadius /* 범위 반지름 */,
            bool areaCoversAll /* 범위 포함 전체 여부 */,
            float durationSeconds /* 지속 시간(초) */,
            float tickIntervalSeconds /* 반복 적용 간격(초) */,
            int maxTargetsPerTick /* 최대 대상 목록 개별 반복 적용 */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onTickStatus /* 발생 시 반복 적용 상태 효과 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SkillExecutionData executionData /* 실행 시점의 스킬 강화 정보 */,
            SkillEffectDefinition[] expireEffects /* 만료 효과 목록 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            bool allowCritical /* 허용 치명타 여부 */,
            float criticalChanceBonus /* 치명타 확률 추가값 */,
            float criticalDamageBonus /* 치명타 피해 추가값 */,
            int generation = 0 /* 실행 세대 */)
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
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Vector2 areaCenter /* 범위 중심 위치 */,
            float areaRadius /* 범위 반지름 */,
            bool areaCoversAll /* 범위 포함 전체 여부 */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            bool criticalAllowed /* 치명타 허용 여부 */,
            float critChanceBonus /* 추가 치명타 확률 */,
            float critDamageBonus /* 추가 치명타 피해 배율 */,
            int maxTargetsPerTick = int.MaxValue /* 최대 대상 목록 개별 반복 적용 */,
            SkillExecutionData executionData = null /* 실행 시점의 스킬 강화 정보 */)
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
                var resolvedDamage = damagePerTick;
                if (executionData != null)
                {
                    resolvedDamage *= SkillExecutionRuleResolver.ResolveConditionalDamageMultiplier(executionData, target.Model);
                }
                resolvedDamage = Mathf.Max(0f, resolvedDamage);
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
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

        /*
         * 인게임 지속 범위 스킬의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
         */
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
            ZoneSkillExecutor.ExecuteAdditionalEffects(
                context,
                snapshot,
                onExpireEffects,
                center,
                true,
                SkillMultiEffectTiming.OnExpire,
                false);
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
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Collider2D[] hitboxColliders /* 피격 판정 콜라이더 목록 */,
            int maxTargetsPerTick /* 최대 대상 목록 개별 반복 적용 */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            bool criticalAllowed /* 치명타 허용 여부 */,
            float critChanceBonus /* 추가 치명타 확률 */,
            float critDamageBonus /* 추가 치명타 피해 배율 */,
            SkillExecutionData executionData /* 실행 시점의 스킬 강화 정보 */)
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
                executionData);
            return routed;
        }

        /*
         * 이번 주기에 결정된 적중 결과를 적용한다.
         */
        private static bool ApplyResolvedHits(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            List<CombatUnitEntry> eligibleTargets /* 적용 가능한 대상 목록 */,
            int maxTargetsPerTick /* 최대 대상 목록 개별 반복 적용 */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            bool criticalAllowed /* 치명타 허용 여부 */,
            float critChanceBonus /* 추가 치명타 확률 */,
            float critDamageBonus /* 추가 치명타 피해 배율 */,
            SkillExecutionData executionData /* 실행 시점의 스킬 강화 정보 */)
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
                var resolvedDamage = damagePerTick;
                if (executionData != null)
                {
                    resolvedDamage *= SkillExecutionRuleResolver.ResolveConditionalDamageMultiplier(executionData, target.Model);
                }
                resolvedDamage = Mathf.Max(0f, resolvedDamage);
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(manager, target.Model, onHitStatus, source);
                }
                ZoneSkillExecutor.ApplyHitEnhancements(
                    manager,
                    sourceRuntime != null ? manager.UnitRegistry : null,
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

        /*
         * 대상 대상 주기를 선택한다.
         */
        private static List<CombatUnitEntry> SelectTargetsForTick(List<CombatUnitEntry> eligibleTargets /* 적용 가능한 대상 목록 */, int maxTargetsPerTick /* 최대 대상 목록 개별 반복 적용 */)
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
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyStatus(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */)
        {
            StatusCombatRules.ApplyStatus(manager, target, status, source);
        }

        /*
         * 출처 스킬 ID를 결정한다.
         */
        private static string ResolveSourceSkillId(SkillExecutionData executionData /* 실행 시점의 스킬 강화 정보 */, SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */)
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
