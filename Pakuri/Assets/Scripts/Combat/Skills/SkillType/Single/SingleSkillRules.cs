using System.Collections.Generic;
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
        public SingleDamageModifierState(float damageMultiplier /* 피해량에 곱할 배율 */, float critChanceBonus /* 추가 치명타 확률 */)
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
        // 처형 조건, 보스 피해 보정, 처치 후 쿨다운 회복 규칙을 구현.
        /*
         * 처형 전용 스킬의 대상 체력이 시전 기준을 벗어났는지 확인한다.
         */
        internal static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float damageMultiplier /* 피해량에 곱할 배율 */,
            float critChanceBonus /* 추가 치명타 확률 */)
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
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            InGameResourceChangeResult result /* 처리 결과 */,
            bool wasExecute /* 발생 처형 여부 */)
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
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            out float threshold /* 기준값 */)
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

        /*
         * IsWithinThreshold 조건을 만족하는지 확인한다.
         */
        private static bool IsWithinThreshold(UnitCombatState target /* 효과를 받을 대상 유닛 */, float threshold /* 기준값 */)
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
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ref SingleDamageModifierState state /* 상태 */)
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

        /*
         * ApplyBossDamageModifier 처리를 대상에 적용한다.
         */
        private static void ApplyBossDamageModifier(
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ref SingleDamageModifierState state /* 상태 */)
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

        /*
         * TryResetCooldown 작업을 시도하고 성공 여부를 반환한다.
         */
        private static bool TryResetCooldown(
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            bool wasExecute /* 발생 처형 여부 */)
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

        /*
         * TryRefundCooldown 작업을 시도하고 성공 여부를 반환한다.
         */
        private static bool TryRefundCooldown(
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
