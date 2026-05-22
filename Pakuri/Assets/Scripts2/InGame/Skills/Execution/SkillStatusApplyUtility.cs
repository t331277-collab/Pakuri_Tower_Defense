using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillStatusApplyUtility
    {
        public static bool TryApplyStatus(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            ProjectileStatusHitSpec status)
        {
            if (manager == null || target == null || status == null || !status.Enabled)
            {
                return false;
            }

            var chance = ResolveApplicationChance(target, status);
            if (chance <= 0f || Random.value > chance)
            {
                return false;
            }

            var appliedStatus = manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                status.DurationSeconds,
                status.MaxStacks,
                status.Permanent,
                status.RefreshDuration);
            if (appliedStatus == null)
            {
                return false;
            }

            TryApplyThresholdStatus(manager, target, status);
            return true;
        }

        public static float ResolveApplicationChance(BaseUnitRuntimeModel target, ProjectileStatusHitSpec status)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chance = Mathf.Clamp01(status.Chance);
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - StatusEffectRuntime.ResolveAilmentResistanceBonus(target));
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
            ProjectileStatusHitSpec status)
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
                thresholdStatus.RefreshDuration);
        }
    }
}
