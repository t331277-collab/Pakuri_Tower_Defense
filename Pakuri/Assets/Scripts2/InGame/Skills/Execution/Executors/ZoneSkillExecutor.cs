using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public sealed class ZoneSkillExecutor : TypedSkillExecutor<ZoneSkillData>
    {
        internal static bool ExecuteRecast(
            SkillExecutionContext context,
            SkillExecutionSnapshot inheritedSnapshot,
            SkillEffectDefinition effect,
            Vector2 center)
        {
            var skill = context != null ? context.SkillData as ZoneSkillData : null;
            if (skill == null
                || effect == null
                || context.CombatManager == null
                || context.CasterEntry == null
                || context.Roster == null
                || (!string.IsNullOrWhiteSpace(effect.RecastSourceSkillId)
                    && !string.Equals(effect.RecastSourceSkillId, skill.SkillId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var maxGeneration = Math.Max(1, effect.RecastMaxGeneration);
            if (context.RecastGeneration >= maxGeneration)
            {
                return false;
            }

            var snapshot = effect.RecastInheritSnapshot
                ? inheritedSnapshot
                : new SkillExecutionSnapshot(skill);
            var radius = ResolveRadius(skill, snapshot) * Mathf.Max(0f, effect.RecastRadiusMultiplier);
            var duration = Mathf.Max(0.05f, effect.RecastDurationSeconds);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var hitTargetCount = ResolveHitTargetCount(skill, snapshot);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            var expireEffects = ResolveOnExpireEffects(context, snapshot, planEffects);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = RuntimeSkillVisualFactory.HasVisual(runtimeVisual);
            var prefab = !hasRuntimeVisual && snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : !hasRuntimeVisual && context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            GameObject instance = null;
            if (hasRuntimeVisual && context.CombatManager.Effects != null)
            {
                instance = RuntimeSkillVisualFactory.Create(
                    context.CombatManager.Effects,
                    runtimeVisual,
                    string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameRecastZone" : $"InGameRecastZone_{skill.SkillId}",
                    center,
                    Quaternion.identity);
                if (instance != null && PrefabHasHitbox(instance))
                {
                    SkillExecutionUtility.ApplyPrefabScale(
                        instance.transform,
                        SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area),
                        snapshot);
                    instance.transform.localScale *= Mathf.Max(0f, effect.RecastRadiusMultiplier);
                    Physics2D.SyncTransforms();
                }
            }
            else if (prefab != null && context.CombatManager.Effects != null)
            {
                instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                if (instance != null && PrefabHasHitbox(instance))
                {
                    SkillExecutionUtility.ApplyPrefabScale(
                        instance.transform,
                        SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area),
                        snapshot);
                    instance.transform.localScale *= Mathf.Max(0f, effect.RecastRadiusMultiplier);
                    Physics2D.SyncTransforms();
                }
            }

            if (instance == null)
            {
                instance = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameRecastZone" : $"InGameRecastZone_{skill.SkillId}");
                instance.transform.position = center;
            }

            var actor = instance.GetComponent<InGameZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameZoneSkillActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                center,
                radius,
                coverAll,
                duration,
                tickInterval,
                hitTargetCount,
                damage,
                attribute,
                statusSpec,
                context.Runtime,
                snapshot,
                expireEffects,
                context.Caster,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                context.RecastGeneration + 1);
            return true;
        }

        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ZoneSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var deploymentCount = ResolveDeploymentCount(snapshot);
            var centers = ResolveAreaCenters(context, skill.Targeting, skill.Area, deploymentCount);
            var radius = ResolveRadius(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var hitTargetCount = ResolveHitTargetCount(skill, snapshot);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            var expireEffects = ResolveOnExpireEffects(context, snapshot, planEffects);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = RuntimeSkillVisualFactory.HasVisual(runtimeVisual);
            var prefab = !hasRuntimeVisual && snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : !hasRuntimeVisual && context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            var routed = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                GameObject instance = null;
                if (hasRuntimeVisual && context.CombatManager.Effects != null)
                {
                    instance = RuntimeSkillVisualFactory.Create(
                        context.CombatManager.Effects,
                        runtimeVisual,
                        string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameZoneSkill" : $"InGameZoneSkill_{skill.SkillId}",
                        center,
                        Quaternion.identity);
                    if (instance != null && PrefabHasHitbox(instance))
                    {
                        SkillExecutionUtility.ApplyPrefabScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
                        Physics2D.SyncTransforms();
                    }
                }
                else if (prefab != null && context.CombatManager.Effects != null)
                {
                    instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                    if (instance != null && PrefabHasHitbox(instance))
                    {
                        SkillExecutionUtility.ApplyPrefabScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
                        Physics2D.SyncTransforms();
                    }
                }

                if (instance == null)
                {
                    instance = new GameObject(string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameZoneSkill" : $"InGameZoneSkill_{skill.SkillId}");
                    instance.transform.position = center;
                }

                var actor = instance.GetComponent<InGameZoneSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<InGameZoneSkillActor>();
                }

                actor.Initialize(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    center,
                    radius,
                    coverAll,
                    duration,
                    tickInterval,
                    hitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    context.Runtime,
                    snapshot,
                    expireEffects,
                    context.Caster,
                    skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
                routed = true;
            routed = SkillMultiEffectExecutor.Execute(context, snapshot, planEffects, center) || routed;
            }

            return new SkillExecutionResult(
                routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected,
                skill.SkillId,
                GetType().Name);
        }

        private static int ResolveDeploymentCount(SkillExecutionSnapshot snapshot)
        {
            return 1 + (snapshot != null && snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
        }

        private static int ResolveHitTargetCount(ZoneSkillData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null || skill.HitAllTargets || !skill.UsesHitTargetCount)
            {
                return int.MaxValue;
            }

            var baseCount = Math.Max(1, skill.HitTargetCount);
            var bonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Math.Max(1, baseCount + bonus);
        }

        private static List<Vector2> ResolveAreaCenters(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area,
            int deploymentCount)
        {
            var primaryCenter = ResolveAreaCenter(context, targeting, area);
            var coverAll = (area != null && area.CoverAll)
                || (targeting != null && targeting.CoverAll);
            return SkillDeploymentCenterUtility.ResolveTargetAnchoredCenters(
                context,
                targeting,
                primaryCenter,
                deploymentCount,
                coverAll,
                SkillDeploymentRepeatMode.RandomExisting);
        }

        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            return SkillAreaUtility.ResolveAreaCenter(context, targeting, area);
        }

        private static float ResolveRadius(ZoneSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillAreaUtility.ResolveRadius(SkillAreaUtility.ResolveBaseRadius(targeting, area), snapshot);
        }

        private static float ResolveDuration(ZoneSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var duration = area != null && area.Duration > 0f
                ? area.Duration
                : timing != null ? timing.ActiveDuration : 0f;
            if (duration <= 0f)
            {
                duration = ResolveTickInterval(skill, snapshot);
            }

            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        private static float ResolveTickInterval(ZoneSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var interval = area != null && area.TickInterval > 0f
                ? area.TickInterval
                : timing != null && timing.TickInterval > 0f ? timing.TickInterval : 1f;
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        private static SkillEffectDefinition[] ResolveOnExpireEffects(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects)
        {
            if (effects == null || effects.Length == 0)
            {
                return Array.Empty<SkillEffectDefinition>();
            }

            var resolved = new List<SkillEffectDefinition>();
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || effect.EffectTiming != SkillMultiEffectTiming.OnExpire
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }

        private static bool PrefabHasHitbox(GameObject hitboxObject)
        {
            if (hitboxObject == null)
            {
                return false;
            }

            var hitboxColliders = hitboxObject.GetComponentsInChildren<Collider2D>();
            return hitboxColliders != null && hitboxColliders.Length > 0;
        }

    }
}


