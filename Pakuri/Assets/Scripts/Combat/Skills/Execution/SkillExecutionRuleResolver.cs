/*
 * 역할: 확정 스킬 규칙 계산.
 * 책임: 스킬 정의·학습 선택·패시브 배율·실행 문맥을 결합해 실행 가능한 값을 만든다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{

    /// <summary><c>SkillExecutionRuleResolver</c> 처리에 필요한 런타임 규칙 또는 대상을 결정한다.</summary>
    internal static class SkillExecutionRuleResolver
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConditionalDamageMultiplier</c> 결과값을 생성해 반환한다.</summary>
        internal static float ConditionalDamageMultiplier(
            SkillExecutionData data,
            UnitCombatState target)
        {

            if (data == null || target == null)
            {
                return 1f;
            }

            IReadOnlyList<ConditionalDamageActionOp> actions = data.ConditionalDamageActions;
            var multiplier = 1f;
            for (var i = 0; i < actions.Count; i++)
            {
                ConditionalDamageActionOp action = actions[i];
                if (HasRequiredStacks(target, action.Condition))
                {
                    multiplier *= action.DamageMultiplier;
                }
            }

            return multiplier;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConditionalCritChanceBonus</c> 결과값을 생성해 반환한다.</summary>
        internal static float ConditionalCritChanceBonus(
            SkillExecutionData data,
            UnitCombatState target)
        {
            if (data == null || target == null)
            {
                return 0f;
            }

            IReadOnlyList<ConditionalCritChanceActionOp> actions = data.ConditionalCritChanceActions;
            var bonus = 0f;
            for (var i = 0; i < actions.Count; i++)
            {
                ConditionalCritChanceActionOp action = actions[i];
                if (HasRequiredStacks(target, action.Condition))
                {
                    bonus += action.ChanceBonus;
                }
            }

            return bonus;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>BurstDamageMultiplier</c> 결과값을 생성해 반환한다.</summary>
        internal static float BurstDamageMultiplier(
            SkillExecutionData data,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (data == null || projectileIndex <= 0)
            {
                return 1f;
            }

            IReadOnlyList<BurstDamageActionOp> actions = data.BurstDamageActions;
            var multiplier = 1f;
            for (var i = 0; i < actions.Count; i++)
            {
                BurstDamageActionOp action = actions[i];
                if (MatchesBurstProjectileIndex(action.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    multiplier *= action.DamageMultiplier;
                }
            }

            return multiplier;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>BurstStatusStacksBonus</c> 결과값을 생성해 반환한다.</summary>
        internal static int BurstStatusStacksBonus(
            SkillExecutionData data,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (data == null || projectileIndex <= 0)
            {
                return 0;
            }

            IReadOnlyList<BurstStatusActionOp> actions = data.BurstStatusActions;
            var bonus = 0;
            for (var i = 0; i < actions.Count; i++)
            {
                BurstStatusActionOp action = actions[i];
                if (MatchesBurstProjectileIndex(action.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    bonus += action.StacksBonus;
                }
            }

            return bonus;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MeetsSourceStatusRequirements</c> 조건을 평가하고 결과를 반환한다.</summary>
        internal static bool MeetsSourceStatusRequirements(
            SkillChoice choice,
            string targetSkillId,
            UnitCombatState owner)
        {
            if (choice == null || choice.Nodes == null)
            {
                return false;
            }

            SkillNode[] nodes = choice.Nodes;
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null
                    || !string.Equals(nodes[i].TargetSkillId, targetSkillId, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                SourceStatusRequirementOp? requirement = nodes[i].GetOperation<SourceStatusRequirementOp>();
                if (!requirement.HasValue)
                {
                    continue;
                }

                if (!SkillRequirement.HasSourceStatus(
                    owner,
                    requirement.Value.Condition.StatusKind,
                    requirement.Value.Condition.MinimumStacks))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 소유한 런타임 상태에 <c>RequiredStacks</c>가 있는지 반환한다.</summary>
        private static bool HasRequiredStacks(UnitCombatState target, StatusStackCondition condition)
        {
            if (target == null || condition.StatusKind == StatusEffectKind.None || condition.MinimumStacks <= 0)
            {
                return false;
            }

            if (condition.StatusKind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }

            return target.Statuses != null
                && target.Statuses.GetStacks(condition.StatusKind) >= condition.MinimumStacks;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesBurstProjectileIndex</c> 조건을 평가하고 결과를 반환한다.</summary>
        private static bool MatchesBurstProjectileIndex(
            int configuredIndex,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex == projectileIndex;
        }
    }
}
