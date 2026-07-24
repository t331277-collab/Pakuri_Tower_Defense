using System;
using Pakuri.NewCore.Definitions.Choices;

/* 선택 노드의 handler와 node_type을 런타임 동작 및 책임 영역으로 분류한다. */
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
        /* handler_id 접두어와 포함 문자열을 기준으로 노드 동작 분류를 반환한다. */
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

        /* 그래프 종류와 노드 타입을 기준으로 실제 소비 런타임을 결정한다. */
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

        /* 노드 타입이 효과 그래프에서 처리되는 타입인지 확인한다. */
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

        /* 노드 타입이 실행 계획에서 사용할 수 있는 타입인지 확인한다. */
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

        /* 실행 계획 노드 중 스킬 계열 실행기가 직접 처리할 타입인지 확인한다. */
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
