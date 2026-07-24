using System;
using Pakuri.NewCore.Definitions.Choices;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public enum SkillNodeBehavior
    {
        Condition,
        Damage,
        Status,
        Timing,
        Projectile,
        Targeting,
        Effect,
        Resource
    }

    public static class SkillNodeSupport
    {
        public static SkillNodeBehavior Resolve(string handlerId)
        {
            if (string.IsNullOrWhiteSpace(handlerId))
            {
                throw new NotSupportedException("A reachable node has no handler_id.");
            }

            if (handlerId.StartsWith("Condition", StringComparison.Ordinal)
                || handlerId.StartsWith("Required", StringComparison.Ordinal)
                || handlerId.StartsWith("TargetHealth", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Condition;
            }

            if (handlerId.StartsWith("Status", StringComparison.Ordinal)
                || handlerId.StartsWith("AttachStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("ThresholdApplyStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("ApplyStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("RedistributeConsumedStatus", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Status;
            }

            if (handlerId.IndexOf("Cooldown", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Reload", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Duration", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Interval", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("DamageDelay", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Timing;
            }

            if (handlerId.IndexOf("Projectile", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("Pierce", StringComparison.Ordinal)
                || handlerId.StartsWith("Magazine", StringComparison.Ordinal)
                || handlerId.StartsWith("Burst", StringComparison.Ordinal)
                || handlerId.StartsWith("Branch", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Projectile;
            }

            if (handlerId.IndexOf("Damage", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Crit", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("Core", StringComparison.Ordinal)
                || handlerId.StartsWith("ConsecutiveHit", StringComparison.Ordinal)
                || handlerId.StartsWith("EveryNthHit", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Damage;
            }

            if (handlerId.IndexOf("Radius", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("HitTarget", StringComparison.Ordinal)
                || handlerId.StartsWith("Beam", StringComparison.Ordinal)
                || handlerId.StartsWith("Knockback", StringComparison.Ordinal)
                || handlerId.StartsWith("RepeatPerTarget", StringComparison.Ordinal)
                || handlerId.StartsWith("EffectTarget", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Targeting;
            }

            if (handlerId.StartsWith("ApplyShield", StringComparison.Ordinal)
                || handlerId.StartsWith("Effect", StringComparison.Ordinal)
                || handlerId.StartsWith("RuntimeEffect", StringComparison.Ordinal)
                || handlerId.StartsWith("RecastZone", StringComparison.Ordinal)
                || handlerId.StartsWith("FollowUp", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Effect;
            }

            if (handlerId.StartsWith("ConsumeTarget", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Resource;
            }

            if (handlerId.StartsWith("TriggerProc", StringComparison.Ordinal)
                || handlerId.StartsWith("ShieldAmount", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Resource;
            }

            throw new NotSupportedException(
                $"Reachable node handler '{handlerId}' is not implemented.");
        }

        public static SkillNodeRuntimeOwner ResolveRuntimeOwner(
            ChoiceNodeDefinition node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (string.Equals(
                node.graph_kind,
                "Effect",
                StringComparison.Ordinal)
                && IsEffectGraphNode(node.node_type_id))
            {
                return SkillNodeRuntimeOwner.EffectGraph;
            }
            if (string.Equals(
                node.graph_kind,
                "Plan",
                StringComparison.Ordinal)
                && IsPlanNode(node.node_type_id))
            {
                return IsExecutorOwnedPlanNode(node.node_type_id)
                    ? SkillNodeRuntimeOwner.FamilyExecutor
                    : node.node_type_id == "TriggerProcChanceBonus"
                        ? SkillNodeRuntimeOwner.TriggerDispatcher
                        : SkillNodeRuntimeOwner.ExecutionPlan;
            }

            throw new NotSupportedException(
                $"Reachable {node.graph_kind} node "
                + $"'{node.node_type_id}' has no runtime consumer.");
        }

        private static bool IsEffectGraphNode(string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "ApplyShield":
                case "ApplyStatus":
                case "AttachStatusPayload":
                case "ConditionAnyStatus":
                case "ConditionHealthRatioMax":
                case "ConditionHitCountMin":
                case "ConditionSkillAttribute":
                case "ConditionStatus":
                case "ConditionStatusExpression":
                case "EffectDamage":
                case "EffectExtendStatusDuration":
                case "EffectLifetime":
                case "EffectTarget":
                case "RecastZone":
                case "RequiredSourceStatus":
                case "RuntimeEffectVisual":
                case "StatusActionSpeedBonus":
                case "StatusAttackPowerBonus":
                case "StatusConditionalStatusChanceBonus":
                case "StatusCriticalChanceBonus":
                case "StatusCriticalDamageBonus":
                case "StatusCriticalResistanceBonus":
                case "StatusDamageBonusRate":
                case "StatusDamageTakenBonus":
                case "StatusElementDamageTakenBonus":
                case "StatusElementResistReduction":
                case "StatusFlatElementResistReduction":
                case "StatusModifier":
                case "StatusMoveSpeedBonus":
                case "StatusOutgoingAdditionalDamage":
                case "StatusRuntimeKindFilter":
                case "StatusShieldReceivedBonus":
                case "StatusSpellPowerBonus":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsPlanNode(string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "AdditionalDamage":
                case "AdditionalProjectileBonus":
                case "BeamWidthBonus":
                case "BranchDamage":
                case "BurstDamageRule":
                case "BurstStatusStacksBonus":
                case "ConditionalDamageMultiplier":
                case "ConsecutiveHitDamageBonus":
                case "ConsumeTargetStatusRatioOverride":
                case "CooldownMultiplier":
                case "CooldownRefund":
                case "CooldownRefundBonus":
                case "CooldownReset":
                case "CoreAdditionalDamage":
                case "CoreDamageMultiplier":
                case "CountStatusDamageMultiplier":
                case "CritChanceBonus":
                case "CritDamageBonus":
                case "DamageDelayMultiplier":
                case "DamageMultiplier":
                case "DurationBonus":
                case "DurationMultiplier":
                case "EveryNthHitChainDamage":
                case "ExecuteCritChanceBonus":
                case "ExecuteDamageMultiplier":
                case "FollowUpProjectile":
                case "HitCountCooldownRefund":
                case "HitTargetCountBonus":
                case "KnockbackDistanceMultiplier":
                case "MagazineBonus":
                case "PierceBonus":
                case "RadiusMultiplier":
                case "RedistributeConsumedStatus":
                case "ReloadReducePerHit":
                case "ReloadTimeMultiplier":
                case "RepeatPerTarget":
                case "RequiredSourceStatus":
                case "ShieldAmountMultiplier":
                case "ShotIntervalMultiplier":
                case "StatusActionSpeedBonus":
                case "StatusAilmentResistanceBonus":
                case "StatusAttackPowerBonus":
                case "StatusConditionalDamageTakenBonus":
                case "StatusCriticalDamageTakenBonus":
                case "StatusDamageBonusRate":
                case "StatusDamageTakenBonus":
                case "StatusDurationBonus":
                case "StatusElementDamageTakenBonus":
                case "StatusFilteredDeployment":
                case "StatusMaxStacksBonus":
                case "StatusShieldReceivedBonus":
                case "StatusStackAmountBonus":
                case "StatusStackAmountSet":
                case "TargetHealthRatioCondition":
                case "TargetHealthRatioThresholdBonus":
                case "TargetPredicateDamageMultiplier":
                case "TargetStatusCritBonus":
                case "TargetStatusStackDamage":
                case "TargetStatusStackDamageMultiplier":
                case "TargetStatusStackDamageRateBonus":
                case "ThresholdApplyStatus":
                case "TriggerProcChanceBonus":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsExecutorOwnedPlanNode(
            string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "AdditionalDamage":
                case "BranchDamage":
                case "ConsumeTargetStatusRatioOverride":
                case "CoreAdditionalDamage":
                case "EveryNthHitChainDamage":
                case "HitCountCooldownRefund":
                case "RedistributeConsumedStatus":
                case "ReloadReducePerHit":
                case "StatusAilmentResistanceBonus":
                case "StatusConditionalDamageTakenBonus":
                case "StatusDurationBonus":
                case "StatusFilteredDeployment":
                case "StatusMaxStacksBonus":
                case "StatusStackAmountBonus":
                case "StatusStackAmountSet":
                case "TargetStatusStackDamage":
                case "ThresholdApplyStatus":
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum SkillNodeRuntimeOwner
    {
        ExecutionPlan,
        FamilyExecutor,
        EffectGraph,
        TriggerDispatcher
    }
}
