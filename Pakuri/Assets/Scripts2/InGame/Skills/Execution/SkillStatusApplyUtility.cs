using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillStatusApplyUtility
    {
        public static bool TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status,
            BaseUnitRuntimeModel source = null)
        {
            if (manager == null || target == null || status == null || !status.Enabled)
            {
                return false;
            }

            var chance = ResolveApplicationChance(target, status, source);
            if (chance <= 0f || Random.value > chance)
            {
                return false;
            }

            var durationSeconds = ResolveDurationSeconds(status, source);

            var appliedStatus = manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                durationSeconds,
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

        public static float ResolveApplicationChance(BaseUnitRuntimeModel target, ProjectileStatusHitSpec status, BaseUnitRuntimeModel source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chance = Mathf.Clamp01(status.Chance + StatusEffectRuntime.ResolveConditionalStatusChanceBonus(source, target));
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - StatusEffectRuntime.ResolveAilmentResistanceBonus(target));
        }

        private static float ResolveDurationSeconds(ProjectileStatusHitSpec status, BaseUnitRuntimeModel source)
        {
            if (status == null)
            {
                return 0f;
            }

            var duration = Mathf.Max(0f, status.DurationSeconds);
            var statusId = ResolveStatusId(status);
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                duration = Mathf.Max(0f, duration + StatusEffectRuntime.ResolveAppliedStatusDurationBonus(source, statusId));
            }

            return duration;
        }

        private static bool IsDebuff(StatusEffectData statusData)
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
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status,
            BaseUnitRuntimeModel source)
        {
            if (manager == null
                || target == null
                || target.Statuses == null
                || status == null
                || status.ThresholdStatusSpec == null
                || !status.ThresholdStatusSpec.Enabled
                || string.IsNullOrWhiteSpace(status.ThresholdSourceStatusId)
                || status.ThresholdSourceMinStacks <= 0
                || !StatusEffectUtility.TryParse(status.ThresholdSourceStatusId, out var triggerKind))
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
            var statusData = status != null ? status.StatusData : null;
            if (statusData != null && !string.IsNullOrWhiteSpace(statusData.StatusTag))
            {
                return statusData.StatusTag;
            }

            return status != null ? StatusEffectUtility.ToId(status.Kind) : string.Empty;
        }
    }
}
