using System;
using Pakuri.Combat;
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
            var routed = InGameZoneSkillActor.ApplyAreaTick(
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

            SpawnVisual(context, snapshot, skill, center);
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

        private static void SpawnVisual(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackData skill,
            Vector2 center)
        {
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;
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
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = caster.Transform.position;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Transform == null)
                {
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

            return stats.SpellPower;
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
