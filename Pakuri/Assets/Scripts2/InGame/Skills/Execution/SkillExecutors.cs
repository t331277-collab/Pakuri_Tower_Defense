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
                    context.CombatManager.ApplyDamage(target.Model, damage, attribute);
                    TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec);
                    return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
                }

                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
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
                        context.CombatManager.ApplyDamage(target.Model, damage, attribute);
                        TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec);
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
                        context.CombatManager.ApplyDamage(target.Model, damage, attribute);
                        TryApplyDirectStatus(context.CombatManager, target.Model, statusSpec);
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
                    branchSpec);
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
            var permanent = definition.Permanent && (resolvedStatusData == null || resolvedStatusData.Duration <= 0f);
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
                RefreshDuration = baseStatus == null || baseStatus.RefreshDuration
            };
        }

        private static StatusEffectData ResolveStatusData(
            StatusEffectData statusData,
            StatusEffectKind kind,
            SkillExecutionSnapshot snapshot)
        {
            var needsChoiceElementDamageOverride = snapshot != null && snapshot.HasStatusElementDamageTakenBonus;
            if (statusData == null || statusData.Kind != kind)
            {
                statusData = StatusEffectRuntime.CreateStatusData(kind, null);
            }

            if (statusData == null || !needsChoiceElementDamageOverride)
            {
                return statusData;
            }

            var overriddenStatus = UnityEngine.Object.Instantiate(statusData);
            overriddenStatus.hideFlags = HideFlags.DontSave;
            overriddenStatus.ElementDamageTakenBonus = snapshot.StatusElementDamageTakenBonus;
            return overriddenStatus;
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
            ProjectileStatusHitSpec statusSpec)
        {
            if (combatManager == null || target == null || statusSpec == null || !statusSpec.Enabled)
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
            {
                return;
            }

            combatManager.ApplyStatus(
                target,
                statusSpec.StatusData,
                statusSpec.Stacks,
                statusSpec.DurationSeconds,
                statusSpec.MaxStacks,
                statusSpec.Permanent,
                statusSpec.RefreshDuration);
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
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            if (prefab == null || context.CombatManager.Effects == null)
            {
                var routed = InGameLineAttackActor.ApplyLineTick(
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
                    statusSpec);
                return new SkillExecutionResult(routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var instance = context.CombatManager.Effects.InstantiateSkillPrefab(
                prefab,
                origin + (Vector3)(direction * (length * 0.5f)),
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
                statusSpec);
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
                width = width * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0.1f, width);
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

            var center = ResolveAreaCenter(context, skill.Targeting, skill.Area);
            var radius = ResolveRadius(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            var statusSpec = ProjectileSkillExecutor.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            GameObject instance = null;
            if (prefab != null && context.CombatManager.Effects != null)
            {
                instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
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
                damage,
                attribute,
                statusSpec);
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            var origin = context.CasterEntry != null && context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            if (context.HasManualAimDirection && context.ManualAimDirection.sqrMagnitude > 0.0001f)
            {
                var radius = area != null && area.Radius > 0f
                    ? area.Radius
                    : targeting != null ? targeting.Radius : 1f;
                return (Vector2)origin + context.ManualAimDirection.normalized * Mathf.Max(1f, radius);
            }

            var target = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            return target != null && target.Transform != null
                ? (Vector2)target.Transform.position
                : (Vector2)origin;
        }

        private static float ResolveRadius(ZoneSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            var radius = area != null && area.Radius > 0f
                ? area.Radius
                : targeting != null ? targeting.Radius : 0f;
            if (snapshot != null)
            {
                radius = radius * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0f, radius);
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
    }

    public sealed class SingleAttackSkillExecutor : TypedSkillExecutor<SingleAttackData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as SingleAttackData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var center = ResolveAreaCenter(context, skill.Targeting, skill.Area);
            var radius = ResolveRadius(skill, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var statusSpec = ProjectileSkillExecutor.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var prefab = ResolvePrefab(context, snapshot, skill);
            var spawnedHitbox = false;
            var routed = false;
            if (skill.UsePrefabHitbox && prefab != null && context.CombatManager.Effects != null)
            {
                center = ResolvePrefabHitboxCenter(context, center, skill);
                var instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                if (instance != null)
                {
                    spawnedHitbox = true;
                    ApplyHitboxScale(instance.transform, snapshot);
                    Physics2D.SyncTransforms();
                    routed = ApplyPrefabHitbox(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        instance,
                        skill.HitAllTargets ? int.MaxValue : skill.HitTargetCount,
                        damage,
                        attribute,
                        statusSpec);
                    UnityEngine.Object.Destroy(instance, 1f);
                }
            }

            if (!spawnedHitbox)
            {
                if (skill.UsesHitTargetCount && !skill.HitAllTargets)
                {
                    routed = ApplyLimitedTargets(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        skill.HitTargetCount,
                        damage,
                        attribute,
                        statusSpec);
                }
                else
                {
                    routed = InGameZoneSkillActor.ApplyAreaTick(
                        context.CombatManager,
                        context.CasterEntry,
                        context.Roster,
                        skill.Targeting,
                        center,
                        radius,
                        coverAll,
                        damage,
                        attribute,
                        statusSpec);
                }

                SpawnVisual(context, prefab, center);
            }

            SkillMultiEffectExecutor.Execute(context, snapshot, skill.MultiEffects, center);
            return new SkillExecutionResult(routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }

        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            var origin = context.CasterEntry != null && context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            if (context.HasManualAimDirection && context.ManualAimDirection.sqrMagnitude > 0.0001f)
            {
                var radius = area != null && area.Radius > 0f
                    ? area.Radius
                    : targeting != null ? targeting.Radius : 1f;
                return (Vector2)origin + context.ManualAimDirection.normalized * Mathf.Max(1f, radius);
            }

            var target = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            return target != null && target.Transform != null
                ? (Vector2)target.Transform.position
                : (Vector2)origin;
        }

        private static float ResolveRadius(SingleAttackData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            var radius = area != null && area.Radius > 0f
                ? area.Radius
                : targeting != null ? targeting.Radius : 0f;
            if (snapshot != null)
            {
                radius = radius * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0f, radius);
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

            var instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance, 1f);
            }
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

        private static void ApplyHitboxScale(Transform target, SkillExecutionSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            var multiplier = Mathf.Max(0f, snapshot.RadiusMultiplier);
            var additive = snapshot.RadiusBonus;
            var scaleFactor = Mathf.Max(0.01f, multiplier + additive);
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
            ProjectileStatusHitSpec statusSpec)
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

                manager.ApplyDamage(target.Model, damage, attribute);
                TryApplyStatus(manager, target.Model, statusSpec);
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
            ProjectileStatusHitSpec statusSpec)
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
                manager.ApplyDamage(target.Model, damage, attribute);
                TryApplyStatus(manager, target.Model, statusSpec);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
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
            if (hitboxColliders == null || target == null || target.Transform == null || target.Model == null || !target.IsAlive)
            {
                return false;
            }

            var targetColliders = target.Transform.GetComponentsInChildren<Collider2D>();
            for (var i = 0; i < hitboxColliders.Length; i++)
            {
                var hitbox = hitboxColliders[i];
                if (hitbox == null || !hitbox.enabled)
                {
                    continue;
                }

                if (hitbox.OverlapPoint(target.Transform.position))
                {
                    return true;
                }

                for (var j = 0; j < targetColliders.Length; j++)
                {
                    var targetCollider = targetColliders[j];
                    if (targetCollider == null || !targetCollider.enabled)
                    {
                        continue;
                    }

                    if (hitbox.Distance(targetCollider).isOverlapped)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void TryApplyStatus(InGameCombatManager manager, BaseUnitRuntimeModel target, ProjectileStatusHitSpec statusSpec)
        {
            if (manager == null || target == null || statusSpec == null || !statusSpec.Enabled)
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
            {
                return;
            }

            manager.ApplyStatus(
                target,
                statusSpec.StatusData,
                statusSpec.Stacks,
                statusSpec.DurationSeconds,
                statusSpec.MaxStacks,
                statusSpec.Permanent,
                statusSpec.RefreshDuration);
        }
    }

    internal static class SkillMultiEffectExecutor
    {
        public static void Execute(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects,
            Vector2 fallbackCenter)
        {
            if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
            {
                return;
            }

            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!ShouldRun(effect, snapshot))
                {
                    continue;
                }

                if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
                {
                    context.CombatManager.StartCoroutine(ExecuteDelayed(context, snapshot, effect, fallbackCenter));
                    continue;
                }

                ExecuteEffect(context, snapshot, effect, fallbackCenter);
            }
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

        private static bool ShouldRun(SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
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

            return !HasAnyChoice(snapshot, effect.ExcludesActiveChoiceId);
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

        private static void ExecuteEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return;
            }

            switch (effect.EffectKind)
            {
                case SkillMultiEffectKind.Damage:
                    ExecuteDamageEffect(context, snapshot, effect, fallbackCenter);
                    break;
                case SkillMultiEffectKind.Status:
                    ExecuteStatusEffect(context, effect, fallbackCenter);
                    break;
            }
        }

        private static void ExecuteDamageEffect(
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
            var statusSpec = ResolveStatusSpec(effect);
            InGameZoneSkillActor.ApplyAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                ResolveRadius(effect, snapshot),
                effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                damage,
                effect.Attribute,
                statusSpec);
            SpawnVisual(context, effect, center);
        }

        private static void ExecuteStatusEffect(
            SkillExecutionContext context,
            SkillEffectDefinition effect,
            Vector2 fallbackCenter)
        {
            var statusSpec = ResolveStatusSpec(effect);
            if (statusSpec == null || !statusSpec.Enabled)
            {
                return;
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

                if (statusSpec.Chance < 1f && UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
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
                        statusSpec.RefreshDuration);
                }
                else
                {
                    context.CombatManager.ApplyStatus(
                        target.Model,
                        statusSpec.StatusData,
                        statusSpec.Stacks,
                        statusSpec.DurationSeconds,
                        statusSpec.MaxStacks,
                        statusSpec.Permanent,
                        statusSpec.RefreshDuration);
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
        }

        private static bool TargetMatchesCondition(BaseUnitRuntimeModel target, SkillEffectDefinition effect)
        {
            if (effect == null || string.IsNullOrWhiteSpace(effect.ConditionStatusId))
            {
                return true;
            }

            if (!StatusEffectUtility.TryParse(effect.ConditionStatusId, out var kind))
            {
                return false;
            }

            if (target != null && target.Statuses != null && target.Statuses.Has(kind))
            {
                return true;
            }

            return kind == StatusEffectKind.Shield
                && target != null
                && target.Resources != null
                && target.Resources.CurrentShield > 0f;
        }

        private static ProjectileStatusHitSpec ResolveStatusSpec(SkillEffectDefinition effect)
        {
            var statusData = CreateStatusData(effect);
            if (statusData == null)
            {
                return null;
            }

            var definition = StatusEffectUtility.GetDefinition(statusData.Kind);
            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = statusData.Kind,
                StatusData = statusData,
                Chance = Mathf.Clamp01(effect.StatusChance > 0f ? effect.StatusChance : 1f),
                Stacks = Mathf.Max(1, effect.StatusStackAmount > 0 ? effect.StatusStackAmount : statusData.BaseStackAmount),
                DurationSeconds = statusData.Duration > 0f ? statusData.Duration : definition.DefaultDurationSeconds,
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
            status.MoveSpeedBonus = effect.StatusMoveSpeedBonus;
            status.MovementSlowRate = effect.StatusMoveSpeedBonus < 0f ? -effect.StatusMoveSpeedBonus : 0f;
            status.DamageTakenBonus = effect.StatusDamageTakenBonus;
            status.CriticalDamageTakenBonus = effect.StatusCriticalDamageTakenBonus;
            status.CriticalResistanceBonus = effect.StatusCriticalResistanceBonus;
            status.ElementResistReduction = effect.StatusElementResistReduction;
            status.ElementDamageTakenBonus = effect.StatusElementDamageTakenBonus;
            status.HasElementModifierTarget = !Mathf.Approximately(effect.StatusDamageBonusRate, 0f)
                || !Mathf.Approximately(effect.StatusElementResistReduction, 0f)
                || !Mathf.Approximately(effect.StatusElementDamageTakenBonus, 0f);
            status.ElementModifierTarget = (ElementType)(int)effect.Attribute;
            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.Modifiers.ResistReductionElement = status.ElementModifierTarget;
            return status;
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

            var instance = context.CombatManager.Effects.InstantiateSkillPrefab(effect.SkillEffectPrefab, center, Quaternion.identity);
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance, 1f);
            }
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

                var instance = context.CombatManager.Effects.InstantiateSkillPrefab(
                    effect.SkillEffectPrefab,
                    target.Transform.position,
                    Quaternion.identity);
                if (instance == null)
                {
                    continue;
                }

                var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                }

                actor.Initialize(target.Transform, lifetime, Vector3.zero);
            }
        }

        private static float ResolveRadius(SkillEffectDefinition effect, SkillExecutionSnapshot snapshot)
        {
            var radius = effect != null ? effect.Radius : 0f;
            if (snapshot != null)
            {
                radius = radius * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0f, radius);
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
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

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
                    statusSpec.RefreshDuration);

                if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
                {
                    var instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
                    if (instance != null)
                    {
                        var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                        if (actor == null)
                        {
                            actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                        }

                        actor.Initialize(target.Transform, Mathf.Max(0.1f, statusSpec.DurationSeconds), Vector3.zero);
                    }
                }

                routed = true;
            }

            return new SkillExecutionResult(routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
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

            var shield = SkillExecutionUtility.ResolveShield(context.Caster, skill);
            var duration = skill.ShieldDuration > 0f
                ? skill.ShieldDuration
                : skill.ShieldStatus != null ? skill.ShieldStatus.Duration : 0f;
            var statusData = skill.ShieldStatus;
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
                    true);
                if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
                {
                    var instance = context.CombatManager.Effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
                    if (instance != null)
                    {
                        var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                        if (actor == null)
                        {
                            actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                        }

                        actor.Initialize(target.Transform, duration, Vector3.zero);
                    }
                }

                routed = true;
            }

            return new SkillExecutionResult(routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
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
            if (caster == null || caster.Transform == null || roster == null)
            {
                return null;
            }

            var candidates = ResolveTargetList(caster, roster, targeting);
            var selection = targeting != null ? targeting.Selection : SkillTargetSelection.Nearest;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var bestHealth = float.MinValue;
            var bestLowestHealth = float.MaxValue;
            var origin = caster.Transform.position;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Transform == null || candidate.Model == null)
                {
                    continue;
                }

                if (selection == SkillTargetSelection.HighestHealth)
                {
                    var health = candidate.Model.Resources != null ? candidate.Model.Resources.CurrentHealth : 0f;
                    if (health < bestHealth)
                    {
                        continue;
                    }

                    if (Mathf.Approximately(health, bestHealth))
                    {
                        var tieOffset = candidate.Transform.position - origin;
                        tieOffset.z = 0f;
                        var tieDistanceSq = tieOffset.sqrMagnitude;
                        if (tieDistanceSq >= bestDistanceSq)
                        {
                            continue;
                        }

                        bestDistanceSq = tieDistanceSq;
                    }
                    else
                    {
                        var offsetForTie = candidate.Transform.position - origin;
                        offsetForTie.z = 0f;
                        bestDistanceSq = offsetForTie.sqrMagnitude;
                    }

                    best = candidate;
                    bestHealth = health;
                    continue;
                }

                if (selection == SkillTargetSelection.LowestHealth)
                {
                    var health = candidate.Model.Resources != null ? candidate.Model.Resources.CurrentHealth : 0f;
                    if (health > bestLowestHealth)
                    {
                        continue;
                    }

                    if (Mathf.Approximately(health, bestLowestHealth))
                    {
                        var tieOffset = candidate.Transform.position - origin;
                        tieOffset.z = 0f;
                        var tieDistanceSq = tieOffset.sqrMagnitude;
                        if (tieDistanceSq >= bestDistanceSq)
                        {
                            continue;
                        }

                        bestDistanceSq = tieDistanceSq;
                    }
                    else
                    {
                        var offsetForTie = candidate.Transform.position - origin;
                        offsetForTie.z = 0f;
                        bestDistanceSq = offsetForTie.sqrMagnitude;
                    }

                    best = candidate;
                    bestLowestHealth = health;
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            if (target == null || target.Transform == null)
            {
                return Vector2.zero;
            }

            var direction = target.Transform.position - origin;
            direction.z = 0f;
            return direction;
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

        public static float ResolveShield(BaseUnitRuntimeModel caster, ShieldSkillData skill)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            return Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
        }

        public static float ResolveProjectileLifetime(ProjectileSkillData skill)
        {
            var projectile = skill != null ? skill.Projectile : null;
            var speed = projectile != null ? Mathf.Max(0.1f, projectile.ProjectileSpeed) : 1f;
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
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
            if (caster == null || roster == null)
            {
                return System.Array.Empty<UnitRosterEntry>();
            }

            var side = targeting != null ? targeting.TargetSide : SkillTargetSide.Enemy;
            if (side == SkillTargetSide.Ally || side == SkillTargetSide.AllAllies || side == SkillTargetSide.Self)
            {
                return caster.Model != null
                    && caster.Model.Identity != null
                    && caster.Model.Identity.Side == UnitSide.Enemy
                        ? roster.Enemies
                        : roster.Players;
            }

            return caster.Model != null
                && caster.Model.Identity != null
                && caster.Model.Identity.Side == UnitSide.Enemy
                    ? roster.Players
                    : roster.Enemies;
        }
    }
}
