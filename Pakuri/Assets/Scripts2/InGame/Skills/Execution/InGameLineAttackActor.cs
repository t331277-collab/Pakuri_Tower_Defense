using System.Collections.Generic;
using Pakuri.Combat;
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
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;
        private BaseUnitRuntimeModel sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;

        public void Initialize(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 lineOrigin,
            Vector2 lineDirection,
            float lineLength,
            float lineWidth,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            BaseUnitRuntimeModel source,
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
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.01f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onHitStatus;
            sourceModel = source;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;

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
                damage,
                attribute,
                statusSpec,
                sourceModel,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus);
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
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            BaseUnitRuntimeModel source,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus)
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

                manager.ApplyDamage(target.Model, damagePerTick, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus);
                TryApplyStatus(manager, target.Model, onHitStatus);
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
                    damage,
                    attribute,
                    statusSpec,
                    sourceModel,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus);
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

        private static void TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status)
        {
            SkillStatusApplyUtility.TryApplyStatus(manager, target, status);
        }
    }
}
