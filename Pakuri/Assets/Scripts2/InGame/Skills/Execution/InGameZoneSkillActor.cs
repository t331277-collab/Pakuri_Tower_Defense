using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameZoneSkillActor : MonoBehaviour
    {
        private InGameCombatManager combatManager;
        private UnitRosterEntry casterEntry;
        private UnitRosterService roster;
        private SkillTargetingSpec targeting;
        private Vector2 center;
        private float radius;
        private bool coverAll;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;

        public void Initialize(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onTickStatus)
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
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onTickStatus;

            ConfigureVisual();
            ApplyAreaTick(
                combatManager,
                casterEntry,
                roster,
                targeting,
                center,
                radius,
                coverAll,
                damage,
                attribute,
                statusSpec);
        }

        public static bool ApplyAreaTick(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 areaCenter,
            float areaRadius,
            bool areaCoversAll,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            if (!areaCoversAll && areaRadius <= 0f)
            {
                var target = SkillExecutionUtility.FindNearestTarget(sourceEntry, unitRoster, targetingSpec);
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                manager.ApplyDamage(target.Model, damagePerTick, damageAttribute);
                TryApplyStatus(manager, target.Model, onHitStatus);
                return true;
            }

            var radiusSq = Mathf.Max(0f, areaRadius) * Mathf.Max(0f, areaRadius);
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

                if (!areaCoversAll)
                {
                    var offset = (Vector2)target.Transform.position - areaCenter;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                manager.ApplyDamage(target.Model, damagePerTick, damageAttribute);
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
            while (remainingDuration > 0f && tickRemaining <= 0f)
            {
                tickRemaining += tickInterval;
                ApplyAreaTick(
                    combatManager,
                    casterEntry,
                    roster,
                    targeting,
                    center,
                    radius,
                    coverAll,
                    damage,
                    attribute,
                    statusSpec);
            }

            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void ConfigureVisual()
        {
            transform.position = center;
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

        private static void TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status)
        {
            if (manager == null || target == null || status == null || !status.Enabled)
            {
                return;
            }

            if (Random.value > Mathf.Clamp01(status.Chance))
            {
                return;
            }

            manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                status.DurationSeconds,
                status.MaxStacks,
                status.Permanent,
                status.RefreshDuration);
        }
    }
}
