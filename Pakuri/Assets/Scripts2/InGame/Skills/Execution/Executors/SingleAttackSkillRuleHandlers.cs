using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal interface ISkillCastCondition
    {
        bool ShouldReject(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackData skill);
    }

    internal interface ISkillDamageModifier
    {
        void Apply(SingleAttackData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state);
    }

    internal interface ISkillPostHitAction
    {
        bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute);
    }

    internal struct SingleAttackDamageModifierState
    {
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

        public static bool ShouldRejectCastForExecuteThreshold(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackData skill)
        {
            return ExecuteCastCondition.ShouldReject(context, snapshot, skill);
        }

        public static SingleAttackDamageModifierState ApplyDamageModifiers(
            SingleAttackData skill,
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

        public static void HandleKillRecovery(
            SkillRuntimeInstance sourceRuntime,
            SingleAttackData skill,
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

    internal sealed class TargetHealthRatioCastCondition : ISkillCastCondition
    {
        public bool ShouldReject(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackData skill)
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

        public static bool TryResolveThreshold(SingleAttackData skill, SkillExecutionSnapshot snapshot, out float threshold)
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

    internal sealed class ExecuteDamageModifier : ISkillDamageModifier
    {
        public void Apply(SingleAttackData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state)
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
            var ops = snapshot != null && snapshot.Plan != null ? snapshot.Plan.CritModifiers : null;
            if (ops == null)
            {
                return;
            }

            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];
                if (op.Kind == CritModifierOpKind.ExecuteChanceBonus)
                {
                    state.CritChanceBonus += op.ChanceBonus;
                }
            }
        }
    }

    internal sealed class BossDamageModifier : ISkillDamageModifier
    {
        public void Apply(SingleAttackData skill, SkillExecutionSnapshot snapshot, BaseUnitRuntimeModel target, ref SingleAttackDamageModifierState state)
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

    internal sealed class KillCooldownResetAction : ISkillPostHitAction
    {
        public bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute)
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

    internal sealed class KillCooldownRefundAction : ISkillPostHitAction
    {
        public bool TryApply(SkillRuntimeInstance sourceRuntime, SingleAttackData skill, SkillExecutionSnapshot snapshot, InGameResourceChangeResult result, bool wasExecute)
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
