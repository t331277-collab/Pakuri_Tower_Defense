using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 직선 공격의 위치, 충돌, 수명 주기를 처리한다.
 */
namespace Pakuri.InGame
{
    public class LineSkillActor : MonoBehaviour
    {
        private InGameCombatManager combatManager;
        private EffectManager effectManager;
        private CombatUnitEntry casterEntry;
        private CombatUnitRegistry roster;
        private SkillTargetingSpec targeting;
        private Vector2 origin;
        private Vector2 direction = Vector2.right;
        private float length;
        private float width;
        private float knockbackDistance;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;
        private SkillEffectDefinition[] onHitStatusEffects;
        private SkillUseState runtime;
        private SkillSnapshot executionSnapshot;
        private UnitCombatState sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private bool visualOnly;
        private readonly HashSet<string> appliedBaseStatusTargets = new HashSet<string>();
        private readonly HashSet<string> appliedEffectStatusTargets = new HashSet<string>();
        private readonly List<Collider2D> lineOverlapResults = new List<Collider2D>(32);

        /*
         * 인게임 직선 공격 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
        public void Initialize(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Vector2 lineOrigin /* 직선 시작 위치 */,
            Vector2 lineDirection /* 직선 방향 */,
            float lineLength /* 직선 길이 */,
            float lineWidth /* 직선 너비 */,
            float lineKnockbackDistance /* 직선 밀쳐내기 거리 */,
            float durationSeconds /* 지속 시간(초) */,
            float tickIntervalSeconds /* 반복 적용 간격(초) */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            SkillEffectDefinition[] onHitEffects /* 발생 시 적중 효과 목록 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string skillId /* 스킬 식별자 */,
            bool allowCritical /* 허용 치명타 여부 */,
            float criticalChanceBonus /* 치명타 확률 추가값 */,
            float criticalDamageBonus /* 치명타 피해 추가값 */)
        {
            combatManager = manager;
            effectManager = manager.Effects;
            visualOnly = false;
            casterEntry = sourceEntry;
            roster = unitRoster;
            targeting = targetingSpec;
            origin = lineOrigin;
            direction = lineDirection.sqrMagnitude > 0.0001f ? lineDirection.normalized : Vector2.right;
            length = Mathf.Max(0.1f, lineLength);
            width = Mathf.Max(0.1f, lineWidth);
            knockbackDistance = Mathf.Max(0f, lineKnockbackDistance);
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.01f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onHitStatus;
            onHitStatusEffects = onHitEffects;
            runtime = sourceRuntime;
            executionSnapshot = snapshot;
            sourceModel = source;
            sourceSkillId = skillId;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            appliedBaseStatusTargets.Clear();
            appliedEffectStatusTargets.Clear();

            ConfigureVisual();
            ApplyLineTick(
                combatManager,
                casterEntry,
                roster,
                targeting,
                origin,
                direction,
                length,
                width,
                knockbackDistance,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                runtime,
                executionSnapshot,
                sourceModel,
                sourceSkillId,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                appliedBaseStatusTargets,
                appliedEffectStatusTargets,
                null,
            lineOverlapResults);
        }

        /*
         * 피해 처리가 없는 직선 비주얼의 애니메이션 수명을 설정하고 그 시간을 반환한다.
         */
        public float InitializeVisualLifetime(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            float minimumLifetimeSeconds /* 최소 유지 시간(초) */)
        {
            effectManager = manager;
            visualOnly = true;
            remainingDuration = EffectVisualBuilder.ResolveLifetime(gameObject, minimumLifetimeSeconds);
            return remainingDuration;
        }

        /*
         * 직선 주기를 적용한다.
         */
        public static bool ApplyLineTick(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Vector2 lineOrigin /* 직선 시작 위치 */,
            Vector2 lineDirection /* 직선 방향 */,
            float lineLength /* 직선 길이 */,
            float lineWidth /* 직선 너비 */,
            float lineKnockbackDistance /* 직선 밀쳐내기 거리 */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            SkillEffectDefinition[] onHitEffects /* 발생 시 적중 효과 목록 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string skillId /* 스킬 식별자 */,
            bool criticalAllowed /* 치명타 허용 여부 */,
            float critChanceBonus /* 추가 치명타 확률 */,
            float critDamageBonus /* 추가 치명타 피해 배율 */,
            HashSet<string> baseStatusAppliedTargets = null /* 기본 상태 효과 적용된 대상 목록 */,
            HashSet<string> effectStatusAppliedTargets = null /* 효과 상태 효과 적용된 대상 목록 */,
            string damageMeterSourceId = null /* 피해량 기록에 사용할 발생 원본 식별자 */,
            List<Collider2D> overlapResults = null /* 겹침 처리 결과 목록 */)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || lineDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            var normalizedDirection = lineDirection.normalized;
            var resolvedLength = Mathf.Max(0.1f, lineLength);
            var resolvedWidth = Mathf.Max(0.1f, lineWidth);
            var hitboxCenter = lineOrigin + normalizedDirection * (resolvedLength * 0.5f);
            var hitboxAngle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            var overlappedColliders = overlapResults ?? new List<Collider2D>(32);
            overlappedColliders.Clear();
            Physics2D.SyncTransforms();
            Physics2D.OverlapBox(
                hitboxCenter,
                new Vector2(resolvedLength, resolvedWidth),
                hitboxAngle,
                ContactFilter2D.noFilter,
                overlappedColliders);

            var candidates = SkillTargeting.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            var hitUnitIds = new HashSet<string>();
            var routed = false;
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

                if (!IsInsideLineHitbox(
                        overlappedColliders,
                        target,
                        lineOrigin,
                        normalizedDirection,
                        resolvedLength,
                        resolvedWidth))
                {
                    continue;
                }

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = damagePerTick;
                if (snapshot != null)
                {
                    resolvedDamage *= snapshot.ResolveConditionalDamageMultiplier(target.Model);
                }
                resolvedDamage = Mathf.Max(0f, resolvedDamage);
                var damageResult = manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, skillId, false, false, damageMeterSourceId);
                TryApplyKnockback(target, normalizedDirection, lineKnockbackDistance);
                if (!damageResult.IsDead)
                {
                    var targetKey = ResolveTargetKey(target.Model);
                    TryApplyStatus(manager, target.Model, onHitStatus, source, targetKey, baseStatusAppliedTargets);
                    TryApplyOnHitEffects(manager, target.Model, onHitEffects, snapshot, source, targetKey, effectStatusAppliedTargets);
                }
                SkillOnHitEffect.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    skillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        /*
         * 인게임 직선 공격의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
         */
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
            if (!visualOnly)
            {
                tickRemaining -= deltaTime;
                if (remainingDuration > 0f && tickRemaining <= 0f)
                {
                    tickRemaining += tickInterval;
                    ApplyLineTick(
                        combatManager,
                        casterEntry,
                        roster,
                        targeting,
                        origin,
                        direction,
                        length,
                        width,
                        knockbackDistance,
                        damage,
                        attribute,
                        statusSpec,
                        onHitStatusEffects,
                        runtime,
                        executionSnapshot,
                        sourceModel,
                        sourceSkillId,
                        criticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        appliedBaseStatusTargets,
                        appliedEffectStatusTargets,
                        null,
                        lineOverlapResults);
                }
            }

            if (remainingDuration <= 0f)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }

        /*
         * 직선 공격 비주얼과 히트박스 크기를 설정한다.
         */
        private void ConfigureVisual()
        {
            transform.position = origin + direction * (length * 0.5f);
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var size = spriteRenderer.sprite.bounds.size;
                var scale = transform.localScale;
                if (size.x > 0.0001f)
                {
                    scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (length / size.x);
                }

                if (size.y > 0.0001f)
                {
                    scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (width / size.y);
                }

                transform.localScale = scale;
            }
        }

        /*
         * 대상이 직선 공격 히트박스 안에 있는지 확인한다.
         */
        private static bool IsInsideLineHitbox(
            IReadOnlyList<Collider2D> overlappedColliders /* 겹친 콜라이더 목록 */,
            CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */,
            Vector2 lineOrigin /* 직선 시작 위치 */,
            Vector2 normalizedDirection /* 정규화된 방향 */,
            float lineLength /* 직선 길이 */,
            float lineWidth /* 직선 너비 */)
        {
            if (target == null || target.Model == null || !target.IsAlive)
            {
                return false;
            }

            var targetColliders = target.GetHitboxColliders();
            var hasEnabledTargetCollider = false;
            for (var i = 0; targetColliders != null && i < targetColliders.Length; i++)
            {
                var targetCollider = targetColliders[i];
                if (targetCollider == null || !targetCollider.enabled)
                {
                    continue;
                }

                hasEnabledTargetCollider = true;
                for (var j = 0; overlappedColliders != null && j < overlappedColliders.Count; j++)
                {
                    if (overlappedColliders[j] == targetCollider)
                    {
                        return true;
                    }
                }
            }

            return !hasEnabledTargetCollider
                && target.Transform != null
                && IsPointInsideLine(lineOrigin, normalizedDirection, lineLength, lineWidth, target.Transform.position);
        }

        /*
         * 지점이 직선 공격 범위 안에 있는지 확인한다.
         */
        private static bool IsPointInsideLine(
            Vector2 lineOrigin /* 직선 시작 위치 */,
            Vector2 normalizedDirection /* 정규화된 방향 */,
            float lineLength /* 직선 길이 */,
            float lineWidth /* 직선 너비 */,
            Vector3 targetPosition /* 대상의 위치 */)
        {
            var offset = (Vector2)targetPosition - lineOrigin;
            var projected = Vector2.Dot(offset, normalizedDirection);
            if (projected < 0f || projected > Mathf.Max(0.1f, lineLength))
            {
                return false;
            }

            var closest = lineOrigin + normalizedDirection * projected;
            var perpendicularDistance = Vector2.Distance((Vector2)targetPosition, closest);
            return perpendicularDistance <= Mathf.Max(0.05f, lineWidth * 0.5f);
        }

        /*
         * 밀쳐내기를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyKnockback(CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */, Vector2 normalizedDirection /* 정규화된 방향 */, float distance /* 거리 */)
        {
            if (target == null
                || target.Transform == null
                || distance <= 0f
                || normalizedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            target.Transform.position += (Vector3)(normalizedDirection.normalized * distance);
        }

        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyStatus(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string targetKey /* 대상 조회 키 */,
            HashSet<string> appliedTargets /* 적용된 대상 목록 */)
        {
            if (status == null || !status.Enabled)
            {
                return;
            }

            if (appliedTargets != null && !string.IsNullOrWhiteSpace(targetKey) && appliedTargets.Contains(targetKey))
            {
                return;
            }

            if (StatusCombatRules.ApplyStatus(manager, target, status, source)
                && appliedTargets != null
                && !string.IsNullOrWhiteSpace(targetKey))
            {
                appliedTargets.Add(targetKey);
            }
        }

        /*
         * 적중 효과를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyOnHitEffects(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            SkillEffectDefinition[] effects /* 실행할 효과 목록 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string targetKey /* 대상 조회 키 */,
            HashSet<string> appliedEffects /* 적용된 효과 목록 */)
        {
            if (manager == null || target == null || effects == null || effects.Length == 0)
            {
                return;
            }

            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || effect.EffectTiming != SkillMultiEffectTiming.OnHit
                    || effect.EffectKind != SkillMultiEffectKind.Status
                    || !SkillEffect.TargetMatchesCondition(target, effect))
                {
                    continue;
                }

                var effectKey = BuildEffectTargetKey(effect.EffectId, targetKey);
                if (appliedEffects != null && !string.IsNullOrWhiteSpace(effectKey) && appliedEffects.Contains(effectKey))
                {
                    continue;
                }

                var status = SkillEffect.ResolveStatusSpec(effect, snapshot);
                if (status == null || !status.Enabled)
                {
                    continue;
                }

                if (StatusCombatRules.ApplyStatus(manager, target, status, source)
                    && appliedEffects != null
                    && !string.IsNullOrWhiteSpace(effectKey))
                {
                    appliedEffects.Add(effectKey);
                }
            }
        }

        /*
         * 대상 키를 결정한다.
         */
        private static string ResolveTargetKey(UnitCombatState target /* 효과를 받을 대상 유닛 */)
        {
            var unitId = target != null && target.Identity != null ? target.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                return unitId;
            }

            return target != null ? target.GetHashCode().ToString() : string.Empty;
        }

        /*
         * 효과 대상 키를 구성한다.
         */
        private static string BuildEffectTargetKey(string effectId /* 효과 식별자 */, string targetKey /* 대상 조회 키 */)
        {
            if (string.IsNullOrWhiteSpace(targetKey))
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(effectId)
                ? targetKey
                : $"{effectId}::{targetKey}";
        }
    }
}
