using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public abstract class TypedSkillExecutor<TSkillData> : IInGameSkillExecutor
        where TSkillData : SkillData
    {
        public bool CanExecute(SkillData skillData)
        {
            return skillData is TSkillData;
        }

        public virtual SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skillId = snapshot != null ? snapshot.SkillId : string.Empty;
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skillId, GetType().Name);
        }
    }

    public sealed class ProjectileSkillExecutor : TypedSkillExecutor<ProjectileSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ProjectileSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillExecutionUtility.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                if (!context.HasManualAimDirection)
                {
                    return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
                }

                direction = Vector2.right;
            }

            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var prefab = skill.Projectile != null ? skill.Projectile.ProjectilePrefab : null;
            if (prefab == null)
            {
                var effects = context.CombatManager.Effects;
                prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                    ? snapshot.SkillEffectPrefab
                    : effects != null ? effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId) : null;
            }

            var statusSpec = ResolveStatusSpec(skill.OnHitStatus, snapshot);
            if (prefab == null)
            {
                if (target != null)
                {
                    var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                    context.CombatManager.ApplyDamage(
                        target.Model,
                        resolvedDamage,
                        attribute,
                        context.Caster,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        snapshot != null ? snapshot.CritChanceBonus : 0f,
                        snapshot != null ? snapshot.CritDamageBonus : 0f,
                        skill.SkillId);
                    TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
                    return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
                }

                return new SkillExecutionResult(
                    context.HasManualAimDirection ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected,
                    skill.SkillId,
                    GetType().Name);
            }

            var projectile = skill.Projectile;
            var speed = projectile != null ? projectile.ProjectileSpeed : 0f;
            var pierce = projectile != null ? projectile.PierceCount : 0;
            var burstProjectileCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var projectileCount = projectile != null ? Math.Max(1, projectile.ProjectilesPerShot) : 1;
            if (snapshot != null)
            {
                pierce += snapshot.PierceBonus;
                if (burstProjectileCount <= 1)
                {
                    projectileCount += snapshot.AdditionalProjectileBonus;
                }
            }

            projectileCount = Math.Max(1, projectileCount);
            pierce = Math.Max(0, pierce);
            var branchSpec = ResolveBranchSpec(snapshot, prefab);
            var isMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var lifetime = SkillExecutionUtility.ResolveProjectileLifetime(skill);
            var boundary = context.CombatManager.ResolveProjectileDestroyBoundaryX();
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ResolveProjectileSpreadDirection(direction, i, projectileCount);
                var effects = context.CombatManager.Effects;
                if (effects == null)
                {
                    if (target != null)
                    {
                        var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                        context.CombatManager.ApplyDamage(
                            target.Model,
                            resolvedDamage,
                            attribute,
                            context.Caster,
                            skill.Damage != null && skill.Damage.CriticalAllowed,
                            snapshot != null ? snapshot.CritChanceBonus : 0f,
                            snapshot != null ? snapshot.CritDamageBonus : 0f,
                            skill.SkillId);
                        TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
                    }

                    continue;
                }

                var instance = effects.InstantiateSkillPrefab(
                    prefab,
                    origin,
                    ResolveRotation(spreadDirection));
                if (instance == null)
                {
                    if (target != null)
                    {
                        var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                        context.CombatManager.ApplyDamage(
                            target.Model,
                            resolvedDamage,
                            attribute,
                            context.Caster,
                            skill.Damage != null && skill.Damage.CriticalAllowed,
                            snapshot != null ? snapshot.CritChanceBonus : 0f,
                            snapshot != null ? snapshot.CritDamageBonus : 0f,
                            skill.SkillId);
                        TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
                    }

                    continue;
                }

                var actor = instance.GetComponent<InGameProjectileActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<InGameProjectileActor>();
                }

                actor.Initialize(
                    context.CombatManager,
                    context.Caster,
                    spreadDirection,
                    speed,
                    damage,
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    statusSpec,
                    branchSpec,
                    null,
                    skill.SkillId,
                    isMagazineLastProjectile,
                    skill.Damage != null && skill.Damage.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
            }

            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        private static Vector2 ResolveProjectileSpreadDirection(Vector2 direction, int index, int count)
        {
            if (count <= 1)
            {
                return direction;
            }

            const float angleStep = 10f;
            var offset = (index - (count - 1) * 0.5f) * angleStep;
            return RotateDirection(direction, offset);
        }

        private static Vector2 RotateDirection(Vector2 direction, float degrees)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.right;
            }

            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        private static Quaternion ResolveRotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        internal static ProjectileStatusHitSpec ResolveStatusSpec(
            StatusApplicationSpec baseStatus,
            SkillExecutionSnapshot snapshot)
        {
            var statusData = baseStatus != null ? baseStatus.Status : null;
            var snapshotTag = snapshot != null ? snapshot.StatusTag : null;
            var tag = !string.IsNullOrWhiteSpace(snapshotTag)
                ? snapshotTag
                : statusData != null ? statusData.StatusTag : null;
            var kind = statusData != null ? statusData.Kind : StatusEffectKind.None;
            if (!StatusEffectUtility.TryParse(tag, out var parsedKind) && kind == StatusEffectKind.None)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(snapshotTag) || kind == StatusEffectKind.None)
            {
                kind = parsedKind;
            }

            var stacks = baseStatus != null ? Math.Max(0, baseStatus.Stacks) : 1;
            var chance = baseStatus != null ? Mathf.Clamp01(baseStatus.Chance) : 1f;
            if (snapshot != null)
            {
                chance = Mathf.Clamp01(chance + snapshot.StatusChanceBonus);
                stacks = snapshot.HasStatusStacksSet
                    ? Math.Max(0, snapshot.StatusStacksSet)
                    : Math.Max(0, stacks + snapshot.StatusStacksBonus);
            }

            if (stacks <= 0 || chance <= 0f)
            {
                return null;
            }

            var definition = StatusEffectUtility.GetDefinition(kind);
            var resolvedStatusData = ResolveStatusData(statusData, kind, snapshot);
            var duration = resolvedStatusData != null && resolvedStatusData.Duration > 0f
                ? resolvedStatusData.Duration
                : definition.DefaultDurationSeconds;
            var maxStacks = resolvedStatusData != null && resolvedStatusData.MaxStacks > 0
                ? resolvedStatusData.MaxStacks
                : definition.DefaultMaxStacks;
            var targetedMaxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, resolvedStatusData, kind);
            if (targetedMaxStacksBonus != 0)
            {
                maxStacks = Mathf.Max(0, maxStacks + targetedMaxStacksBonus);
            }
            var permanent = definition.Permanent && (resolvedStatusData == null || resolvedStatusData.Duration <= 0f);
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
                if (duration > 0f)
                {
                    permanent = false;
                }
            }

            var targetedDurationBonus = ResolveStatusDurationBonus(snapshot, resolvedStatusData, kind);
            if (!Mathf.Approximately(targetedDurationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + targetedDurationBonus);
                if (duration > 0f)
                {
                    permanent = false;
                }
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = kind,
                StatusData = resolvedStatusData,
                Chance = chance,
                Stacks = stacks,
                DurationSeconds = duration,
                MaxStacks = maxStacks,
                Permanent = permanent,
                RefreshDuration = baseStatus == null || baseStatus.RefreshDuration,
                ThresholdSourceStatusId = snapshot != null ? snapshot.ThresholdStatusId : string.Empty,
                ThresholdSourceMinStacks = snapshot != null ? snapshot.ThresholdStatusMinStacks : 0,
                ThresholdStatusSpec = ResolveThresholdStatusSpec(snapshot)
            };
        }

        internal static StatusEffectData ResolveStatusData(
            StatusEffectData statusData,
            StatusEffectKind kind,
            SkillExecutionSnapshot snapshot)
        {
            var needsChoiceElementDamageOverride = snapshot != null && snapshot.HasStatusElementDamageTakenBonus;
            var needsChoiceCriticalDamageOverride = snapshot != null && snapshot.HasStatusCriticalDamageTakenBonus;
            var needsChoiceAilmentResistanceOverride = snapshot != null && snapshot.HasStatusAilmentResistanceBonus;
            var needsChoiceConditionalDamageTakenOverride = snapshot != null && snapshot.HasStatusConditionalDamageTakenBonus;
            if (statusData == null || statusData.Kind != kind)
            {
                statusData = StatusEffectRuntime.CreateStatusData(kind, null);
            }

            if (statusData == null
                || (!needsChoiceElementDamageOverride
                    && !needsChoiceCriticalDamageOverride
                    && !needsChoiceAilmentResistanceOverride
                    && !needsChoiceConditionalDamageTakenOverride))
            {
                return statusData;
            }

            var overriddenStatus = UnityEngine.Object.Instantiate(statusData);
            overriddenStatus.hideFlags = HideFlags.DontSave;
            if (needsChoiceElementDamageOverride)
            {
                overriddenStatus.ElementDamageTakenBonus = snapshot.StatusElementDamageTakenBonus;
            }

            if (needsChoiceCriticalDamageOverride)
            {
                overriddenStatus.CriticalDamageTakenBonus = snapshot.StatusCriticalDamageTakenBonus;
            }

            if (needsChoiceAilmentResistanceOverride)
            {
                overriddenStatus.AilmentResistanceBonus = snapshot.StatusAilmentResistanceBonus;
            }

            if (needsChoiceConditionalDamageTakenOverride)
            {
                overriddenStatus.ConditionalSourceStatusTag = snapshot.StatusConditionalSourceStatusId;
                overriddenStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
            }

            return overriddenStatus;
        }

        private static float ResolveStatusDurationBonus(
            SkillExecutionSnapshot snapshot,
            StatusEffectData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectUtility.GetDefinition(kind).Id;
            return snapshot.ResolveStatusDurationBonus(statusId);
        }

        private static int ResolveStatusMaxStacksBonus(
            SkillExecutionSnapshot snapshot,
            StatusEffectData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectUtility.GetDefinition(kind).Id;
            return snapshot.ResolveStatusMaxStacksBonus(statusId);
        }

        private static ProjectileStatusHitSpec ResolveThresholdStatusSpec(SkillExecutionSnapshot snapshot)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(snapshot.ThresholdApplyStatusId)
                || !StatusEffectUtility.TryParse(snapshot.ThresholdApplyStatusId, out var kind))
            {
                return null;
            }

            var statusData = StatusEffectRuntime.CreateStatusData(kind, null);
            if (statusData == null)
            {
                return null;
            }

            var definition = StatusEffectUtility.GetDefinition(kind);
            var duration = statusData.Duration > 0f ? statusData.Duration : definition.DefaultDurationSeconds;
            var targetedDurationBonus = ResolveStatusDurationBonus(snapshot, statusData, kind);
            if (!Mathf.Approximately(targetedDurationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + targetedDurationBonus);
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = kind,
                StatusData = statusData,
                Chance = 1f,
                Stacks = Mathf.Max(1, statusData.BaseStackAmount),
                DurationSeconds = duration,
                MaxStacks = statusData.MaxStacks,
                Permanent = statusData.Permanent && duration <= 0f,
                RefreshDuration = true
            };
        }

        private static ProjectileBranchHitSpec ResolveBranchSpec(SkillExecutionSnapshot snapshot, GameObject prefab)
        {
            if (snapshot == null || !snapshot.HasBranchBehavior || prefab == null)
            {
                return null;
            }

            var chance = snapshot.HasBranchChanceSet ? snapshot.BranchChanceSet : snapshot.BranchChanceBonus;
            var count = snapshot.HasBranchCount ? snapshot.BranchCount : chance > 0f ? 1 : 0;
            var radius = snapshot.HasBranchSearchRadius ? snapshot.BranchSearchRadius : 4.5f;
            if (chance <= 0f || count <= 0 || radius <= 0f)
            {
                return null;
            }

            return new ProjectileBranchHitSpec
            {
                Enabled = true,
                ProjectilePrefab = prefab,
                Chance = Mathf.Clamp01(chance),
                Count = Math.Max(1, count),
                DamageMultiplier = snapshot.HasBranchDamageMultiplier ? Mathf.Max(0f, snapshot.BranchDamageMultiplier) : 1f,
                SearchRadius = Mathf.Max(0f, radius)
            };
        }

        private static void TryApplyDirectStatus(
            InGameCombatManager combatManager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec statusSpec,
            BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(combatManager, target, statusSpec, source);
        }
    }

    public sealed class BeamSkillExecutor : TypedSkillExecutor<BeamSkillData>
    {
        private const float DefaultBeamLength = 31f;

        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as BeamSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillExecutionUtility.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            direction.Normalize();
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            var statusSpec = ProjectileSkillExecutor.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var length = ResolveBeamLength(skill, origin, direction, context.CombatManager);
            var width = ResolveBeamWidth(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var onHitStatusEffects = ResolveOnHitStatusEffects(context, snapshot, skill.MultiEffects);
            var castEffects = ResolveCastEffects(context, snapshot, skill.MultiEffects);
            var center = (Vector2)origin + direction * (length * 0.5f);
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            if (prefab == null || context.CombatManager.Effects == null)
            {
                InGameLineAttackActor.ApplyLineTick(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    origin,
                    direction,
                    length,
                    width,
                    damage,
                    attribute,
                    statusSpec,
                    onHitStatusEffects,
                    snapshot,
                    context.Caster,
                    skill.SkillId,
                    skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
                if (castEffects.Length > 0)
                {
                    SkillMultiEffectExecutor.Execute(context, snapshot, castEffects, center);
                }
                return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
            }

            var instance = context.CombatManager.Effects.InstantiateSkillPrefab(
                prefab,
                center,
                ResolveRotation(direction));
            if (instance == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var actor = instance.GetComponent<InGameLineAttackActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameLineAttackActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                origin,
                direction,
                length,
                width,
                duration,
                tickInterval,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                snapshot,
                context.Caster,
                skill.SkillId,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            if (castEffects.Length > 0)
            {
                SkillMultiEffectExecutor.Execute(context, snapshot, castEffects, center);
            }
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        private static float ResolveBeamLength(BeamSkillData skill, Vector3 origin, Vector2 direction, InGameCombatManager manager)
        {
            if (skill != null && skill.BeamLength > 0f)
            {
                return skill.BeamLength;
            }

            if (manager != null && Mathf.Abs(direction.x) > 0.0001f)
            {
                var boundary = manager.ResolveProjectileDestroyBoundaryX();
                var distance = Mathf.Abs((boundary - origin.x) / direction.x);
                if (distance > 0.1f)
                {
                    return Mathf.Max(1f, distance);
                }
            }

            return DefaultBeamLength;
        }

        private static float ResolveDuration(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var timing = skill != null ? skill.Timing : null;
            var duration = timing != null && timing.ActiveDuration > 0f
                ? timing.ActiveDuration
                : ResolveTickInterval(skill, snapshot);
            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        private static float ResolveBeamWidth(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var width = skill != null ? skill.BeamWidth : 0f;
            if (snapshot != null)
            {
                width *= ResolveBeamVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        private static float ResolveBeamVisualWidthScale(SkillExecutionSnapshot snapshot)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        private static float ResolveTickInterval(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var interval = ResolveTickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        private static float ResolveTickInterval(BeamSkillData skill)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

        private static Quaternion ResolveRotation(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        private static SkillEffectDefinition[] ResolveOnHitStatusEffects(
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
                    || effect.EffectTiming != SkillMultiEffectTiming.OnHit
                    || effect.EffectKind != SkillMultiEffectKind.Status
                    || effect.TargetSide != SkillMultiEffectTargetSide.Enemy
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }

        private static SkillEffectDefinition[] ResolveCastEffects(
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
                    || effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }
    }

    public sealed class ZoneSkillExecutor : TypedSkillExecutor<ZoneSkillData>
    {
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
            var statusSpec = ProjectileSkillExecutor.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var expireEffects = ResolveOnExpireEffects(context, snapshot, skill.MultiEffects);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            var routed = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                GameObject instance = null;
                if (prefab != null && context.CombatManager.Effects != null)
                {
                    instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                    if (instance != null && PrefabHasHitbox(instance))
                    {
                        ApplyPrefabHitboxScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
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
            var centers = new List<Vector2> { primaryCenter };
            var coverAll = (area != null && area.CoverAll)
                || (targeting != null && targeting.CoverAll);
            if (deploymentCount <= 1
                || context == null
                || coverAll
                || context.HasManualTargetPoint
                || context.HasManualAimDirection)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            var orderedTargets = ResolveOrderedTargets(context.CasterEntry, context.Roster, targeting);
            if (orderedTargets.Count <= 0)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            centers.Clear();
            for (var i = 0; i < orderedTargets.Count && centers.Count < deploymentCount; i++)
            {
                var target = orderedTargets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                centers.Add((Vector2)target.Transform.position);
            }

            while (centers.Count < deploymentCount)
            {
                var randomIndex = UnityEngine.Random.Range(0, orderedTargets.Count);
                var fallbackTarget = orderedTargets[randomIndex];
                if (fallbackTarget == null || fallbackTarget.Transform == null)
                {
                    centers.Add(primaryCenter);
                    continue;
                }

                centers.Add((Vector2)fallbackTarget.Transform.position);
            }

            return centers;
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

        private static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec)
        {
            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            var targets = new List<UnitRosterEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            var selection = targetingSpec != null ? targetingSpec.Selection : SkillTargetSelection.Nearest;
            targets.Sort((left, right) => CompareTargets(sourceEntry, selection, left, right));
            return targets;
        }

        private static int CompareTargets(UnitRosterEntry sourceEntry, SkillTargetSelection selection, UnitRosterEntry left, UnitRosterEntry right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var leftHealth = left.Model != null && left.Model.Resources != null ? left.Model.Resources.CurrentHealth : 0f;
            var rightHealth = right.Model != null && right.Model.Resources != null ? right.Model.Resources.CurrentHealth : 0f;
            if (selection == SkillTargetSelection.HighestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return rightHealth.CompareTo(leftHealth);
            }

            if (selection == SkillTargetSelection.LowestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return leftHealth.CompareTo(rightHealth);
            }

            var leftDistance = ResolveDistanceSquared(sourceEntry, left);
            var rightDistance = ResolveDistanceSquared(sourceEntry, right);
            return leftDistance.CompareTo(rightDistance);
        }

        private static float ResolveDistanceSquared(UnitRosterEntry sourceEntry, UnitRosterEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
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

        private static void ApplyPrefabHitboxScale(Transform target, float baseRadius, SkillExecutionSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            var scaleFactor = SkillAreaUtility.ResolvePrefabScaleFactor(baseRadius, snapshot);
            if (Mathf.Approximately(scaleFactor, 1f))
            {
                return;
            }

            target.localScale *= scaleFactor;
        }
    }

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
                var skillPoint = GameObject.Find("SkillPoint");
                if (skillPoint != null)
                {
                    return skillPoint.transform.position;
                }

                return Vector2.zero;
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
            var statusSpec = ProjectileSkillExecutor.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var critChanceBonus = snapshot != null ? snapshot.CritChanceBonus : 0f;
            var critDamageBonus = snapshot != null ? snapshot.CritDamageBonus : 0f;
            var hitTargetCountBonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            var effectiveHitTargetCount = skill.HitAllTargets
                ? int.MaxValue
                : Mathf.Max(1, skill.HitTargetCount + hitTargetCountBonus);
            var followUpSpec = allowConditionalFollowUp ? ResolveFollowUpSpec(snapshot, statusSpec, prefab) : null;
            var followUpTargets = followUpSpec.HasValue ? new List<SingleAttackFollowUpTarget>() : null;
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
                    ApplyHitboxScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
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

        private static void ApplyHitboxScale(Transform target, float baseRadius, SkillExecutionSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            var scaleFactor = SkillAreaUtility.ResolvePrefabScaleFactor(baseRadius, snapshot);
            if (Mathf.Approximately(scaleFactor, 1f))
            {
                return;
            }

            target.localScale *= scaleFactor;
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

            var targets = ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
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
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
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

            var targets = ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
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

            var targets = ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            if (!coverAll && radius <= 0f)
            {
                var target = targets.Count > 0 ? targets[0] : null;
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
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
                var resolvedDamage = SkillExecutionUtility.ResolveDamageAgainstTarget(damage, snapshot, target.Model);
                manager.ApplyDamage(target.Model, resolvedDamage, attribute, source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                routed = true;
            }

            return routed;
        }

        private static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec)
        {
            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, unitRoster, targetingSpec);
            var targets = new List<UnitRosterEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            var selection = targetingSpec != null ? targetingSpec.Selection : SkillTargetSelection.Nearest;
            targets.Sort((left, right) => CompareTargets(sourceEntry, selection, left, right));
            return targets;
        }

        private static int CompareTargets(UnitRosterEntry sourceEntry, SkillTargetSelection selection, UnitRosterEntry left, UnitRosterEntry right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var leftHealth = left.Model != null && left.Model.Resources != null ? left.Model.Resources.CurrentHealth : 0f;
            var rightHealth = right.Model != null && right.Model.Resources != null ? right.Model.Resources.CurrentHealth : 0f;
            if (selection == SkillTargetSelection.HighestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return rightHealth.CompareTo(leftHealth);
            }

            if (selection == SkillTargetSelection.LowestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return leftHealth.CompareTo(rightHealth);
            }

            var leftDistance = ResolveDistanceSquared(sourceEntry, left);
            var rightDistance = ResolveDistanceSquared(sourceEntry, right);
            return leftDistance.CompareTo(rightDistance);
        }

        private static float ResolveDistanceSquared(UnitRosterEntry sourceEntry, UnitRosterEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
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

    internal static class SkillMultiEffectExecutor
    {
        public static bool Execute(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, null);
        }

        internal static bool ExecuteOnExpire(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            return ExecuteFiltered(context, snapshot, effects, fallbackCenter, SkillMultiEffectTiming.OnExpire);
        }

        private static bool ExecuteFiltered(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter,
            SkillMultiEffectTiming? requiredTiming)
        {
            if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
            {
                return false;
            }

            var routed = false;
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                if (requiredTiming.HasValue)
                {
                    if (effect.EffectTiming != requiredTiming.Value)
                    {
                        continue;
                    }
                }
                else if (effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || effect.EffectTiming == SkillMultiEffectTiming.OnExpire)
                {
                    continue;
                }

                if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
                {
                    context.CombatManager.StartCoroutine(ExecuteDelayed(context, snapshot, effect, fallbackCenter));
                    routed = true;
                    continue;
                }

                routed = ExecuteEffect(context, snapshot, effect, fallbackCenter) || routed;
            }

            return routed;
        }

        private static IEnumerator ExecuteDelayed(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            var delay = effect != null ? Mathf.Max(0f, effect.DelaySeconds) : 0f;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }

            ExecuteEffect(context, snapshot, effect, fallbackCenter);
        }

        internal static bool ShouldRun(SkillExecutionContext context, SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            if (effect == null)
            {
                return false;
            }

            if (!effect.EnabledByDefault && string.IsNullOrWhiteSpace(effect.RequiresActiveChoiceId))
            {
                return false;
            }

            if (!HasAllChoices(snapshot, effect.RequiresActiveChoiceId))
            {
                return false;
            }

            if (HasAnyChoice(snapshot, effect.ExcludesActiveChoiceId))
            {
                return false;
            }

            if (!HasAllLearnedPassives(context, effect.RequiresPassiveSkillId))
            {
                return false;
            }

            return !HasAnyLearnedPassive(context, effect.ExcludesPassiveSkillId);
        }

        private static bool HasAllChoices(SkillExecutionSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (!string.IsNullOrWhiteSpace(choice) && !snapshot.HasActiveChoice(choice.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyChoice(SkillExecutionSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (!string.IsNullOrWhiteSpace(choice) && snapshot.HasActiveChoice(choice.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAllLearnedPassives(SkillExecutionContext context, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && !HasLearnedPassive(context, passiveId.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyLearnedPassive(SkillExecutionContext context, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && HasLearnedPassive(context, passiveId.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLearnedPassive(SkillExecutionContext context, string passiveId)
        {
            var monster = context != null ? context.Caster as MonsterUnitRuntimeModel : null;
            return monster != null
                && monster.State != null
                && !string.IsNullOrWhiteSpace(passiveId)
                && monster.State.LearnedPassiveSkillIds.Contains(passiveId);
        }

        private static bool ExecuteEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return false;
            }

            switch (effect.EffectKind)
            {
                case SkillMultiEffectKind.Damage:
                    return ExecuteDamageEffect(context, snapshot, effect, fallbackCenter);
                case SkillMultiEffectKind.Status:
                    return ExecuteStatusEffect(context, snapshot, effect, fallbackCenter);
                case SkillMultiEffectKind.ExtendStatusDuration:
                    return ExecuteExtendStatusDurationEffect(context, effect);
            }

            return false;
        }

        private static bool ExecuteDamageEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            var targeting = BuildTargeting(effect);
            var center = ResolveEffectCenter(context, effect, targeting, fallbackCenter);
            var damageSpec = new SkillDamageSpec
            {
                Element = (ElementType)(int)effect.Attribute,
                BaseDamage = effect.BaseDamage,
                StatCoefficient = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient)
                    ? effect.SpellPowerCoefficient
                    : effect.AttackPowerCoefficient,
                StatSource = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient)
                    ? StatSource.Intelligence
                    : StatSource.Attack,
                CriticalAllowed = true
            };

            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, damageSpec, snapshot) * Mathf.Max(0f, effect.DamageMultiplier);
            var statusSpec = ResolveStatusSpec(effect, snapshot);
            var routed = InGameZoneSkillActor.ApplyAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                ResolveRadius(effect, snapshot),
                effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                damage,
                effect.Attribute,
                statusSpec,
                context.Caster,
                !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId,
                damageSpec.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            if (routed)
            {
                SpawnVisual(context, effect, center);
            }

            return routed;
        }

        private static bool ExecuteExtendStatusDurationEffect(
            SkillExecutionContext context,
            SkillEffectDefinition effect)
        {
            if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || effect == null)
            {
                return false;
            }

            var statusKey = !string.IsNullOrWhiteSpace(effect.StatusEffectId)
                ? effect.StatusEffectId
                : effect.StatusEffectLabel;
            if (!StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                return false;
            }

            var durationDelta = Mathf.Max(0f, effect.StatusDurationSeconds);
            if (durationDelta <= 0f)
            {
                return false;
            }

            var targeting = BuildTargeting(effect);
            var targets = SkillExecutionUtility.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                routed = context.CombatManager.ExtendStatusDuration(target.Model, kind, durationDelta) || routed;
            }

            return routed;
        }

        private static bool ExecuteStatusEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            var statusSpec = ResolveStatusSpec(effect, snapshot);
            if (statusSpec == null || !statusSpec.Enabled)
            {
                return false;
            }

            var targeting = BuildTargeting(effect);
            var targets = SkillExecutionUtility.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
            var visualTargets = effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets
                ? new List<UnitRosterEntry>()
                : null;
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                if (!TargetMatchesCondition(target.Model, effect))
                {
                    continue;
                }

                if (statusSpec.StatusData != null && statusSpec.StatusData.Kind == StatusEffectKind.Shield)
                {
                    context.CombatManager.ApplyShieldStatus(
                        target.Model,
                        statusSpec.StatusData,
                        ResolveStatusEffectShieldAmount(context.Caster, effect),
                        statusSpec.DurationSeconds,
                        statusSpec.Stacks,
                        statusSpec.MaxStacks,
                        statusSpec.Permanent,
                        statusSpec.RefreshDuration,
                        context.Caster);
                }
                else
                {
                    if (!SkillStatusApplyUtility.TryApplyStatus(context.CombatManager, target.Model, statusSpec, context.Caster))
                    {
                        continue;
                    }
                }
                if (visualTargets != null)
                {
                    visualTargets.Add(target);
                }

                routed = true;
            }

            if (routed)
            {
                if (visualTargets != null)
                {
                    SpawnVisualOnTargets(context, effect, visualTargets, statusSpec.DurationSeconds);
                }
                else
                {
                    SpawnVisual(context, effect, ResolveEffectCenter(context, effect, targeting, fallbackCenter));
                }
            }

            return routed;
        }

        internal static bool TargetMatchesCondition(BaseUnitRuntimeModel target, SkillEffectDefinition effect)
        {
            if (effect == null)
            {
                return true;
            }

            var statusMatches = true;
            if (!string.IsNullOrWhiteSpace(effect.ConditionStatusId))
            {
                statusMatches = StatusEffectRuntime.MatchesConditionStatus(target, effect.ConditionStatusId);
            }

            var skillMatches = true;
            if (!string.IsNullOrWhiteSpace(effect.ConditionSkillAttribute))
            {
                skillMatches = HasActiveSkillAttribute(target, effect.ConditionSkillAttribute);
            }

            return statusMatches && skillMatches;
        }

        private static float ResolveStatusDurationBonus(
            SkillExecutionSnapshot snapshot,
            StatusEffectData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectUtility.GetDefinition(kind).Id;
            return snapshot.ResolveStatusDurationBonus(statusId);
        }

        internal static ProjectileStatusHitSpec ResolveStatusSpec(
            SkillEffectDefinition effect,
            SkillExecutionSnapshot snapshot = null)
        {
            var statusData = CreateStatusData(effect);
            if (statusData == null)
            {
                return null;
            }

            var definition = StatusEffectUtility.GetDefinition(statusData.Kind);
            var duration = statusData.Duration > 0f ? statusData.Duration : definition.DefaultDurationSeconds;
            var targetedDurationBonus = ResolveStatusDurationBonus(snapshot, statusData, statusData.Kind);
            if (!Mathf.Approximately(targetedDurationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + targetedDurationBonus);
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = statusData.Kind,
                StatusData = statusData,
                Chance = Mathf.Clamp01(effect.StatusChance > 0f ? effect.StatusChance : 1f),
                Stacks = Mathf.Max(1, effect.StatusStackAmount > 0 ? effect.StatusStackAmount : statusData.BaseStackAmount),
                DurationSeconds = duration,
                MaxStacks = statusData.MaxStacks,
                Permanent = statusData.Permanent,
                RefreshDuration = true
            };
        }

        private static StatusEffectData CreateStatusData(SkillEffectDefinition effect)
        {
            if (effect == null)
            {
                return null;
            }

            var statusKey = !string.IsNullOrWhiteSpace(effect.StatusEffectId)
                ? effect.StatusEffectId
                : effect.StatusEffectLabel;
            if (!StatusEffectUtility.TryParse(statusKey, out var kind))
            {
                return null;
            }

            var status = StatusEffectRuntime.CreateStatusData(kind, effect.StatusEffectLabel);
            if (status == null)
            {
                return null;
            }

            status.SourceSkillId = !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId;
            if (effect.StatusEffectPrefab != null)
            {
                status.StatusEffectPrefab = effect.StatusEffectPrefab;
            }

            if (StatusEffectRuntime.TryParseStatusTargetScope(effect.StatusTargetScope, out var scope))
            {
                status.TargetScope = scope;
            }

            status.MergePolicy = StatusEffectRuntime.TryParseStatusMergePolicy(effect.StatusMergePolicy, out var mergePolicy)
                ? mergePolicy
                : StatusMergePolicy.SameSourceRefresh;
            status.ShieldAmountRefreshPolicy = StatusEffectRuntime.TryParseShieldRefreshPolicy(effect.ShieldAmountRefreshPolicy, out var shieldPolicy)
                ? shieldPolicy
                : ShieldRefreshRule.TakeHighest;
            if (effect.StatusDurationSeconds > 0f)
            {
                status.Duration = effect.StatusDurationSeconds;
                status.Permanent = false;
            }

            if (effect.StatusMaxStacks > 0)
            {
                status.MaxStacks = effect.StatusMaxStacks;
                status.IsStackable = status.MaxStacks != 1;
            }

            if (effect.StatusStackAmount > 0)
            {
                status.BaseStackAmount = effect.StatusStackAmount;
            }

            status.Modifiers.ActionSpeedBonus = effect.StatusActionSpeedBonus;
            status.Modifiers.AttackPowerBonus = effect.StatusAttackPowerBonus;
            status.Modifiers.SpellPowerBonus = effect.StatusSpellPowerBonus;
            status.Modifiers.DamageBonusRate = effect.StatusDamageBonusRate;
            status.Modifiers.ShieldReceivedBonus = effect.StatusShieldReceivedBonus;
            status.Modifiers.CritChanceBonusRate = effect.StatusCriticalChanceBonus;
            status.MoveSpeedBonus = effect.StatusMoveSpeedBonus;
            status.MovementSlowRate = effect.StatusMoveSpeedBonus < 0f ? -effect.StatusMoveSpeedBonus : 0f;
            status.DamageTakenBonus = effect.StatusDamageTakenBonus;
            status.CriticalDamageTakenBonus = effect.StatusCriticalDamageTakenBonus;
            status.AilmentResistanceBonus = effect.StatusAilmentResistanceBonus;
            status.CriticalResistanceBonus = effect.StatusCriticalResistanceBonus;
            status.ElementResistReduction = effect.StatusElementResistReduction;
            status.FlatElementResistReduction = effect.StatusFlatElementResistReduction;
            status.ElementDamageTakenBonus = effect.StatusElementDamageTakenBonus;
            status.ConditionalTargetStatusTag = effect.StatusConditionalTargetStatusId;
            status.ConditionalStatusChanceBonus = effect.StatusConditionalStatusChanceBonus;
            status.AppliedStatusDurationBonusStatusId = effect.StatusAppliedStatusDurationBonusStatusId;
            status.AppliedStatusDurationBonus = effect.StatusAppliedStatusDurationBonus;
            status.HasElementModifierTarget = !Mathf.Approximately(effect.StatusDamageBonusRate, 0f)
                || !Mathf.Approximately(effect.StatusElementResistReduction, 0f)
                || !Mathf.Approximately(effect.StatusFlatElementResistReduction, 0f)
                || !Mathf.Approximately(effect.StatusElementDamageTakenBonus, 0f);
            status.ElementModifierTarget = (ElementType)(int)effect.Attribute;
            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.Modifiers.ResistReductionElement = status.ElementModifierTarget;
            return status;
        }

        private static bool HasActiveSkillAttribute(BaseUnitRuntimeModel target, string rawAttribute)
        {
            if (target == null
                || target.SkillRuntime == null
                || string.IsNullOrWhiteSpace(rawAttribute)
                || !Enum.TryParse(rawAttribute.Trim(), true, out ElementType attribute))
            {
                return false;
            }

            var activeSkills = target.SkillRuntime.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null && runtime.Data != null && runtime.Data.Element == attribute)
                {
                    return true;
                }
            }

            return false;
        }

        private static float ResolveStatusEffectShieldAmount(BaseUnitRuntimeModel caster, SkillEffectDefinition effect)
        {
            if (effect == null)
            {
                return 0f;
            }

            var useSpellPower = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient);
            var stats = caster != null ? caster.Stats : null;
            var stat = 0f;
            if (stats != null)
            {
                stat = useSpellPower
                    ? stats.SpellPower * StatusEffectRuntime.ResolveSpellPowerMultiplier(caster)
                    : stats.AttackPower * StatusEffectRuntime.ResolveAttackPowerMultiplier(caster);
            }

            var coefficient = useSpellPower ? effect.SpellPowerCoefficient : effect.AttackPowerCoefficient;
            return Mathf.Max(0f, (effect.BaseDamage + stat * coefficient) * Mathf.Max(0f, effect.DamageMultiplier));
        }

        private static SkillTargetingSpec BuildTargeting(SkillEffectDefinition effect)
        {
            return new SkillTargetingSpec
            {
                TargetSide = MapTargetSide(effect.TargetSide),
                Selection = MapTargetSelection(effect.TargetSelection),
                Shape = MapTargetShape(effect.TargetShape),
                Radius = effect.Radius,
                CoverAll = effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield
            };
        }

        private static SkillTargetSide MapTargetSide(SkillMultiEffectTargetSide side)
        {
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    return SkillTargetSide.Self;
                case SkillMultiEffectTargetSide.AllAllies:
                    return SkillTargetSide.AllAllies;
                default:
                    return SkillTargetSide.Enemy;
            }
        }

        private static SkillTargetSelection MapTargetSelection(SkillMultiEffectTargetSelection selection)
        {
            return selection == SkillMultiEffectTargetSelection.Owner
                ? SkillTargetSelection.Owner
                : SkillTargetSelection.Nearest;
        }

        private static SkillTargetShape MapTargetShape(SkillMultiEffectTargetShape shape)
        {
            switch (shape)
            {
                case SkillMultiEffectTargetShape.Battlefield:
                    return SkillTargetShape.Battlefield;
                case SkillMultiEffectTargetShape.Single:
                    return SkillTargetShape.Single;
                default:
                    return SkillTargetShape.Circle;
            }
        }

        private static Vector2 ResolveEffectCenter(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            SkillTargetingSpec targeting,
            Vector2 fallbackCenter)
        {
            if (effect != null)
            {
                switch (effect.CenterMode)
                {
                    case SkillMultiEffectCenterMode.PrimarySkillCenter:
                        return fallbackCenter;
                    case SkillMultiEffectCenterMode.Caster:
                        return context != null && context.CasterEntry != null && context.CasterEntry.Transform != null
                            ? (Vector2)context.CasterEntry.Transform.position
                            : fallbackCenter;
                    case SkillMultiEffectCenterMode.NearestEnemy:
                        var enemyTargeting = new SkillTargetingSpec
                        {
                            TargetSide = SkillTargetSide.Enemy,
                            Selection = SkillTargetSelection.Nearest,
                            Shape = SkillTargetShape.Circle,
                            Radius = effect.Radius,
                            CoverAll = false
                        };
                        var enemyTarget = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, enemyTargeting);
                        return enemyTarget != null && enemyTarget.Transform != null
                            ? (Vector2)enemyTarget.Transform.position
                            : fallbackCenter;
                }
            }

            var target = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            if (target != null && target.Transform != null)
            {
                return target.Transform.position;
            }

            return fallbackCenter;
        }

        private static void SpawnVisual(SkillExecutionContext context, SkillEffectDefinition effect, Vector2 center)
        {
            if (effect == null || effect.SkillEffectPrefab == null || context == null || context.CombatManager == null || context.CombatManager.Effects == null)
            {
                return;
            }

            SkillVisualSpawnUtility.SpawnTransient(context.CombatManager.Effects, effect.SkillEffectPrefab, center, Quaternion.identity, 1f);
        }

        private static void SpawnVisualOnTargets(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            IReadOnlyList<UnitRosterEntry> targets,
            float duration)
        {
            if (effect == null
                || effect.SkillEffectPrefab == null
                || context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || targets == null)
            {
                return;
            }

            var lifetime = Mathf.Max(0.1f, duration);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                SkillVisualSpawnUtility.SpawnAttached(
                    context.CombatManager.Effects,
                    effect.SkillEffectPrefab,
                    target.Transform,
                    lifetime,
                    Vector3.zero);
            }
        }

        private static float ResolveRadius(SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            return SkillAreaUtility.ResolveRadius(effect != null ? effect.Radius : 0f, snapshot);
        }
    }

    public sealed class BuffSkillExecutor : TypedSkillExecutor<BuffSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as BuffSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var statusSpec = ResolveBuffStatusSpec(skill, snapshot);
            if (statusSpec == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var targets = ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;
            var routed = false;
            var castCommitted = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                castCommitted = true;
                if (UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
                {
                    continue;
                }

                context.CombatManager.ApplyStatus(
                    target.Model,
                    statusSpec.StatusData,
                    statusSpec.Stacks,
                    statusSpec.DurationSeconds,
                    statusSpec.MaxStacks,
                    statusSpec.Permanent,
                    statusSpec.RefreshDuration,
                    context.Caster);

                if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
                {
                    SkillVisualSpawnUtility.SpawnAttached(
                        context.CombatManager.Effects,
                        prefab,
                        target.Transform,
                        statusSpec.DurationSeconds,
                        Vector3.zero);
                }

                routed = true;
            }

            return new SkillExecutionResult(routed || castCommitted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }

        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null)
            {
                return null;
            }

            var spec = ProjectileSkillExecutor.ResolveStatusSpec(skill.AttachedStatus, snapshot);
            if (spec != null)
            {
                return spec;
            }

            if (!string.IsNullOrWhiteSpace(skill.ApplyStatusTag)
                && StatusEffectUtility.TryParse(skill.ApplyStatusTag, out var kind))
            {
                var statusData = StatusEffectRuntime.CreateStatusData(kind, skill.ApplyStatusTag);
                if (statusData != null)
                {
                    statusData.SourceSkillId = skill.SkillId;
                    statusData.TargetScope = skill.Target == BuffTarget.Self
                        ? StatusTargetScope.Self
                        : StatusTargetScope.AllAllies;
                    statusData.MergePolicy = StatusMergePolicy.SameSourceRefresh;
                }

                return new ProjectileStatusHitSpec
                {
                    Enabled = true,
                    Kind = kind,
                    StatusData = statusData,
                    Chance = 1f,
                    Stacks = statusData != null ? Math.Max(1, statusData.BaseStackAmount) : 1,
                    DurationSeconds = statusData != null ? statusData.Duration : 0f,
                    MaxStacks = statusData != null ? statusData.MaxStacks : 0,
                    Permanent = statusData != null && statusData.Permanent,
                    RefreshDuration = true
                };
            }

            return null;
        }

        internal static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveBuffTargets(
            UnitRosterEntry caster,
            UnitRosterService roster,
            BuffTarget targetMode)
        {
            if (targetMode == BuffTarget.Self)
            {
                return caster != null
                    ? new[] { caster }
                    : System.Array.Empty<UnitRosterEntry>();
            }

            return SkillExecutionUtility.ResolveTargetList(
                caster,
                roster,
                new SkillTargetingSpec
                {
                    TargetSide = SkillTargetSide.AllAllies,
                    Selection = SkillTargetSelection.Owner,
                    Shape = SkillTargetShape.Battlefield,
                    CoverAll = true
                });
        }
    }

    public sealed class ShieldSkillExecutor : TypedSkillExecutor<ShieldSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ShieldSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var shield = SkillExecutionUtility.ResolveShield(context.Caster, skill, snapshot);
            var duration = skill.ShieldDuration > 0f
                ? skill.ShieldDuration
                : skill.ShieldStatus != null ? skill.ShieldStatus.Duration : 0f;
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            var statusData = ProjectileSkillExecutor.ResolveStatusData(skill.ShieldStatus, StatusEffectKind.Shield, snapshot);
            if (statusData == null || duration <= 0f)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            var targets = BuffSkillExecutor.ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusData,
                    shield,
                    duration,
                    1,
                    0,
                    false,
                    true,
                    context.Caster);
                if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
                {
                    SkillVisualSpawnUtility.SpawnAttached(
                        context.CombatManager.Effects,
                        prefab,
                        target.Transform,
                        duration,
                        Vector3.zero);
                }

                routed = true;
            }

            var multiEffectRouted = false;
            if (routed && skill.MultiEffects != null && skill.MultiEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillMultiEffectExecutor.Execute(context, snapshot, skill.MultiEffects, center);
            }

            return new SkillExecutionResult(routed || multiEffectRouted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }
    }

    public sealed class PassiveSkillExecutor : TypedSkillExecutor<PassiveSkillData>
    {
    }

    internal static class SkillExecutionUtility
    {
        public static UnitRosterEntry FindNearestTarget(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.FindNearestTarget(caster, roster, targeting);
        }

        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            return SkillTargetingUtility.DirectionToTarget(origin, target);
        }

        public static float ResolveDamage(
            BaseUnitRuntimeModel caster,
            SkillDamageSpec damage,
            SkillExecutionSnapshot snapshot)
        {
            if (damage == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, damage.StatSource);
            var baseDamage = Mathf.Max(0f, damage.BaseDamage + stat * damage.StatCoefficient);
            if (snapshot != null)
            {
                baseDamage = (baseDamage + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            baseDamage *= StatusEffectRuntime.ResolveOutgoingDamageMultiplier(caster, MapAttribute(damage.Element));
            return Mathf.Max(0f, baseDamage);
        }

        public static float ResolveShield(BaseUnitRuntimeModel caster, ShieldSkillData skill, SkillExecutionSnapshot snapshot = null)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            var shield = Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            return Mathf.Max(0f, shield);
        }

        public static float ResolveProjectileLifetime(ProjectileSkillData skill)
        {
            var projectile = skill != null ? skill.Projectile : null;
            var speed = projectile != null ? Mathf.Max(0.1f, projectile.ProjectileSpeed) : 1f;
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
        }

        public static float ResolveDamageAgainstTarget(
            float baseDamage,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel target)
        {
            if (snapshot == null || target == null)
            {
                return Mathf.Max(0f, baseDamage);
            }

            return Mathf.Max(0f, baseDamage * snapshot.ResolveConditionalDamageMultiplier(target));
        }

        public static DamageAttribute MapAttribute(ElementType element)
        {
            return (DamageAttribute)(int)element;
        }

        private static float ResolveStat(BaseUnitRuntimeModel caster, StatSource source)
        {
            var stats = caster != null ? caster.Stats : null;
            if (stats == null)
            {
                return 0f;
            }

            if (source == StatSource.Attack)
            {
                return stats.AttackPower * StatusEffectRuntime.ResolveAttackPowerMultiplier(caster);
            }

            return stats.SpellPower * StatusEffectRuntime.ResolveSpellPowerMultiplier(caster);
        }

        public static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.ResolveTargetList(caster, roster, targeting);
        }
    }
}
