using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public sealed class SingleAttackSkillExecutor : TypedSkillExecutor<SingleAttackData>
    {
        private readonly struct SingleAttackExecutionOutcome
        {
            public SingleAttackExecutionOutcome(bool routed, bool castCommitted)
            {
                Routed = routed;
                CastCommitted = castCommitted;
            }

            public bool Routed { get; }
            public bool CastCommitted { get; }
        }

        private readonly struct SingleAttackFollowUpSpec
        {
            public SingleAttackFollowUpSpec(string requiredStatusId, int repeatCount, float intervalSeconds, float damageMultiplier, GameObject prefab)
            {
                RequiredStatusId = requiredStatusId;
                RepeatCount = repeatCount;
                IntervalSeconds = intervalSeconds;
                DamageMultiplier = damageMultiplier;
                Prefab = prefab;
            }

            public string RequiredStatusId { get; }
            public int RepeatCount { get; }
            public float IntervalSeconds { get; }
            public float DamageMultiplier { get; }
            public GameObject Prefab { get; }
        }

        private readonly struct SingleAttackFollowUpTarget
        {
            public SingleAttackFollowUpTarget(BaseUnitRuntimeModel model, Vector2 center)
            {
                Model = model;
                Center = center;
            }

            public BaseUnitRuntimeModel Model { get; }
            public Vector2 Center { get; }
        }

        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as SingleAttackData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var center = ResolveAreaCenter(context, skill.Targeting, skill.Area);
            var prefab = ResolvePrefab(context, snapshot, skill);
            var outcome = ExecuteAtCenter(context, snapshot, skill, center, prefab, true);
            var multiEffectRouted = SkillMultiEffectExecutor.Execute(context, snapshot, skill.MultiEffects, center);
            var routed = outcome.Routed || multiEffectRouted;
            return new SkillExecutionResult(
                routed || outcome.CastCommitted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected,
                skill.SkillId,
                GetType().Name);
        }

        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            return SkillAreaUtility.ResolveAreaCenter(context, targeting, area);
        }

        private static float ResolveRadius(SingleAttackData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillAreaUtility.ResolveRadius(SkillAreaUtility.ResolveBaseRadius(targeting, area), snapshot);
        }

        private static GameObject ResolvePrefab(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackData skill)
        {
            return snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;
        }

        private static void SpawnVisual(SkillExecutionContext context, GameObject prefab, Vector2 center)
        {
            if (prefab == null || context.CombatManager.Effects == null)
            {
                return;
            }

            SkillVisualSpawnUtility.SpawnTransient(context.CombatManager.Effects, prefab, center, Quaternion.identity, 1f);
        }

        private static Vector2 ResolvePrefabHitboxCenter(SkillExecutionContext context, Vector2 fallbackCenter, SingleAttackData skill)
        {
            if (skill != null && skill.HitAllTargets)
            {
                return SkillExecutionUtility.ResolveTargetGroupCenter(context, skill.Targeting, fallbackCenter);
            }

            return fallbackCenter;
        }

        private static SingleAttackExecutionOutcome ExecuteAtCenter(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackData skill,
            Vector2 center,
            GameObject prefab,
            bool allowConditionalFollowUp)
        {
            var radius = ResolveRadius(skill, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var critChanceBonus = snapshot != null ? snapshot.CritChanceBonus : 0f;
            var critDamageBonus = snapshot != null ? snapshot.CritDamageBonus : 0f;
            var hitTargetCountBonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            var effectiveHitTargetCount = skill.HitAllTargets
                ? int.MaxValue
                : Mathf.Max(1, skill.HitTargetCount + hitTargetCountBonus);
            var followUpSpec = allowConditionalFollowUp ? ResolveFollowUpSpec(snapshot, statusSpec, prefab) : null;
            var followUpTargets = followUpSpec.HasValue ? new List<SingleAttackFollowUpTarget>() : null;
            var onHitRuntime = allowConditionalFollowUp ? context.Runtime : null;
            var spawnedHitbox = false;
            var routed = false;
            var castCommitted = false;

            if (skill.UsePrefabHitbox && prefab != null && context.CombatManager.Effects != null)
            {
                center = ResolvePrefabHitboxCenter(context, center, skill);
                var instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                if (instance != null)
                {
                    spawnedHitbox = true;
                    castCommitted = true;
                    SkillExecutionUtility.ApplyPrefabScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
                    Physics2D.SyncTransforms();
                    routed = ApplyPrefabHitbox(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        instance,
                        effectiveHitTargetCount,
                        damage,
                        attribute,
                        statusSpec,
                        context.Caster,
                        skill.SkillId,
                        onHitRuntime,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        snapshot,
                        followUpSpec,
                        followUpTargets);
                    UnityEngine.Object.Destroy(instance, 1f);
                }
            }

            if (!spawnedHitbox)
            {
                castCommitted = true;
                if (skill.UsesHitTargetCount && !skill.HitAllTargets)
                {
                    routed = ApplyLimitedTargets(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        effectiveHitTargetCount,
                        damage,
                        attribute,
                        statusSpec,
                        context.Caster,
                        skill.SkillId,
                        onHitRuntime,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        snapshot,
                        center,
                        followUpSpec,
                        followUpTargets);
                }
                else
                {
                    routed = ApplyAreaTargets(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        center,
                        radius,
                        coverAll,
                        damage,
                        attribute,
                        statusSpec,
                        context.Caster,
                        skill.SkillId,
                        onHitRuntime,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        snapshot,
                        followUpSpec,
                        followUpTargets);
                }

                if (routed)
                {
                    SpawnVisual(context, prefab, center);
                }
            }

            if (allowConditionalFollowUp)
            {
                ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
            }

            return new SingleAttackExecutionOutcome(routed, castCommitted);
        }

        private static bool ApplyPrefabHitbox(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            GameObject hitboxObject,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxObject == null || maxTargets <= 0)
            {
                return false;
            }

            var hitboxColliders = hitboxObject.GetComponentsInChildren<Collider2D>();
            if (hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsTargetInsideHitbox(hitboxColliders, target))
                {
                    continue;
                }

                RegisterFollowUpTarget(
                    followUpTargets,
                    followUpSpec,
                    target,
                    target != null && target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static bool ApplyLimitedTargets(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            Vector2 center,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || maxTargets <= 0)
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static bool ApplyAreaTargets(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 center,
            float radius,
            bool coverAll,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            if (!coverAll && radius <= 0f)
            {
                var target = targets.Count > 0 ? targets[0] : null;
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                return true;
            }

            var routed = false;
            var radiusSq = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                if (!coverAll)
                {
                    var offset = (Vector2)target.Transform.position - center;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
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

        private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, UnitRosterEntry target)
        {
            return UnitHitboxUtility.IsTargetInsideHitbox(hitboxColliders, target);
        }

        private static SingleAttackFollowUpSpec? ResolveFollowUpSpec(
            SkillExecutionSnapshot snapshot,
            ProjectileStatusHitSpec statusSpec,
            GameObject prefab)
        {
            if (snapshot == null
                || !snapshot.HasBranchCount
                || snapshot.BranchCount <= 0
                || !snapshot.HasBranchDamageMultiplier
                || snapshot.BranchDamageMultiplier <= 0f
                || !snapshot.HasBranchSearchRadius
                || snapshot.BranchSearchRadius <= 0f)
            {
                return null;
            }

            var requiredStatusId = !string.IsNullOrWhiteSpace(snapshot.StatusTag)
                ? snapshot.StatusTag
                : statusSpec != null && statusSpec.StatusData != null
                    ? statusSpec.StatusData.StatusTag
                    : statusSpec != null
                        ? StatusEffectUtility.ToId(statusSpec.Kind)
                        : string.Empty;
            if (string.IsNullOrWhiteSpace(requiredStatusId))
            {
                return null;
            }

            return new SingleAttackFollowUpSpec(
                requiredStatusId,
                snapshot.BranchCount,
                snapshot.BranchSearchRadius,
                snapshot.BranchDamageMultiplier,
                prefab);
        }

        private static void RegisterFollowUpTarget(
            List<SingleAttackFollowUpTarget> followUpTargets,
            SingleAttackFollowUpSpec? followUpSpec,
            UnitRosterEntry target,
            Vector2 center)
        {
            if (followUpTargets == null
                || !followUpSpec.HasValue
                || target == null
                || target.Model == null
                || !HasStatus(target.Model, followUpSpec.Value.RequiredStatusId))
            {
                return;
            }

            for (var i = 0; i < followUpTargets.Count; i++)
            {
                if (ReferenceEquals(followUpTargets[i].Model, target.Model))
                {
                    return;
                }
            }

            followUpTargets.Add(new SingleAttackFollowUpTarget(target.Model, center));
        }

        private static void ScheduleConditionalFollowUps(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackData skill,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (context == null
                || context.CombatManager == null
                || context.Roster == null
                || context.CasterEntry == null
                || context.Caster == null
                || skill == null
                || !followUpSpec.HasValue
                || followUpTargets == null
                || followUpTargets.Count == 0)
            {
                return;
            }

            var spec = followUpSpec.Value;
            for (var i = 0; i < followUpTargets.Count; i++)
            {
                var followUpTarget = followUpTargets[i];
                for (var repeatIndex = 1; repeatIndex <= spec.RepeatCount; repeatIndex++)
                {
                    context.CombatManager.StartCoroutine(ExecuteConditionalFollowUpAfterDelay(
                        context,
                        snapshot,
                        skill,
                        followUpTarget,
                        spec,
                        spec.IntervalSeconds * repeatIndex));
                }
            }
        }

        private static IEnumerator ExecuteConditionalFollowUpAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackData skill,
            SingleAttackFollowUpTarget followUpTarget,
            SingleAttackFollowUpSpec followUpSpec,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            if (context == null
                || context.CombatManager == null
                || context.Roster == null
                || context.CasterEntry == null
                || context.Caster == null
                || skill == null)
            {
                yield break;
            }

            var liveTarget = followUpTarget.Model != null
                ? context.Roster.Find(followUpTarget.Model)
                : null;
            var center = liveTarget != null && liveTarget.Transform != null
                ? (Vector2)liveTarget.Transform.position
                : followUpTarget.Center;
            var followUpSnapshot = snapshot != null ? CloneSnapshotWithDamageMultiplier(snapshot, followUpSpec.DamageMultiplier) : null;
            ExecuteAtCenter(context, followUpSnapshot, skill, center, followUpSpec.Prefab, false);
        }

        private static SkillExecutionSnapshot CloneSnapshotWithDamageMultiplier(
            SkillExecutionSnapshot snapshot,
            float damageMultiplier)
        {
            if (snapshot == null)
            {
                return null;
            }

            var clone = new SkillExecutionSnapshot(snapshot.Source);
            clone.ApplyChoiceSpec(new SkillChoiceEffectSpec
            {
                HasDamageMultiplier = true,
                DamageMultiplier = snapshot.DamageMultiplier * Mathf.Max(0f, damageMultiplier),
                BaseDamageBonus = snapshot.BaseDamageBonus,
                HasCooldownMultiplier = true,
                CooldownMultiplier = snapshot.CooldownMultiplier,
                HasRadiusMultiplier = true,
                RadiusMultiplier = snapshot.RadiusMultiplier,
                RadiusBonus = snapshot.RadiusBonus,
                HasDurationMultiplier = true,
                DurationMultiplier = snapshot.DurationMultiplier,
                DurationBonus = snapshot.DurationBonus,
                HasReloadTimeMultiplier = true,
                ReloadTimeMultiplier = snapshot.ReloadTimeMultiplier,
                HasShotIntervalMultiplier = true,
                ShotIntervalMultiplier = snapshot.ShotIntervalMultiplier,
                BranchChanceBonus = snapshot.BranchChanceBonus,
                HasBranchChanceSet = snapshot.HasBranchChanceSet,
                BranchChanceSet = snapshot.BranchChanceSet,
                HasBranchCount = snapshot.HasBranchCount,
                BranchCount = snapshot.BranchCount,
                HasBranchDamageMultiplier = snapshot.HasBranchDamageMultiplier,
                BranchDamageMultiplier = snapshot.BranchDamageMultiplier,
                HasBranchSearchRadius = snapshot.HasBranchSearchRadius,
                BranchSearchRadius = snapshot.BranchSearchRadius,
                HitTargetCountBonus = snapshot.HitTargetCountBonus,
                CritChanceBonus = snapshot.CritChanceBonus,
                CritDamageBonus = snapshot.CritDamageBonus,
                StatusTag = snapshot.StatusTag,
                HasStatusChanceBonus = !Mathf.Approximately(snapshot.StatusChanceBonus, 0f),
                StatusChanceBonus = snapshot.StatusChanceBonus,
                StatusStacksBonus = snapshot.StatusStacksBonus,
                HasStatusStacksSet = snapshot.HasStatusStacksSet,
                StatusStacksSet = snapshot.StatusStacksSet,
                HasStatusElementDamageTakenBonus = snapshot.HasStatusElementDamageTakenBonus,
                StatusElementDamageTakenBonus = snapshot.StatusElementDamageTakenBonus,
                HasStatusCriticalDamageTakenBonus = snapshot.HasStatusCriticalDamageTakenBonus,
                StatusCriticalDamageTakenBonus = snapshot.StatusCriticalDamageTakenBonus,
                HasStatusAilmentResistanceBonus = snapshot.HasStatusAilmentResistanceBonus,
                StatusAilmentResistanceBonus = snapshot.StatusAilmentResistanceBonus,
                ThresholdStatusId = snapshot.ThresholdStatusId,
                ThresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks,
                ThresholdApplyStatusId = snapshot.ThresholdApplyStatusId,
                SkillEffectPrefab = snapshot.SkillEffectPrefab,
                HasStatusConditionalDamageTakenBonus = snapshot.HasStatusConditionalDamageTakenBonus,
                StatusConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus,
                StatusConditionalSourceStatusId = snapshot.StatusConditionalSourceStatusId
            });
            return clone;
        }

        private static bool HasStatus(BaseUnitRuntimeModel target, string statusId)
        {
            return target != null
                && target.Statuses != null
                && !string.IsNullOrWhiteSpace(statusId)
                && target.Statuses.Has(statusId);
        }

        private static void TryApplyStatus(InGameCombatManager manager, BaseUnitRuntimeModel target, ProjectileStatusHitSpec statusSpec, BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(manager, target, statusSpec, source);
        }
    }
}


