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

            manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                status.DurationSeconds,
                status.MaxStacks,
                status.Permanent,
                status.RefreshDuration);
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
    }
}
