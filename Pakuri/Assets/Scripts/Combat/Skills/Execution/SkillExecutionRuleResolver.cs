using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 조립된 SkillExecutionData와 실행 시점의 대상·투사체 정보를 함께 사용해 조건부 규칙을 평가한다.
 * SkillNode는 규칙 값만 정의하고 SkillExecutionData는 규칙 목록을 조립하므로, 전투 상태를 읽는 판정은
 * 이 클래스에만 둔다. 이를 통해 실행 스냅샷이 전투 대상 조회까지 책임지는 구조를 막는다.
 */
namespace Pakuri.InGame
{
    internal static class SkillExecutionRuleResolver
    {
        internal static float ResolveConditionalDamageMultiplier(
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

        internal static float ResolveConditionalCritChanceBonus(
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

        internal static float ResolveBurstDamageMultiplier(
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

        internal static int ResolveBurstStatusStacksBonus(
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

        /* Choice 정의 필드와 Node 조건을 모두 만족해야 해당 강화가 적용된다. */
        internal static bool MeetsSourceStatusRequirements(
            SkillChoice choice,
            string targetSkillId,
            UnitCombatState owner)
        {
            if (choice == null || choice.Source == null
                || !SkillRequirement.MeetsSourceStatus(choice.Source, owner))
            {
                return false;
            }

            SkillNode[] nodes = SkillNodeMapper.ResolveChoiceRuntimePlan(choice, targetSkillId).Nodes;
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null || !nodes[i].SourceStatusRequirement.HasValue)
                {
                    continue;
                }

                SourceStatusRequirementOp requirement = nodes[i].SourceStatusRequirement.Value;
                if (!SkillRequirement.HasSourceStatus(
                    owner,
                    requirement.Condition.StatusKind,
                    requirement.Condition.MinimumStacks))
                {
                    return false;
                }
            }

            return true;
        }

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

        /* 설정값 0은 기존 데이터 계약대로 연속 발사의 마지막 투사체를 의미한다. */
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
