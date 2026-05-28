using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameLineAttackActor : MonoBehaviour
    {
        private InGameCombatManager combatManager;
        private UnitRosterEntry casterEntry;
        private UnitRosterService roster;
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
        private SkillExecutionSnapshot executionSnapshot;
        private BaseUnitRuntimeModel sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private readonly HashSet<string> appliedBaseStatusTargets = new HashSet<string>();
        private readonly HashSet<string> appliedEffectStatusTargets = new HashSet<string>();

        public void Initialize(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
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
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel source,
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
                appliedEffectStatusTargets);
        }

        public static bool ApplyLineTick(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
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
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel source,
            string skillId,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            HashSet<string> baseStatusAppliedTargets = null,
            HashSet<string> effectStatusAppliedTargets = null,
            string damageMeterSourceId = null)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || lineDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            var normalizedDirection = lineDirection.normalized;
            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
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

                if (!IsInsideLine(lineOrigin, normalizedDirection, lineLength, lineWidth, target.Transform.position))
                {
                    continue;
                }

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damagePerTick, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, skillId, false, false, damageMeterSourceId);
                TryApplyKnockback(target, normalizedDirection, lineKnockbackDistance);
                var targetKey = ResolveTargetKey(target.Model);
                TryApplyStatus(manager, target.Model, onHitStatus, source, targetKey, baseStatusAppliedTargets);
                TryApplyOnHitEffects(manager, target.Model, onHitEffects, snapshot, source, targetKey, effectStatusAppliedTargets);
                SkillOnHitAdditionalDamageUtility.TryApply(
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
                    appliedEffectStatusTargets);
            }

            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
            }
        }

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

        private static bool IsInsideLine(
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

        private static void TryApplyKnockback(UnitRosterEntry target, Vector2 normalizedDirection, float distance)
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

        private static void TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status,
            BaseUnitRuntimeModel source,
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

            if (SkillStatusApplyUtility.TryApplyStatus(manager, target, status, source)
                && appliedTargets != null
                && !string.IsNullOrWhiteSpace(targetKey))
            {
                appliedTargets.Add(targetKey);
            }
        }

        private static void TryApplyOnHitEffects(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            SkillEffectDefinition[] effects,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel source,
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
                    || !SkillMultiEffectExecutor.TargetMatchesCondition(target, effect))
                {
                    continue;
                }

                var effectKey = BuildEffectTargetKey(effect.EffectId, targetKey);
                if (appliedEffects != null && !string.IsNullOrWhiteSpace(effectKey) && appliedEffects.Contains(effectKey))
                {
                    continue;
                }

                var status = SkillMultiEffectExecutor.ResolveStatusSpec(effect, snapshot);
                if (status == null || !status.Enabled)
                {
                    continue;
                }

                if (SkillStatusApplyUtility.TryApplyStatus(manager, target, status, source)
                    && appliedEffects != null
                    && !string.IsNullOrWhiteSpace(effectKey))
                {
                    appliedEffects.Add(effectKey);
                }
            }
        }

        private static string ResolveTargetKey(BaseUnitRuntimeModel target)
        {
            var unitId = target != null && target.Identity != null ? target.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                return unitId;
            }

            return target != null ? target.GetHashCode().ToString() : string.Empty;
        }

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
