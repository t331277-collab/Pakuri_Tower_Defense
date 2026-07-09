using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
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
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = RuntimeSkillVisualFactory.HasVisual(runtimeVisual);
            var prefab = !hasRuntimeVisual && snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : !hasRuntimeVisual && context.CombatManager.Effects != null
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

                if (hasRuntimeVisual && target.Transform != null && context.CombatManager.Effects != null)
                {
                    SkillVisualSpawnUtility.SpawnAttached(
                        context.CombatManager.Effects,
                        runtimeVisual,
                        string.IsNullOrWhiteSpace(skill.SkillId) ? "RuntimeBuffVisual" : $"RuntimeBuffVisual_{skill.SkillId}",
                        target.Transform,
                        statusSpec.DurationSeconds,
                        Vector3.zero);
                }
                else if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
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

            var multiEffectRouted = false;
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillMultiEffectExecutor.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return new SkillExecutionResult(routed || castCommitted || multiEffectRouted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }

        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null)
            {
                return null;
            }

            var spec = SkillStatusSpecUtility.ResolveStatusSpec(skill.AttachedStatus, snapshot);
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

            var statusData = SkillStatusSpecUtility.ResolveStatusData(skill.ShieldStatus, StatusEffectKind.Shield, snapshot);
            if (statusData == null || duration <= 0f)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = RuntimeSkillVisualFactory.HasVisual(runtimeVisual);
            var prefab = !hasRuntimeVisual && snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : !hasRuntimeVisual && context.CombatManager.Effects != null
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
                if (hasRuntimeVisual && target.Transform != null && context.CombatManager.Effects != null)
                {
                    SkillVisualSpawnUtility.SpawnAttached(
                        context.CombatManager.Effects,
                        runtimeVisual,
                        string.IsNullOrWhiteSpace(skill.SkillId) ? "RuntimeShieldVisual" : $"RuntimeShieldVisual_{skill.SkillId}",
                        target.Transform,
                        duration,
                        Vector3.zero);
                }
                else if (prefab != null && target.Transform != null && context.CombatManager.Effects != null)
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
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillMultiEffectExecutor.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return new SkillExecutionResult(routed || multiEffectRouted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }
    }

    public sealed class PassiveSkillExecutor : TypedSkillExecutor<PassiveSkillData>
    {
    }
}
