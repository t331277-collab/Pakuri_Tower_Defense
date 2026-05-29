using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillStatusSpecUtility
    {

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
            var needsChoiceActionSpeedOverride = snapshot != null && snapshot.HasStatusActionSpeedBonus;
            var needsChoiceAttackPowerOverride = snapshot != null && snapshot.HasStatusAttackPowerBonus;
            if (statusData == null || statusData.Kind != kind)
            {
                statusData = StatusEffectRuntime.CreateStatusData(kind, null);
            }

            if (statusData == null
                || (!needsChoiceElementDamageOverride
                    && !needsChoiceCriticalDamageOverride
                    && !needsChoiceAilmentResistanceOverride
                    && !needsChoiceConditionalDamageTakenOverride
                    && !needsChoiceActionSpeedOverride
                    && !needsChoiceAttackPowerOverride))
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

            if (needsChoiceActionSpeedOverride)
            {
                overriddenStatus.Modifiers.ActionSpeedBonus += snapshot.StatusActionSpeedBonus;
            }

            if (needsChoiceAttackPowerOverride)
            {
                overriddenStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
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
    }
}

