using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 상태 효과의 확률, 지속시간, 적용 조건을 계산한다.
     */
    internal static class SkillStatusApplyUtility
    {
        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
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

        /*
         * 적용 확률을 결정한다.
         */
        public static float ResolveApplicationChance(BaseUnitRuntimeModel target, ProjectileStatusHitSpec status, BaseUnitRuntimeModel source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chance = Mathf.Clamp01(status.Chance + StatusEffectRules.ResolveConditionalStatusChanceBonus(source, target));
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - StatusEffectRules.ResolveAilmentResistanceBonus(target));
        }

        /*
         * 지속시간 초를 결정한다.
         */
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
                duration = Mathf.Max(0f, duration + StatusEffectRules.ResolveAppliedStatusDurationBonus(source, statusId));
            }

            return duration;
        }

        /*
         * 대상에게 불리한 상태 효과인지 확인한다.
         */
        private static bool IsDebuff(RuntimeStatusData statusData)
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

        /*
         * 임계값 상태를 적용하고 성공 여부를 반환한다.
         */
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

        /*
         * 상태 ID를 결정한다.
         */
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
