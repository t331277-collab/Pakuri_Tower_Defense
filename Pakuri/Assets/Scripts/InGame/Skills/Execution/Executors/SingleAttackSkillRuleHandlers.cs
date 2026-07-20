using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * I 스킬 시전 조건 구현에 필요한 계약을 정의한다.
 */
namespace Pakuri.InGame
{
    internal interface ISkillCastCondition
    {
        /*
         * 거부를 실행해야 하는지 확인한다.
         */
        bool ShouldReject(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackSkillRuntimeData skill);
    }

    /*
     * I 스킬 피해 보정값 구현에 필요한 계약을 정의한다.
     */
    internal interface ISkillDamageModifier
    {
        /*
         * 대상과 스킬 조건에 맞는 피해 보정값을 적용한다.
         */
        void Apply(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state);
    }

    /*
     * I 스킬 적중 후 적중 행동 구현에 필요한 계약을 정의한다.
     */
    internal interface ISkillPostHitAction
    {
        /*
         * 적중 결과에 후속 행동을 적용한다.
         */
        bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute);
    }

    /*
     * 단일 공격 피해 보정값 상태값에 필요한 값을 보관한다.
     */
    internal struct SingleAttackDamageModifierState
    {
        /*
         * 단일 공격 피해 보정값 상태값에 필요한 값을 초기화한다.
         */
        public SingleAttackDamageModifierState(float damageMultiplier, float critChanceBonus)
        {
            DamageMultiplier = damageMultiplier;
            CritChanceBonus = critChanceBonus;
            IsExecute = false;
        }

        public float DamageMultiplier;
        public float CritChanceBonus;
        public bool IsExecute;
    }

    /*
     * 단일 공격의 시전 조건, 피해 보정, 처치 후 처리를 조율한다.
     */
    internal static class SingleAttackSkillRuleHandlers
    {
        private static readonly ISkillCastCondition ExecuteCastCondition = new TargetHealthRatioCastCondition();
        private static readonly ISkillDamageModifier[] DamageModifiers =
        {
            new ExecuteDamageModifier(),
            new BossDamageModifier()
        };

        private static readonly ISkillPostHitAction[] KillActions =
        {
            new KillCooldownResetAction(),
            new KillCooldownRefundAction()
        };

        /*
         * 처형 체력 조건 때문에 시전을 거부해야 하는지 확인한다.
         */
        public static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill)
        {
            return ExecuteCastCondition.ShouldReject(context, snapshot, skill);
        }

        /*
         * 피해 보정값을 적용한다.
         */
        public static SingleAttackDamageModifierState ApplyDamageModifiers(
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel target,
            float damageMultiplier,
            float critChanceBonus)
        {
            var state = new SingleAttackDamageModifierState(damageMultiplier, critChanceBonus);
            for (var i = 0; i < DamageModifiers.Length; i++)
            {
                DamageModifiers[i].Apply(skill, snapshot, target, ref state);
            }

            return state;
        }

        /*
         * 대상 처치 결과에 따라 재사용 대기시간 회복을 처리한다.
         */
        public static void HandleKillRecovery(
            SkillRuntimeInstance sourceRuntime,
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            InGameResourceChangeResult result,
            bool wasExecute)
        {
            if (sourceRuntime == null || !result.IsDead)
            {
                return;
            }

            for (var i = 0; i < KillActions.Length; i++)
            {
                if (KillActions[i].TryApply(sourceRuntime, skill, snapshot, result, wasExecute))
                {
                    return;
                }
            }
        }
    }

    /*
     * 대상 체력 비율을 기준으로 단일 공격 시전 가능 여부를 판단한다.
     */
    internal sealed class TargetHealthRatioCastCondition : ISkillCastCondition
    {
        /*
         * 거부를 실행해야 하는지 확인한다.
         */
        public bool ShouldReject(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackSkillRuntimeData skill)
        {
            if (context == null
                || skill == null
                || !skill.RequireExecuteThresholdToCast)
            {
                return false;
            }

            if (!TryResolveThreshold(skill, snapshot, out var threshold))
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            return target == null || target.Model == null || !IsWithinThreshold(target.Model, threshold);
        }

        /*
         * 임계값을 결정하고 성공 여부를 반환한다.
         */
        public static bool TryResolveThreshold(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, out float threshold)
        {
            var bonus = 0f;
            var ops = snapshot != null && snapshot.Plan != null ? snapshot.Plan.CastConditions : null;
            if (ops != null)
            {
                for (var i = 0; i < ops.Count; i++)
                {
                    var op = ops[i];
                    if (op.Kind == CastConditionOpKind.TargetHealthRatioBonus)
                    {
                        bonus += op.Value;
                    }
                }
            }

            threshold = Mathf.Clamp01(Mathf.Max(0f, skill != null ? skill.ExecuteHealthRatioThreshold : 0f) + bonus);
            return threshold > 0f;
        }

        /*
         * 현재 체력 비율이 지정한 임계값 안인지 확인한다.
         */
        public static bool IsWithinThreshold(BaseUnitRuntimeModel target, float threshold)
        {
            var resources = target != null ? target.Resources : null;
            var stats = target != null ? target.Stats : null;
            if (resources == null || stats == null || stats.MaxHealth <= 0f || threshold <= 0f)
            {
                return false;
            }

            return resources.CurrentHealth / stats.MaxHealth <= threshold;
        }
    }

    /*
     * 처형 조건을 만족한 대상에게 피해 배율을 적용한다.
     */
    internal sealed class ExecuteDamageModifier : ISkillDamageModifier
    {
        /*
         * 처형 체력 조건을 만족하면 피해 배율을 적용한다.
         */
        public void Apply(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state)
        {
            if (skill == null
                || target == null
                || !TargetHealthRatioCastCondition.TryResolveThreshold(skill, snapshot, out var threshold)
                || !TargetHealthRatioCastCondition.IsWithinThreshold(target, threshold))
            {
                return;
            }

            state.IsExecute = true;
            state.DamageMultiplier *= skill.ExecuteDamageMultiplier > 0f ? skill.ExecuteDamageMultiplier : 1f;
            var damageOps = snapshot != null && snapshot.Plan != null ? snapshot.Plan.DamageModifiers : null;
            if (damageOps != null)
            {
                for (var i = 0; i < damageOps.Count; i++)
                {
                    var op = damageOps[i];
                    if (op.Kind == DamageModifierOpKind.ExecuteMultiplier)
                    {
                        state.DamageMultiplier *= op.Multiplier;
                    }
                }
            }

            var critOps = snapshot != null && snapshot.Plan != null ? snapshot.Plan.CritModifiers : null;
            if (critOps == null)
            {
                return;
            }

            for (var i = 0; i < critOps.Count; i++)
            {
                var op = critOps[i];
                if (op.Kind == CritModifierOpKind.ExecuteChanceBonus)
                {
                    state.CritChanceBonus += op.ChanceBonus;
                }
            }
        }
    }

    /*
     * 보스 대상에게 지정된 피해 배율을 적용한다.
     */
    internal sealed class BossDamageModifier : ISkillDamageModifier
    {
        /*
         * 대상이 보스이면 지정된 피해 배율을 적용한다.
         */
        public void Apply(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state)
        {
            if (skill == null || target == null || !target.IsBoss)
            {
                return;
            }

            state.DamageMultiplier *= skill.BossDamageMultiplier > 0f ? skill.BossDamageMultiplier : 1f;
            var ops = snapshot != null && snapshot.Plan != null ? snapshot.Plan.DamageModifiers : null;
            if (ops == null)
            {
                return;
            }

            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];
                if (op.Kind == DamageModifierOpKind.BossMultiplier)
                {
                    state.DamageMultiplier *= op.Multiplier;
                }
            }
        }
    }

    /*
     * 처치 성공 시 스킬 재사용 대기시간을 초기화한다.
     */
    internal sealed class KillCooldownResetAction : ISkillPostHitAction
    {
        /*
         * 처치 성공 시 재사용 대기시간 초기화를 시도한다.
         */
        public bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute)
        {
            if (sourceRuntime == null
                || !result.IsDead
                || snapshot == null
                || snapshot.Plan == null
                || snapshot.Plan.KillActions == null)
            {
                return false;
            }

            var ops = snapshot.Plan.KillActions;
            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];
                if (op.Kind != KillActionOpKind.CooldownReset)
                {
                    continue;
                }

                if (op.RequiresExecute && !wasExecute)
                {
                    continue;
                }

                sourceRuntime.ResetCooldown();
                return true;
            }

            return false;
        }
    }

    /*
     * 처치 성공 시 스킬 재사용 대기시간 일부를 돌려준다.
     */
    internal sealed class KillCooldownRefundAction : ISkillPostHitAction
    {
        /*
         * 처치 성공 시 재사용 대기시간 반환을 시도한다.
         */
        public bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute)
        {
            if (sourceRuntime == null || !result.IsDead)
            {
                return false;
            }

            var refundBonus = 0f;
            var ops = snapshot != null && snapshot.Plan != null ? snapshot.Plan.KillActions : null;
            if (ops != null)
            {
                for (var i = 0; i < ops.Count; i++)
                {
                    var op = ops[i];
                    if (op.Kind == KillActionOpKind.CooldownRefundBonus)
                    {
                        refundBonus += op.RatioBonus;
                    }
                }
            }

            var refundRatio = Mathf.Clamp01((skill != null ? skill.KillCooldownRefundRatio : 0f) + refundBonus);
            if (refundRatio <= 0f)
            {
                return false;
            }

            sourceRuntime.ReduceCooldownRemaining(sourceRuntime.EffectiveCooldownDuration * refundRatio);
            return true;
        }
    }
}
