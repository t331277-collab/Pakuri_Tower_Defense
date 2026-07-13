using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using System.Text;
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
        private int maxHitTargetCount;
        private float damage;
        private DamageAttribute attribute;
        private ProjectileStatusHitSpec statusSpec;
        private SkillRuntimeInstance runtime;
        private SkillExecutionSnapshot snapshot;
        private SkillEffectDefinition[] onExpireEffects;
        private BaseUnitRuntimeModel sourceModel;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private Collider2D[] prefabHitboxColliders;
        private bool usePrefabHitbox;
        private int recastGeneration;

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
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onTickStatus,
            SkillRuntimeInstance sourceRuntime,
            SkillExecutionSnapshot executionSnapshot,
            SkillEffectDefinition[] expireEffects,
            BaseUnitRuntimeModel source,
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
            LogPrefabHitboxInitialization();

            ConfigureVisual();
            ApplyCurrentAreaTick();
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
            ProjectileStatusHitSpec onHitStatus,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargetsPerTick = int.MaxValue,
            SkillExecutionSnapshot executionSnapshot = null)
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

                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = ResolveDamageAgainstTarget(damagePerTick, executionSnapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, onHitStatus, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
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
            var eligibleTargets = new List<UnitRosterEntry>();
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
                0f,
                recastGeneration: recastGeneration);
            SkillMultiEffectExecutor.ExecuteOnExpire(context, snapshot, onExpireEffects, center);
            onExpireEffects = null;
        }

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
                    snapshot,
                    GetDebugSkillId());
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

        internal static bool ApplyColliderAreaTick(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            Collider2D[] hitboxColliders,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot executionSnapshot,
            string debugSkillId)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<UnitRosterEntry>();
            var debug = IsDebugSkill(debugSkillId);
            if (debug)
            {
                Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Tick start. candidates={candidates.Count}, hitboxes={DescribeColliderCollection(hitboxColliders)}");
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                var overlapped = IsTargetInsideHitbox(hitboxColliders, target, debug, debugSkillId);
                if (!overlapped)
                {
                    if (debug && target != null)
                    {
                        Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Miss target={DescribeTarget(target)}");
                    }
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
            if (debug)
            {
                if (selectedTargets.Count > 0 && selectedTargets.Count < eligibleTargets.Count)
                {
                    Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] RandomPick selected={selectedTargets.Count}/{eligibleTargets.Count}");
                }

                for (var i = 0; i < selectedTargets.Count; i++)
                {
                    var target = selectedTargets[i];
                    if (target == null || target.Model == null)
                    {
                        continue;
                    }

                    var resolvedDamage = ResolveDamageAgainstTarget(damagePerTick, executionSnapshot, target.Model);
                    Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Hit target={DescribeTarget(target)} damage={resolvedDamage}");
                }

                Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Tick end. routed={routed}");
            }

            return routed;
        }

        private static bool ApplyResolvedHits(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            List<UnitRosterEntry> eligibleTargets,
            int maxTargetsPerTick,
            float damagePerTick,
            DamageAttribute damageAttribute,
            ProjectileStatusHitSpec onHitStatus,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot executionSnapshot)
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
                manager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, onHitStatus, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    sourceRuntime != null ? manager.Roster : null,
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

        private static List<UnitRosterEntry> SelectTargetsForTick(List<UnitRosterEntry> eligibleTargets, int maxTargetsPerTick)
        {
            if (eligibleTargets == null || eligibleTargets.Count == 0)
            {
                return new List<UnitRosterEntry>();
            }

            if (maxTargetsPerTick <= 0 || maxTargetsPerTick >= eligibleTargets.Count)
            {
                return new List<UnitRosterEntry>(eligibleTargets);
            }

            var selectedTargets = new List<UnitRosterEntry>(eligibleTargets);
            for (var i = 0; i < maxTargetsPerTick; i++)
            {
                var randomIndex = UnityEngine.Random.Range(i, selectedTargets.Count);
                (selectedTargets[i], selectedTargets[randomIndex]) = (selectedTargets[randomIndex], selectedTargets[i]);
            }

            selectedTargets.RemoveRange(maxTargetsPerTick, selectedTargets.Count - maxTargetsPerTick);
            return selectedTargets;
        }

        private static float ResolveDamageAgainstTarget(
            float baseDamage,
            SkillExecutionSnapshot executionSnapshot,
            BaseUnitRuntimeModel target)
        {
            return SkillExecutionUtility.ResolveDamageAgainstTarget(baseDamage, executionSnapshot, target);
        }

        private static bool IsTargetInsideHitbox(
            Collider2D[] hitboxColliders,
            UnitRosterEntry target,
            bool debug,
            string debugSkillId)
        {
            if (hitboxColliders == null || target == null || target.Model == null || !target.IsAlive)
            {
                return false;
            }

            if (!debug)
            {
                return UnitHitboxUtility.IsTargetInsideHitbox(hitboxColliders, target);
            }

            var targetColliders = target.GetHitboxColliders();
            if (debug)
            {
                Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Checking target={DescribeTarget(target)} targetColliders={DescribeColliderCollection(targetColliders)}");
            }

            if (targetColliders == null || targetColliders.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < hitboxColliders.Length; i++)
            {
                var hitbox = hitboxColliders[i];
                if (hitbox == null || !hitbox.enabled)
                {
                    continue;
                }

                for (var j = 0; j < targetColliders.Length; j++)
                {
                    var targetCollider = targetColliders[j];
                    if (targetCollider == null || !targetCollider.enabled)
                    {
                        continue;
                    }

                    var distance = hitbox.Distance(targetCollider);
                    if (debug)
                    {
                        Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Compare hitbox={DescribeCollider(hitbox)} targetCollider={DescribeCollider(targetCollider)} overlapped={distance.isOverlapped} distance={distance.distance}");
                    }

                    if (distance.isOverlapped)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetDebugSkillId()
        {
            return runtime != null ? runtime.SkillId : string.Empty;
        }

        private void LogPrefabHitboxInitialization()
        {
            var debugSkillId = GetDebugSkillId();
            if (!IsDebugSkill(debugSkillId))
            {
                return;
            }

            Debug.Log($"[ZoneHitboxDebug:{debugSkillId}] Initialize usePrefabHitbox={usePrefabHitbox} center={center} radius={radius} scale={transform.localScale} hitboxes={DescribeColliderCollection(prefabHitboxColliders)}");
        }

        private static bool IsDebugSkill(string skillId)
        {
            return !string.IsNullOrWhiteSpace(skillId)
                && string.Equals(skillId, "eve-c", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string DescribeTarget(UnitRosterEntry target)
        {
            if (target == null)
            {
                return "<null-target>";
            }

            var name = target.Transform != null ? target.Transform.name : "<null-transform>";
            var unitId = target.Model != null && target.Model.Identity != null ? target.Model.Identity.UnitId : "<null-unit-id>";
            var pos = target.ResolveTargetPoint().ToString();
            return $"{name}/{unitId}@{pos}";
        }

        private static string DescribeColliderCollection(Collider2D[] colliders)
        {
            if (colliders == null || colliders.Length == 0)
            {
                return "[]";
            }

            var sb = new StringBuilder();
            sb.Append('[');
            for (var i = 0; i < colliders.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(DescribeCollider(colliders[i]));
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string DescribeCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return "<null-collider>";
            }

            var bounds = collider.bounds;
            return $"{collider.GetType().Name}:{collider.name}:enabled={collider.enabled}:trigger={collider.isTrigger}:center={bounds.center}:size={bounds.size}";
        }

        private static void TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status,
            BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(manager, target, status, source);
        }

        private static string ResolveSourceSkillId(SkillExecutionSnapshot executionSnapshot, SkillRuntimeInstance sourceRuntime)
        {
            if (sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillId))
            {
                return sourceRuntime.SkillId;
            }

            return executionSnapshot != null ? executionSnapshot.SkillId : string.Empty;
        }
    }
}
