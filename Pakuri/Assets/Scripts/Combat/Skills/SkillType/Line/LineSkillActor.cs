using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 직선 공격의 위치, 충돌, 수명 주기를 처리한다.
 */
namespace Pakuri.InGame
{
    public sealed class LineSkillActor : MonoBehaviour
    {
        private InGameCombatManager combatManager;
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
        private SkillRuntimeInstance runtime;
        private SkillSnapshot executionSnapshot;
        private UnitCombatState sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private readonly HashSet<string> appliedBaseStatusTargets = new HashSet<string>();
        private readonly HashSet<string> appliedEffectStatusTargets = new HashSet<string>();
        private readonly List<Collider2D> lineOverlapResults = new List<Collider2D>(32);

        /*
         * 인게임 직선 공격 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 lineOrigin,
            Vector2 lineDirection,
            float lineLength,
            float lineWidth,
            float lineKnockbackDistance,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            SkillEffectDefinition[] onHitEffects,
            SkillRuntimeInstance sourceRuntime,
            SkillSnapshot snapshot,
            UnitCombatState source,
            string skillId,
            bool allowCritical,
            float criticalChanceBonus,
            float criticalDamageBonus)
        {
            combatManager = manager;
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
         * 직선 주기를 적용한다.
         */
        public static bool ApplyLineTick(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 lineOrigin,
            Vector2 lineDirection,
            float lineLength,
            float lineWidth,
            float lineKnockbackDistance,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            SkillEffectDefinition[] onHitEffects,
            SkillRuntimeInstance sourceRuntime,
            SkillSnapshot snapshot,
            UnitCombatState source,
            string skillId,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            HashSet<string> baseStatusAppliedTargets = null,
            HashSet<string> effectStatusAppliedTargets = null,
            string damageMeterSourceId = null,
            List<Collider2D> overlapResults = null)
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
                var resolvedDamage = SkillValueCalculator.ResolveDamageAgainstTarget(damagePerTick, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, skillId, false, false, damageMeterSourceId);
                TryApplyKnockback(target, normalizedDirection, lineKnockbackDistance);
                var targetKey = ResolveTargetKey(target.Model);
                TryApplyStatus(manager, target.Model, onHitStatus, source, targetKey, baseStatusAppliedTargets);
                TryApplyOnHitEffects(manager, target.Model, onHitEffects, snapshot, source, targetKey, effectStatusAppliedTargets);
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
            if (combatManager == null)
            {
                Destroy(gameObject);
                return;
            }

            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
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

            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
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
            IReadOnlyList<Collider2D> overlappedColliders,
            CombatUnitEntry target,
            Vector2 lineOrigin,
            Vector2 normalizedDirection,
            float lineLength,
            float lineWidth)
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
            Vector2 lineOrigin,
            Vector2 normalizedDirection,
            float lineLength,
            float lineWidth,
            Vector3 targetPosition)
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
        private static void TryApplyKnockback(CombatUnitEntry target, Vector2 normalizedDirection, float distance)
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
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source,
            string targetKey,
            HashSet<string> appliedTargets)
        {
            if (status == null || !status.Enabled)
            {
                return;
            }

            if (appliedTargets != null && !string.IsNullOrWhiteSpace(targetKey) && appliedTargets.Contains(targetKey))
            {
                return;
            }

            if (SkillStatus.TryApplyStatus(manager, target, status, source)
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
            InGameCombatManager manager,
            UnitCombatState target,
            SkillEffectDefinition[] effects,
            SkillSnapshot snapshot,
            UnitCombatState source,
            string targetKey,
            HashSet<string> appliedEffects)
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

                if (SkillStatus.TryApplyStatus(manager, target, status, source)
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
        private static string ResolveTargetKey(UnitCombatState target)
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
        private static string BuildEffectTargetKey(string effectId, string targetKey)
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
