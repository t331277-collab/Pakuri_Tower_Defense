using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 상태 설정을 계산하고 전투 대상에 적용한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillStatus
    {

        /*
         * 상태 설정을 결정한다.
         */
        internal static ProjectileStatusHitSpec ResolveStatusSpec(
            StatusApplicationSpec baseStatus,
            SkillSnapshot snapshot)
        {
            var statusData = baseStatus != null ? baseStatus.Status : null;
            var snapshotTag = snapshot != null ? snapshot.StatusTag : null;
            var tag = !string.IsNullOrWhiteSpace(snapshotTag)
                ? snapshotTag
                : statusData != null ? statusData.StatusTag : null;
            var kind = statusData != null ? statusData.Kind : StatusEffectKind.None;
            if (!StatusEffectLookup.TryParse(tag, out var parsedKind) && kind == StatusEffectKind.None)
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

            var definition = StatusEffectLookup.GetDefinition(kind);
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
            SkillSnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(statusId)
                || stacks <= 0
                || !StatusEffectLookup.TryParse(statusId, out var kind))
            {
                return null;
            }

            var statusData = ResolveStatusData(StatusRuntimeDataFactory.Create(kind, null), kind, snapshot);
            if (statusData == null)
            {
                return null;
            }

            var definition = StatusEffectLookup.GetDefinition(kind);
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
        internal static StatusRuntimeData ResolveStatusData(
            StatusRuntimeData statusData,
            StatusEffectKind kind,
            SkillSnapshot snapshot)
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
                : StatusEffectLookup.GetDefinition(kind).Id;
            var actionSpeedBonus = snapshot != null ? snapshot.ResolveStatusActionSpeedBonus(statusId) : 0f;
            var needsChoiceActionSpeedOverride = !Mathf.Approximately(actionSpeedBonus, 0f);
            var needsChoiceAttackPowerOverride = snapshot != null && snapshot.HasStatusAttackPowerBonus;
            if (statusData == null || statusData.Kind != kind)
            {
                statusData = StatusRuntimeDataFactory.Create(kind, null);
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
         * 상태 적용 확률과 지속시간을 계산해 대상에게 적용한다.
         */
        internal static bool TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source = null)
        {
            if (manager == null || target == null || status == null || !status.Enabled)
            {
                return false;
            }

            var chance = ResolveApplicationChance(target, status, source);
            if (chance <= 0f || UnityEngine.Random.value > chance)
            {
                return false;
            }

            var appliedStatus = manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                ResolveDurationSeconds(status, source),
                status.MaxStacks,
                status.Permanent,
                status.RefreshDuration,
                source);
            if (appliedStatus == null)
            {
                return false;
            }

            TryApplyThresholdStatus(manager, target, status, source);
            return true;
        }

        /*
         * 대상의 상태 저항을 반영한 최종 적용 확률을 계산한다.
         */
        internal static float ResolveApplicationChance(
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chance = Mathf.Clamp01(
                status.Chance + StatusCombatRules.ResolveConditionalStatusChanceBonus(source, target));
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - StatusCombatRules.ResolveAilmentResistanceBonus(target));
        }

        private static float ResolveDurationSeconds(
            ProjectileStatusHitSpec status,
            UnitCombatState source)
        {
            var duration = Mathf.Max(0f, status.DurationSeconds);
            var statusId = ResolveStatusId(status);
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                duration = Mathf.Max(
                    0f,
                    duration + StatusCombatRules.ResolveAppliedStatusDurationBonus(source, statusId));
            }

            return duration;
        }

        private static bool IsDebuff(StatusRuntimeData statusData)
        {
            if (statusData == null)
            {
                return false;
            }

            return statusData.IsControlEffect
                || statusData.MoveSpeedBonus < 0f
                || statusData.DamageTakenBonus > 0f
                || statusData.CriticalDamageTakenBonus > 0f
                || statusData.ConditionalDamageTakenBonus > 0f
                || statusData.ElementDamageTakenBonus > 0f
                || statusData.ElementResistReduction > 0f
                || statusData.FlatElementResistReduction > 0f
                || statusData.Modifiers.ActionSpeedBonus < 0f
                || statusData.Modifiers.AttackPowerBonus < 0f
                || statusData.Modifiers.SpellPowerBonus < 0f
                || statusData.Modifiers.DamageBonusRate < 0f;
        }

        private static void TryApplyThresholdStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source)
        {
            if (target.Statuses == null
                || status.ThresholdStatusSpec == null
                || !status.ThresholdStatusSpec.Enabled
                || string.IsNullOrWhiteSpace(status.ThresholdSourceStatusId)
                || status.ThresholdSourceMinStacks <= 0
                || !StatusEffectLookup.TryParse(status.ThresholdSourceStatusId, out var triggerKind))
            {
                return;
            }

            if (target.Statuses.GetStacks(triggerKind) < status.ThresholdSourceMinStacks)
            {
                return;
            }

            var thresholdStatus = status.ThresholdStatusSpec;
            manager.ApplyStatus(
                target,
                thresholdStatus.StatusData,
                thresholdStatus.Stacks,
                thresholdStatus.DurationSeconds,
                thresholdStatus.MaxStacks,
                thresholdStatus.Permanent,
                thresholdStatus.RefreshDuration,
                source);
        }

        private static string ResolveStatusId(ProjectileStatusHitSpec status)
        {
            var statusData = status.StatusData;
            if (statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag))
            {
                return statusData.StatusTag;
            }

            return StatusEffectLookup.ToId(status.Kind);
        }

        /*
         * 상태 지속시간 보너스를 결정한다.
         */
        private static float ResolveStatusDurationBonus(
            SkillSnapshot snapshot,
            StatusRuntimeData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectLookup.GetDefinition(kind).Id;
            return snapshot.ResolveStatusDurationBonus(statusId);
        }

        /*
         * 상태 최대 중첩 보너스를 결정한다.
         */
        private static int ResolveStatusMaxStacksBonus(
            SkillSnapshot snapshot,
            StatusRuntimeData statusData,
            StatusEffectKind kind)
        {
            if (snapshot == null)
            {
                return 0;
            }

            var statusId = statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag)
                ? statusData.StatusTag
                : StatusEffectLookup.GetDefinition(kind).Id;
            return snapshot.ResolveStatusMaxStacksBonus(statusId);
        }

        /*
         * 임계값 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec ResolveThresholdStatusSpec(SkillSnapshot snapshot)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(snapshot.ThresholdApplyStatusId)
                || !StatusEffectLookup.TryParse(snapshot.ThresholdApplyStatusId, out var kind))
            {
                return null;
            }

            var statusData = StatusRuntimeDataFactory.Create(kind, null);
            if (statusData == null)
            {
                return null;
            }

            var definition = StatusEffectLookup.GetDefinition(kind);
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



