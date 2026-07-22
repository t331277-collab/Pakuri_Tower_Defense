using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 단일 공격의 시전 조건, 피해 보정, 처치 후 처리를 계산한다.
 */
namespace Pakuri.InGame
{
    internal struct SingleDamageModifierState
    {
        /*
         * SingleDamageModifierState에 필요한 값을 초기화한다.
         */
        public SingleDamageModifierState(float damageMultiplier, float critChanceBonus)
        {
            DamageMultiplier = damageMultiplier;
            CritChanceBonus = critChanceBonus;
            IsExecute = false;
        }

        public float DamageMultiplier;
        public float CritChanceBonus;
        public bool IsExecute;
    }

    internal static class SingleSkillRules
    {
        /*
         * 처형 전용 스킬의 대상 체력이 시전 기준을 벗어났는지 확인한다.
         */
        internal static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            SingleSkillRuntimeData skill)
        {
            if (!skill.RequireExecuteThresholdToCast
                || !TryResolveThreshold(skill, snapshot, out var threshold))
            {
                return false;
            }

            var targets = SkillTargeting.ResolveOrderedTargets(
                context.CasterEntry,
                context.Roster,
                skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            return target == null || target.Model == null || !IsWithinThreshold(target.Model, threshold);
        }

        /*
         * 처형·보스 조건에 해당하는 피해와 치명타 보정을 적용한다.
         */
        internal static SingleDamageModifierState ApplyDamageModifiers(
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            UnitCombatState target,
            float damageMultiplier,
            float critChanceBonus)
        {
            var state = new SingleDamageModifierState(damageMultiplier, critChanceBonus);
            ApplyExecuteDamageModifier(skill, snapshot, target, ref state);
            ApplyBossDamageModifier(skill, snapshot, target, ref state);
            return state;
        }

        /*
         * 처치 조건에 맞춰 재사용 대기시간을 초기화하거나 일부 돌려준다.
         */
        internal static void HandleKillRecovery(
            SkillRuntimeInstance sourceRuntime,
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            InGameResourceChangeResult result,
            bool wasExecute)
        {
            if (sourceRuntime == null || !result.IsDead)
            {
                return;
            }

            if (TryResetCooldown(sourceRuntime, snapshot, wasExecute))
            {
                return;
            }

            TryRefundCooldown(sourceRuntime, skill, snapshot);
        }

        /*
         * TryResolveThreshold 작업을 시도하고 성공 여부를 반환한다.
         */
        private static bool TryResolveThreshold(
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            out float threshold)
        {
            var bonus = 0f;
            var ops = snapshot != null && snapshot.Plan != null ? snapshot.Plan.CastConditions : null;
            if (ops != null)
            {
                for (var i = 0; i < ops.Count; i++)
                {
                    bonus += ops[i].TargetHealthRatioBonus;
                }
            }

            threshold = Mathf.Clamp01(Mathf.Max(0f, skill.ExecuteHealthRatioThreshold) + bonus);
            return threshold > 0f;
        }

        /*
         * IsWithinThreshold 조건을 만족하는지 확인한다.
         */
        private static bool IsWithinThreshold(UnitCombatState target, float threshold)
        {
            var resources = target.Resources;
            var stats = target.Stats;
            if (resources == null || stats == null || stats.MaxHealth <= 0f || threshold <= 0f)
            {
                return false;
            }

            return resources.CurrentHealth / stats.MaxHealth <= threshold;
        }

        /*
         * ApplyExecuteDamageModifier 처리를 대상에 적용한다.
         */
        private static void ApplyExecuteDamageModifier(
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            UnitCombatState target,
            ref SingleDamageModifierState state)
        {
            if (!TryResolveThreshold(skill, snapshot, out var threshold)
                || !IsWithinThreshold(target, threshold))
            {
                return;
            }

            state.IsExecute = true;
            if (skill.ExecuteDamageMultiplier > 0f)
            {
                state.DamageMultiplier *= skill.ExecuteDamageMultiplier;
            }

            var plan = snapshot != null ? snapshot.Plan : null;
            if (plan == null)
            {
                return;
            }

            for (var i = 0; i < plan.DamageModifiers.Count; i++)
            {
                var op = plan.DamageModifiers[i];
                if (op.Kind == DamageModifierOpKind.ExecuteMultiplier)
                {
                    state.DamageMultiplier *= op.Multiplier;
                }
            }

            for (var i = 0; i < plan.CritModifiers.Count; i++)
            {
                var op = plan.CritModifiers[i];
                state.CritChanceBonus += op.ChanceBonus;
            }
        }

        /*
         * ApplyBossDamageModifier 처리를 대상에 적용한다.
         */
        private static void ApplyBossDamageModifier(
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            UnitCombatState target,
            ref SingleDamageModifierState state)
        {
            if (!target.IsBoss)
            {
                return;
            }

            if (skill.BossDamageMultiplier > 0f)
            {
                state.DamageMultiplier *= skill.BossDamageMultiplier;
            }

            var plan = snapshot != null ? snapshot.Plan : null;
            if (plan == null)
            {
                return;
            }

            for (var i = 0; i < plan.DamageModifiers.Count; i++)
            {
                var op = plan.DamageModifiers[i];
                if (op.Kind == DamageModifierOpKind.BossMultiplier)
                {
                    state.DamageMultiplier *= op.Multiplier;
                }
            }
        }

        /*
         * TryResetCooldown 작업을 시도하고 성공 여부를 반환한다.
         */
        private static bool TryResetCooldown(
            SkillRuntimeInstance sourceRuntime,
            SkillSnapshot snapshot,
            bool wasExecute)
        {
            var plan = snapshot != null ? snapshot.Plan : null;
            if (plan == null)
            {
                return false;
            }

            for (var i = 0; i < plan.KillActions.Count; i++)
            {
                var op = plan.KillActions[i];
                if (op.Kind != KillActionOpKind.CooldownReset
                    || (op.RequiresExecute && !wasExecute))
                {
                    continue;
                }

                sourceRuntime.ResetCooldown();
                return true;
            }

            return false;
        }

        /*
         * TryRefundCooldown 작업을 시도하고 성공 여부를 반환한다.
         */
        private static bool TryRefundCooldown(
            SkillRuntimeInstance sourceRuntime,
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot)
        {
            var refundBonus = 0f;
            var plan = snapshot != null ? snapshot.Plan : null;
            if (plan != null)
            {
                for (var i = 0; i < plan.KillActions.Count; i++)
                {
                    var op = plan.KillActions[i];
                    if (op.Kind == KillActionOpKind.CooldownRefundBonus)
                    {
                        refundBonus += op.RatioBonus;
                    }
                }
            }

            var refundRatio = Mathf.Clamp01(skill.KillCooldownRefundRatio + refundBonus);
            if (refundRatio <= 0f)
            {
                return false;
            }

            sourceRuntime.ReduceCooldownRemaining(sourceRuntime.EffectiveCooldownDuration * refundRatio);
            return true;
        }
    }
}
