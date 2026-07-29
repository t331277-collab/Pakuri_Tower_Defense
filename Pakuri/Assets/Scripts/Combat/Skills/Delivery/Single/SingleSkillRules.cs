/*
 * 역할: 단일 대상 전달 규칙.
 * 책임: 대상 유효성·연쇄 순서·돌진 위치와 공통 단일 스킬 판단을 결정한다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>SingleDamageModifierState</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    internal struct SingleDamageModifierState
    {

        /// <summary><c>SingleDamageModifierState</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

    /// <summary><c>SingleSkillRules</c>에 공통으로 적용되는 런타임 규칙을 구현한다.</summary>
    internal static class SingleSkillRules
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>RejectCastForExecuteThreshold</c> 실행 필요 여부를 반환한다.</summary>
        internal static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            SingleSkillDefinition skill)
        {
            if (!skill.RequireExecuteThresholdToCast
                || !TryResolveThreshold(skill, snapshot, out var threshold))
            {
                return false;
            }

            var targets = SkillTargeting.OrderedTargets(context, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            return target == null || target.Model == null || !IsWithinThreshold(target.Model, threshold);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>DamageModifiers</c>를 적용한다.</summary>
        internal static SingleDamageModifierState ApplyDamageModifiers(
            SingleSkillDefinition skill,
            SkillExecutionData snapshot,
            UnitCombatState target,
            float damageMultiplier,
            float critChanceBonus)
        {
            var state = new SingleDamageModifierState(damageMultiplier, critChanceBonus);
            ApplyExecuteDamageModifier(skill, snapshot, target, ref state);
            ApplyBossDamageModifier(skill, snapshot, target, ref state);
            return state;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>KillRecovery</c>를 처리한다.</summary>
        internal static void HandleKillRecovery(
            SkillUseState sourceRuntime,
            SingleSkillDefinition skill,
            SkillExecutionData snapshot,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolveThreshold</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static bool TryResolveThreshold(
            SingleSkillDefinition skill,
            SkillExecutionData snapshot,
            out float threshold)
        {
            var bonus = 0f;
            IReadOnlyList<CastConditionOp> ops = null;
            if (snapshot != null)
            {
                ops = snapshot.CastConditionOps;
            }
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>WithinThreshold</c> 조건 충족 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteDamageModifier</c>를 적용한다.</summary>
        private static void ApplyExecuteDamageModifier(
            SingleSkillDefinition skill,
            SkillExecutionData snapshot,
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

            if (snapshot == null)
            {
                return;
            }

            for (var i = 0; i < snapshot.DamageModifierOps.Count; i++)
            {
                var op = snapshot.DamageModifierOps[i];
                if (op.Kind == DamageModifierOpKind.ExecuteMultiplier)
                {
                    state.DamageMultiplier *= op.Multiplier;
                }
            }

            for (var i = 0; i < snapshot.CritModifierOps.Count; i++)
            {
                var op = snapshot.CritModifierOps[i];
                state.CritChanceBonus += op.ChanceBonus;
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>BossDamageModifier</c>를 적용한다.</summary>
        private static void ApplyBossDamageModifier(
            SingleSkillDefinition skill,
            SkillExecutionData snapshot,
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

            if (snapshot == null)
            {
                return;
            }

            for (var i = 0; i < snapshot.DamageModifierOps.Count; i++)
            {
                var op = snapshot.DamageModifierOps[i];
                if (op.Kind == DamageModifierOpKind.BossMultiplier)
                {
                    state.DamageMultiplier *= op.Multiplier;
                }
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResetCooldown</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static bool TryResetCooldown(
            SkillUseState sourceRuntime,
            SkillExecutionData snapshot,
            bool wasExecute)
        {
            if (snapshot == null)
            {
                return false;
            }

            for (var i = 0; i < snapshot.KillActionOps.Count; i++)
            {
                var op = snapshot.KillActionOps[i];
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>RefundCooldown</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static bool TryRefundCooldown(
            SkillUseState sourceRuntime,
            SingleSkillDefinition skill,
            SkillExecutionData snapshot)
        {
            var refundBonus = 0f;
            if (snapshot != null)
            {
                for (var i = 0; i < snapshot.KillActionOps.Count; i++)
                {
                    var op = snapshot.KillActionOps[i];
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
