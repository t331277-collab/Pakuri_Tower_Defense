using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 상태 설정 계산과 변환 기능을 제공한다.
     */
    internal static class SkillStatusSpecUtility
    {

        /*
         * 상태 설정을 결정한다.
         */
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

        /*
         * 직접 상태 설정을 생성한다.
         */
        internal static ProjectileStatusHitSpec CreateDirectStatusSpec(
            string statusId,
            int stacks,
            SkillExecutionSnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(statusId)
                || stacks <= 0
                || !StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return null;
            }

            var statusData = ResolveStatusData(StatusEffectFactory.Create(kind, null), kind, snapshot);
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

            var maxStacks = statusData.MaxStacks > 0 ? statusData.MaxStacks : definition.DefaultMaxStacks;
            var targetedMaxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, statusData, kind);
            if (targetedMaxStacksBonus != 0)
            {
                maxStacks = Mathf.Max(0, maxStacks + targetedMaxStacksBonus);
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = true,
                Kind = kind,
                StatusData = statusData,
                Chance = 1f,
                Stacks = Mathf.Max(1, stacks),
                DurationSeconds = duration,
                MaxStacks = maxStacks,
                Permanent = statusData.Permanent && duration <= 0f,
                RefreshDuration = true
            };
        }

        /*
         * 상태 데이터를 결정한다.
         */
        internal static RuntimeStatusData ResolveStatusData(
            RuntimeStatusData statusData,
            StatusEffectKind kind,
            SkillExecutionSnapshot snapshot)
        {
            var needsChoiceElementDamageOverride = snapshot != null && snapshot.HasStatusElementDamageTakenBonus;
            var needsChoiceCriticalDamageOverride = snapshot != null && snapshot.HasStatusCriticalDamageTakenBonus;
            var needsChoiceAilmentResistanceOverride = snapshot != null && snapshot.HasStatusAilmentResistanceBonus;
            var needsChoiceDamageBonusRateOverride = snapshot != null && snapshot.HasStatusDamageBonusRate;
            var needsChoiceShieldReceivedOverride = snapshot != null && snapshot.HasStatusShieldReceivedBonus;
            var needsChoiceCriticalChanceOverride = snapshot != null && snapshot.HasStatusCriticalChanceBonus;
            var needsChoiceDamageTakenOverride = snapshot != null && snapshot.HasStatusDamageTakenBonus;
            var needsChoiceFlatElementResistOverride = snapshot != null && snapshot.HasStatusFlatElementResistReduction;
            var needsChoiceConditionalDamageTakenOverride = snapshot != null && snapshot.HasStatusConditionalDamageTakenBonus;
            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectUtility.GetDefinition(kind).Id;
            var actionSpeedBonus = snapshot != null ? snapshot.ResolveStatusActionSpeedBonus(statusId) : 0f;
            var needsChoiceActionSpeedOverride = !Mathf.Approximately(actionSpeedBonus, 0f);
            var needsChoiceAttackPowerOverride = snapshot != null && snapshot.HasStatusAttackPowerBonus;
            if (statusData == null || statusData.Kind != kind)
            {
                statusData = StatusEffectFactory.Create(kind, null);
            }

            if (statusData == null
                || (!needsChoiceElementDamageOverride
                    && !needsChoiceCriticalDamageOverride
                    && !needsChoiceAilmentResistanceOverride
                    && !needsChoiceDamageBonusRateOverride
                    && !needsChoiceShieldReceivedOverride
                    && !needsChoiceCriticalChanceOverride
                    && !needsChoiceDamageTakenOverride
                    && !needsChoiceFlatElementResistOverride
                    && !needsChoiceConditionalDamageTakenOverride
                    && !needsChoiceActionSpeedOverride
                    && !needsChoiceAttackPowerOverride))
            {
                return statusData;
            }

            var overriddenStatus = statusData.Clone();
            if (needsChoiceElementDamageOverride)
            {
                overriddenStatus.ElementDamageTakenBonus += snapshot.StatusElementDamageTakenBonus;
            }

            if (needsChoiceCriticalDamageOverride)
            {
                overriddenStatus.CriticalDamageTakenBonus += snapshot.StatusCriticalDamageTakenBonus;
            }

            if (needsChoiceAilmentResistanceOverride)
            {
                overriddenStatus.AilmentResistanceBonus += snapshot.StatusAilmentResistanceBonus;
            }

            if (needsChoiceDamageBonusRateOverride)
            {
                overriddenStatus.Modifiers.DamageBonusRate += snapshot.StatusDamageBonusRate;
            }

            if (needsChoiceShieldReceivedOverride)
            {
                overriddenStatus.Modifiers.ShieldReceivedBonus += snapshot.StatusShieldReceivedBonus;
            }

            if (needsChoiceCriticalChanceOverride)
            {
                overriddenStatus.Modifiers.CritChanceBonusRate += snapshot.StatusCriticalChanceBonus;
            }

            if (needsChoiceDamageTakenOverride)
            {
                overriddenStatus.DamageTakenBonus += snapshot.StatusDamageTakenBonus;
            }

            if (needsChoiceFlatElementResistOverride)
            {
                overriddenStatus.FlatElementResistReduction += snapshot.StatusFlatElementResistReduction;
            }

            if (needsChoiceConditionalDamageTakenOverride)
            {
                overriddenStatus.ConditionalSourceStatusTag = snapshot.StatusConditionalSourceStatusId;
                overriddenStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
            }

            if (needsChoiceActionSpeedOverride)
            {
                overriddenStatus.Modifiers.ActionSpeedBonus += actionSpeedBonus;
            }

            if (needsChoiceAttackPowerOverride)
            {
                overriddenStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
            }

            return overriddenStatus;
        }

        /*
         * 상태 지속시간 보너스를 결정한다.
         */
        private static float ResolveStatusDurationBonus(
            SkillExecutionSnapshot snapshot,
            RuntimeStatusData statusData,
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

        /*
         * 상태 최대 중첩 보너스를 결정한다.
         */
        private static int ResolveStatusMaxStacksBonus(
            SkillExecutionSnapshot snapshot,
            RuntimeStatusData statusData,
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

        /*
         * 임계값 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec ResolveThresholdStatusSpec(SkillExecutionSnapshot snapshot)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(snapshot.ThresholdApplyStatusId)
                || !StatusEffectUtility.TryParse(snapshot.ThresholdApplyStatusId, out var kind))
            {
                return null;
            }

            var statusData = StatusEffectFactory.Create(kind, null);
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

