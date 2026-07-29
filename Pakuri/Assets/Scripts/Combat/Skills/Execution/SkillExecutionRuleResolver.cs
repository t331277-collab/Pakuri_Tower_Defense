using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * SkillExecutionData에 보관된 조건부 Node 규칙을 현재 대상·투사체 순번·시전자 상태에 대입한다.
 * Node는 조건값만 보관하고, 실제 전투 상태 조회와 최종 보정 계산은 이 Resolver가 담당한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillExecutionRuleResolver
    {
        /* 대상의 현재 상태 중첩 조건을 만족하는 모든 피해 배율을 곱해 반환한다. */
        internal static float ConditionalDamageMultiplier(
            SkillExecutionData data,
            UnitCombatState target)
        {
            // SkillNode가 보관한 상태 중첩 조건을 현재 대상 상태와 비교하는 부분을 구현.
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

        /* 대상의 현재 상태 중첩 조건을 만족하는 치명타 확률 보너스를 합산한다. */
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

        /* 현재 투사체 순번에 일치하는 연속 발사 피해 배율을 곱해 반환한다. */
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

        /* 현재 투사체 순번에 일치하는 상태 중첩 보너스를 합산한다. */
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

        /* Choice 정의 필드와 Node 조건을 모두 만족해야 해당 강화가 적용된다. */
        internal static bool MeetsSourceStatusRequirements(
            SkillChoice choice,
            string targetSkillId,
            UnitCombatState owner)
        {
            if (choice == null || choice.Source == null)
            {
                return false;
            }

            SkillNode[] nodes = SkillNodeMapper.GetChoiceRuntimeNodes(choice, targetSkillId);
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
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

        /* 보호막을 포함해 대상이 요구 상태의 최소 중첩을 현재 만족하는지 확인한다. */
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
