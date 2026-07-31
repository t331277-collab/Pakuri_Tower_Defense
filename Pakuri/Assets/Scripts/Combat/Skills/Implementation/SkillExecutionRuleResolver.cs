/*
 * 역할: 확정 스킬 규칙 계산.
 * 책임: 스킬 정의·학습 선택·패시브 배율·실행 문맥을 결합해 실행 가능한 값을 만든다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// SkillExecutionRuleResolver 처리에 필요한 런타임 규칙 또는 대상을 결정한다.
    internal static class SkillExecutionRuleResolver
    {

        /// 정의된 Node 의미로 기본 실행값을 만든다.
        internal static SkillExecutionData CreateDefinitionSnapshot(SkillDefinition source)
        {
            var snapshot = new SkillExecutionData(source);
            if (source != null)
            {
                ApplyNodes(snapshot, source.Nodes, source.SkillId);
            }
            return snapshot;
        }

        /// 선택 효과의 의미를 기존 실행값에 합성한다.
        internal static void ApplyChoice(SkillExecutionData snapshot, SkillChoice choice)
        {
            if (snapshot == null || choice == null || choice.Nodes == null)
            {
                return;
            }
            if (choice.SkillEffectPrefab != null)
            {
                snapshot.SkillEffectPrefab = choice.SkillEffectPrefab;
            }
            ApplyNodes(snapshot, choice.Nodes, snapshot.SkillId);
        }

        /// 소유자의 학습 상태까지 합성한 실행값을 만든다.
        internal static SkillExecutionData BuildExecutionData(
            UnitCombatState owner,
            SkillExecutionData runtime,
            UnitSpawnManager roster)
        {
            var skill = runtime != null ? runtime.Data : null;
            var snapshot = CreateDefinitionSnapshot(skill);
            if (skill == null || owner == null || owner.Skills == null)
            {
                return snapshot;
            }

            ApplyPassiveBaseModifiers(snapshot, owner, skill);
            ApplyChoices(snapshot, owner.Skills.ChosenEnhancementIds, skill, owner, roster);
            ApplyChoices(snapshot, owner.Skills.ChosenMasterSkillIds, skill, owner, roster);
            return snapshot;
        }

        /// 학습한 지속 효과 중 현재 스킬에 맞는 보정을 합성한다.
        private static void ApplyPassiveBaseModifiers(
            SkillExecutionData snapshot,
            UnitCombatState owner,
            SkillDefinition skill)
        {
            if (snapshot == null || owner?.Skills == null || skill == null)
            {
                return;
            }

            foreach (var passiveId in owner.Skills.LearnedPassiveSkillIds)
            {
                var passiveRuntime = owner.SkillState.FindBySkillId(passiveId);
                var passive = passiveRuntime?.Data as PassiveSkillDefinition;
                if (passive == null || passive.BaseModifierChoices == null)
                {
                    continue;
                }

                for (var i = 0; i < passive.BaseModifierChoices.Length; i++)
                {
                    var choice = passive.BaseModifierChoices[i];
                    if (AppliesToSkill(choice, skill))
                    {
                        ApplyChoice(snapshot, choice);
                    }
                }
            }
        }

        /// 선택된 강화와 마스터 효과를 조건에 맞게 합성한다.
        private static void ApplyChoices(
            SkillExecutionData snapshot,
            IReadOnlyCollection<string> choiceIds,
            SkillDefinition skill,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || choiceIds == null || skill == null || owner?.SkillState == null)
            {
                return;
            }

            foreach (var choiceId in choiceIds)
            {
                var choice = owner.SkillState.FindChoice(choiceId);
                if (AppliesToSkill(choice, skill)
                    && MeetsSourceStatusRequirements(choice, skill.SkillId, owner))
                {
                    snapshot.AddActiveChoiceId(choice.ChoiceId);
                    ApplyChoice(snapshot, choice);
                    ApplyDynamicChoiceRules(snapshot, choice, owner, roster);
                }
            }
        }

        /// 전투 중 대상 수에 따라 선택 효과의 배율을 확정한다.
        internal static void ApplyDynamicChoiceRules(
            SkillExecutionData snapshot,
            SkillChoice choice,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || choice?.Nodes == null || roster == null)
            {
                return;
            }

            for (var i = 0; i < choice.Nodes.Length; i++)
            {
                var node = choice.Nodes[i];
                if (node == null || !string.Equals(node.TargetSkillId, snapshot.SkillId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var action = node.GetOperation<CountStatusDamageActionOp>();
                if (action.HasValue)
                {
                    var count = CountMatchingTargets(owner, roster, action.Value.TargetSide, action.Value.StatusKind);
                    if (action.Value.MaximumCount > 0)
                    {
                        count = Mathf.Min(count, action.Value.MaximumCount);
                    }
                    if (count > 0)
                    {
                        snapshot.ApplyDynamicDamageMultiplier(1f + count * action.Value.AmountPerCount);
                    }
                }
            }
        }

        /// 지정 진영에서 조건을 만족하는 생존 대상 수를 센다.
        private static int CountMatchingTargets(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side,
            StatusEffectKind statusKind)
        {
            if (owner == null || roster == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var entries = CountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].IsAlive && HasStatus(entries[i].Model, statusKind))
                {
                    count++;
                }
            }
            return count;
        }

        /// 시전자와 효과 범위에 맞는 후보 대상을 고른다.
        private static IReadOnlyList<CombatUnitEntry> CountEntries(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side)
        {
            if (owner?.Identity == null || roster == null)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var allies = ownerIsEnemy ? roster.Enemies : roster.Players;
                    var self = FindEntryForModel(owner, allies);
                    return IsSkillTarget(self) ? new[] { self } : Array.Empty<CombatUnitEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Enemies : roster.Players);
                default:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Players : roster.Enemies);
            }
        }

        /// 전투 대상으로 유효한 항목만 남긴다.
        private static IReadOnlyList<CombatUnitEntry> FilterSkillTargets(IReadOnlyList<CombatUnitEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            var filtered = new List<CombatUnitEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                if (IsSkillTarget(entries[i]))
                {
                    filtered.Add(entries[i]);
                }
            }
            return filtered;
        }

        /// 핵심 오브젝트가 아닌 유효 대상인지 확인한다.
        private static bool IsSkillTarget(CombatUnitEntry entry)
        {
            var role = entry?.Model?.Identity?.Role;
            return entry != null && (role == null || role != UnitRole.Nexus);
        }

        /// 모델과 같은 항목을 목록에서 찾는다.
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model,
            IReadOnlyList<CombatUnitEntry> entries)
        {
            if (model == null || entries == null)
            {
                return null;
            }
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && object.ReferenceEquals(entries[i].Model, model))
                {
                    return entries[i];
                }
            }
            return null;
        }

        /// 대상이 요구 상태를 보유하는지 확인한다.
        private static bool HasStatus(UnitCombatState target, StatusEffectKind statusKind)
        {
            if (target == null || statusKind == StatusEffectKind.None)
            {
                return false;
            }
            if (statusKind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }
            return target.Statuses != null && target.Statuses.GetStacks(statusKind) > 0;
        }

        /// 선택 효과가 지정 스킬을 대상으로 하는지 확인한다.
        private static bool AppliesToSkill(SkillChoice choice, SkillDefinition skill)
        {
            if (choice == null || skill == null)
            {
                return false;
            }
            if (choice.Nodes != null && choice.Nodes.Length > 0)
            {
                for (var i = 0; i < choice.Nodes.Length; i++)
                {
                    if (choice.Nodes[i] != null
                        && string.Equals(choice.Nodes[i].TargetSkillId, skill.SkillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            var targetSkillId = string.IsNullOrWhiteSpace(choice.TargetSkillId)
                ? choice.SkillId
                : choice.TargetSkillId;
            return string.Equals(targetSkillId, skill.SkillId, StringComparison.OrdinalIgnoreCase);
        }

	/// Node 의미를 실행값으로 해석해 저장한다.
	internal static void ApplyNodes(SkillExecutionData snapshot, IReadOnlyList<SkillNode> nodes, string targetSkillId = null)
	{

		if (nodes == null || nodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] == null
				|| (!string.IsNullOrWhiteSpace(targetSkillId)
					&& !string.Equals(nodes[i].TargetSkillId, targetSkillId, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}

			CastConditionOp? castCondition = nodes[i].GetOperation<CastConditionOp>();
			if (castCondition.HasValue)
			{
				snapshot.castConditionOps.Add(castCondition.Value);
			}

			DamageModifierOp? damageModifier = nodes[i].GetOperation<DamageModifierOp>();
			if (damageModifier.HasValue)
			{
				snapshot.damageModifierOps.Add(damageModifier.Value);
			}

			CritModifierOp? critModifier = nodes[i].GetOperation<CritModifierOp>();
			if (critModifier.HasValue)
			{
				snapshot.critModifierOps.Add(critModifier.Value);
			}

			KillActionOp? killAction = nodes[i].GetOperation<KillActionOp>();
			if (killAction.HasValue)
			{
				snapshot.killActionOps.Add(killAction.Value);
			}

			SkillCastEffectOp? castEffect = nodes[i].GetOperation<SkillCastEffectOp>();
			if (castEffect.HasValue && castEffect.Value.Effect != null)
			{
				snapshot.castEffects.Add(castEffect.Value.Effect);
			}

			SkillReactionOp? reaction = nodes[i].GetOperation<SkillReactionOp>();
			if (reaction.HasValue && reaction.Value.Reaction != null)
			{
				snapshot.reactions.Add(reaction.Value.Reaction);
			}

			SkillActionOp? skillActionOp = nodes[i].GetOperation<SkillActionOp>();
			if (skillActionOp.HasValue)
			{
				ApplyNodeAction(snapshot, skillActionOp.Value);
			}

			ConsecutiveHitActionOp? consecutiveHitAction = nodes[i].GetOperation<ConsecutiveHitActionOp>();
			if (consecutiveHitAction.HasValue)
			{
				ApplyConsecutiveHitAction(snapshot, consecutiveHitAction.Value);
			}

			BranchDamageActionOp? branchDamageAction = nodes[i].GetOperation<BranchDamageActionOp>();
			if (branchDamageAction.HasValue)
			{
				ApplyBranchDamageAction(snapshot, branchDamageAction.Value);
			}

			ConditionalDamageActionOp? conditionalDamageAction = nodes[i].GetOperation<ConditionalDamageActionOp>();
			if (conditionalDamageAction.HasValue)
			{
				ApplyConditionalDamageAction(snapshot, conditionalDamageAction.Value);
			}

			ConditionalCritChanceActionOp? conditionalCritAction = nodes[i].GetOperation<ConditionalCritChanceActionOp>();
			if (conditionalCritAction.HasValue)
			{
				ApplyConditionalCritChanceAction(snapshot, conditionalCritAction.Value);
			}

			BurstDamageActionOp? burstDamageAction = nodes[i].GetOperation<BurstDamageActionOp>();
			if (burstDamageAction.HasValue)
			{
				ApplyBurstDamageAction(snapshot, burstDamageAction.Value);
			}

			BurstStatusActionOp? burstStatusAction = nodes[i].GetOperation<BurstStatusActionOp>();
			if (burstStatusAction.HasValue)
			{
				ApplyBurstStatusAction(snapshot, burstStatusAction.Value);
			}

			StatusConditionalDamageTakenActionOp? statusDamageTakenAction = nodes[i].GetOperation<StatusConditionalDamageTakenActionOp>();
			if (statusDamageTakenAction.HasValue)
			{
				ApplyStatusConditionalDamageTakenAction(snapshot, statusDamageTakenAction.Value);
			}

			FollowUpProjectileActionOp? followUpAction = nodes[i].GetOperation<FollowUpProjectileActionOp>();
			if (followUpAction.HasValue)
			{
				ApplyFollowUpProjectileAction(snapshot, followUpAction.Value);
			}

			ThresholdStatusActionOp? thresholdStatusAction = nodes[i].GetOperation<ThresholdStatusActionOp>();
			if (thresholdStatusAction.HasValue)
			{
				ApplyThresholdStatusAction(snapshot, thresholdStatusAction.Value);
			}

			RepeatPerTargetActionOp? repeatAction = nodes[i].GetOperation<RepeatPerTargetActionOp>();
			if (repeatAction.HasValue)
			{
				ApplyRepeatPerTargetAction(snapshot, repeatAction.Value);
			}

			RedistributeConsumedStatusActionOp? redistributeAction = nodes[i].GetOperation<RedistributeConsumedStatusActionOp>();
			if (redistributeAction.HasValue)
			{
				ApplyRedistributeConsumedStatusAction(snapshot, redistributeAction.Value);
			}

			AdditionalDamageActionOp? additionalDamageAction = nodes[i].GetOperation<AdditionalDamageActionOp>();
			if (additionalDamageAction.HasValue)
			{
				ApplyAdditionalDamageAction(snapshot, additionalDamageAction.Value);
			}

			CoreDamageActionOp? coreDamageAction = nodes[i].GetOperation<CoreDamageActionOp>();
			if (coreDamageAction.HasValue)
			{
				ApplyCoreDamageAction(snapshot, coreDamageAction.Value);
			}

			CoreAdditionalDamageActionOp? coreAdditionalDamageAction = nodes[i].GetOperation<CoreAdditionalDamageActionOp>();
			if (coreAdditionalDamageAction.HasValue)
			{
				ApplyCoreAdditionalDamageAction(snapshot, coreAdditionalDamageAction.Value);
			}

			HitChainDamageActionOp? hitChainAction = nodes[i].GetOperation<HitChainDamageActionOp>();
			if (hitChainAction.HasValue)
			{
				ApplyHitChainDamageAction(snapshot, hitChainAction.Value);
			}

			HitCountCooldownRefundActionOp? hitCountRefundAction = nodes[i].GetOperation<HitCountCooldownRefundActionOp>();
			if (hitCountRefundAction.HasValue)
			{
				ApplyHitCountCooldownRefundAction(snapshot, hitCountRefundAction.Value);
			}

			ReloadReducePerHitActionOp? reloadReduceAction = nodes[i].GetOperation<ReloadReducePerHitActionOp>();
			if (reloadReduceAction.HasValue)
			{
				ApplyReloadReducePerHitAction(snapshot, reloadReduceAction.Value);
			}
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyNodeAction(SkillExecutionData snapshot, SkillActionOp action)
	{
		switch (action.Kind)
		{
		case SkillActionOpKind.DamageMultiplier:
			snapshot.DamageMultiplier += PositiveOrDefault(action.Amount, 1f) - 1f;
			break;
		case SkillActionOpKind.ShieldAmountMultiplier:
			snapshot.ShieldAmountMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.CooldownMultiplier:
			snapshot.CooldownMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.MagazineBonus:
			snapshot.MagazineBonus += action.Count;
			break;
		case SkillActionOpKind.ReloadTimeMultiplier:
			snapshot.ReloadTimeMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.PierceBonus:
			snapshot.PierceBonus += action.Count;
			break;
		case SkillActionOpKind.RadiusMultiplier:
			snapshot.RadiusMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.RadiusBonus:
			snapshot.RadiusBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationBonus:
			snapshot.DurationBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationMultiplier:
			snapshot.DurationMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.DamageDelayMultiplier:
			snapshot.DamageDelayMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.AdditionalProjectileBonus:
			snapshot.AdditionalProjectileBonus += action.Count;
			break;
		case SkillActionOpKind.ShotIntervalMultiplier:
			snapshot.ShotIntervalMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.StatusStackAmountBonus:
			snapshot.StatusStacksBonus += action.Count;
			break;
		case SkillActionOpKind.StatusStackAmountSet:
			snapshot.HasStatusStacksSet = true;
			snapshot.StatusStacksSet = Mathf.Max(0, action.Count);
			break;
		case SkillActionOpKind.StatusMaxStacksBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && action.Count != 0)
			{
				snapshot.statusMaxStacksBonuses.TryGetValue(action.ReferenceId, out var value3);
				snapshot.statusMaxStacksBonuses[action.ReferenceId] = value3 + action.Count;
			}
			break;
		case SkillActionOpKind.TargetStatusStackDamageRateBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && !Mathf.Approximately(action.Amount, 0f))
			{
				snapshot.targetStatusStackDamageRateBonuses.TryGetValue(action.ReferenceId, out var value2);
				snapshot.targetStatusStackDamageRateBonuses[action.ReferenceId] = value2 + action.Amount;
			}
			break;
		case SkillActionOpKind.TriggerProcChanceBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && !Mathf.Approximately(action.Amount, 0f))
			{
				snapshot.triggerProcChanceBonuses.TryGetValue(action.ReferenceId, out var value);
				snapshot.triggerProcChanceBonuses[action.ReferenceId] = value + action.Amount;
			}
			break;
		case SkillActionOpKind.HitTargetCountBonus:
			snapshot.HitTargetCountBonus += action.Count;
			break;
		case SkillActionOpKind.LineCastRepeatCountBonus:
			snapshot.LineCastRepeatCountBonus += action.Count;
			break;
		case SkillActionOpKind.StatusActionSpeedBonus:
			ApplyStatusActionSpeedBonus(snapshot, action.ReferenceId, action.Amount);
			break;
		case SkillActionOpKind.StatusAttackPowerBonus:
			snapshot.HasStatusAttackPowerBonus = true;
			snapshot.StatusAttackPowerBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusAilmentResistanceBonus:
			snapshot.HasStatusAilmentResistanceBonus = true;
			snapshot.StatusAilmentResistanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageBonusRate:
			snapshot.HasStatusDamageBonusRate = true;
			snapshot.StatusDamageBonusRate += action.Amount;
			break;
		case SkillActionOpKind.StatusShieldReceivedBonus:
			snapshot.HasStatusShieldReceivedBonus = true;
			snapshot.StatusShieldReceivedBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalChanceBonus:
			snapshot.HasStatusCriticalChanceBonus = true;
			snapshot.StatusCriticalChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageTakenBonus:
			snapshot.HasStatusDamageTakenBonus = true;
			snapshot.StatusDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusFlatElementResistReduction:
			snapshot.HasStatusFlatElementResistReduction = true;
			snapshot.StatusFlatElementResistReduction += action.Amount;
			break;
		case SkillActionOpKind.StatusDurationBonus:
			ApplyStatusDurationBonus(snapshot, action.ReferenceId, action.Amount);
			break;
		case SkillActionOpKind.StatusElementDamageTakenBonus:
			snapshot.HasStatusElementDamageTakenBonus = true;
			snapshot.StatusElementDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalDamageTakenBonus:
			snapshot.HasStatusCriticalDamageTakenBonus = true;
			snapshot.StatusCriticalDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.CritChanceBonus:
			snapshot.CritChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.CritDamageBonus:
			snapshot.CritDamageBonus += action.Amount;
			break;
		case SkillActionOpKind.BeamWidthBonus:
			snapshot.BeamWidthBonus += action.Amount;
			break;
		case SkillActionOpKind.KnockbackDistanceMultiplier:
			snapshot.KnockbackDistanceMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.TargetStatusStackDamageMultiplier:
			snapshot.TargetStatusStackDamageMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.ConsumeTargetStatusRatioOverride:
			snapshot.HasConsumeTargetStatusRatioOverride = true;
			snapshot.ConsumeTargetStatusRatioOverride = Mathf.Clamp01(action.Amount);
			break;
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyConsecutiveHitAction(SkillExecutionData snapshot, ConsecutiveHitActionOp action)
	{
		snapshot.ConsecutiveHitBonusRate += Mathf.Max(0f, action.BonusRate);
		snapshot.ConsecutiveHitMax += Mathf.Max(0f, action.MaxBonus);
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyBranchDamageAction(SkillExecutionData snapshot, BranchDamageActionOp action)
	{
		snapshot.BranchChanceBonus += action.ChanceBonus;
		if (action.BranchCount > 0)
		{
			snapshot.HasBranchCount = true;
			snapshot.BranchCount = action.BranchCount;
		}
		if (action.DamageMultiplier > 0f)
		{
			snapshot.HasBranchDamageMultiplier = true;
			snapshot.BranchDamageMultiplier = action.DamageMultiplier;
		}
		if (action.SearchRadius > 0f)
		{
			snapshot.HasBranchSearchRadius = true;
			snapshot.BranchSearchRadius = action.SearchRadius;
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyConditionalDamageAction(SkillExecutionData snapshot, ConditionalDamageActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& action.DamageMultiplier > 0f)
		{
			snapshot.conditionalDamageActions.Add(action);
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyConditionalCritChanceAction(SkillExecutionData snapshot, ConditionalCritChanceActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& !Mathf.Approximately(action.ChanceBonus, 0f))
		{
			snapshot.conditionalCritChanceActions.Add(action);
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyBurstDamageAction(SkillExecutionData snapshot, BurstDamageActionOp action)
	{
		if (action.DamageMultiplier > 0f)
		{
			snapshot.burstDamageActions.Add(action);
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyBurstStatusAction(SkillExecutionData snapshot, BurstStatusActionOp action)
	{
		if (action.StacksBonus != 0)
		{
			snapshot.burstStatusActions.Add(action);
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyStatusConditionalDamageTakenAction(SkillExecutionData snapshot, StatusConditionalDamageTakenActionOp action)
	{
		snapshot.HasStatusConditionalDamageTakenBonus = true;
		snapshot.StatusConditionalDamageTakenBonus += action.Bonus;
		snapshot.StatusConditionalSourceStatusKind = action.RequiredSourceStatus;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyFollowUpProjectileAction(SkillExecutionData snapshot, FollowUpProjectileActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		snapshot.FollowUpProjectileCount = action.Count;
		snapshot.FollowUpProjectileDelaySeconds = Mathf.Max(0f, action.DelaySeconds);
		snapshot.FollowUpProjectileDamageMultiplier = Mathf.Max(0f, action.DamageMultiplier);
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyThresholdStatusAction(SkillExecutionData snapshot, ThresholdStatusActionOp action)
	{
		if (action.Condition.StatusKind == StatusEffectKind.None
			|| action.Condition.MinimumStacks <= 0
			|| action.AppliedStatus == StatusEffectKind.None)
		{
			return;
		}

		snapshot.ThresholdStatusKind = action.Condition.StatusKind;
		snapshot.ThresholdStatusMinStacks = action.Condition.MinimumStacks;
		snapshot.ThresholdApplyStatusKind = action.AppliedStatus;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyRepeatPerTargetAction(SkillExecutionData snapshot, RepeatPerTargetActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		snapshot.RepeatCountPerTarget += action.Count;
		snapshot.RepeatIntervalSeconds = Mathf.Max(snapshot.RepeatIntervalSeconds, action.IntervalSeconds);
		if (action.DamageMultiplier > 0f)
		{
			snapshot.RepeatDamageMultiplier *= action.DamageMultiplier;
		}
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyRedistributeConsumedStatusAction(SkillExecutionData snapshot, RedistributeConsumedStatusActionOp action)
	{
		if (action.Ratio <= 0f || action.StatusKind == StatusEffectKind.None || action.SearchRadius <= 0f)
		{
			return;
		}

		snapshot.RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(action.Ratio);
		snapshot.RedistributeConsumedStatusKind = action.StatusKind;
		snapshot.RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, action.SearchRadius);
		snapshot.RedistributeConsumedStatusTargetCount = Mathf.Max(0, action.TargetCount);
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyAdditionalDamageAction(SkillExecutionData snapshot, AdditionalDamageActionOp action)
	{
		snapshot.HasOnHitAdditionalDamage = true;
		snapshot.OnHitAdditionalDamageChance = action.Chance;
		snapshot.OnHitAdditionalDamageMultiplier = action.Multiplier;
		snapshot.OnHitAdditionalDamageAttribute = action.Attribute;
		snapshot.OnHitAdditionalDamageTarget = action.Target;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyCoreDamageAction(SkillExecutionData snapshot, CoreDamageActionOp action)
	{
		snapshot.CoreHitboxName = action.HitboxName;
		snapshot.HasCoreDamageMultiplier = true;
		snapshot.CoreDamageMultiplier *= action.Multiplier;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyCoreAdditionalDamageAction(SkillExecutionData snapshot, CoreAdditionalDamageActionOp action)
	{
		snapshot.CoreHitboxName = action.HitboxName;
		snapshot.HasCoreOnHitAdditionalDamage = true;
		snapshot.CoreOnHitAdditionalDamageChance = action.Chance;
		snapshot.CoreOnHitAdditionalDamageMultiplier = action.Multiplier;
		snapshot.CoreOnHitAdditionalDamageAttribute = action.Attribute;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyHitChainDamageAction(SkillExecutionData snapshot, HitChainDamageActionOp action)
	{
		if (action.HitPeriod <= 0)
		{
			return;
		}

		snapshot.OnHitChainHitPeriod = action.HitPeriod;
		snapshot.OnHitChainTargetCount = action.TargetCount;
		snapshot.OnHitChainSearchRadius = action.SearchRadius;
		snapshot.OnHitChainDamageMultiplier = action.Multiplier;
		snapshot.OnHitChainDamageAttribute = action.Attribute;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyHitCountCooldownRefundAction(SkillExecutionData snapshot, HitCountCooldownRefundActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillId))
		{
			return;
		}

		snapshot.HitCountCooldownRefundTargetSkillId = action.TargetSkillId;
		snapshot.HitCountCooldownRefundMinTargets = action.MinimumTargets;
		snapshot.HitCountCooldownRefundRatio = action.Ratio;
	}

		/// Node 보정값을 실행 데이터에 반영한다.
		internal static void ApplyReloadReducePerHitAction(SkillExecutionData snapshot, ReloadReducePerHitActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillId))
		{
			return;
		}

		snapshot.ReloadReduceTargetSkillId = action.TargetSkillId;
		snapshot.ReloadReduceSecondsPerHit += action.SecondsPerHit;
	}

	/// 전달된 런타임 입력값을 사용해 snapshot.StatusActionSpeedBonus를 적용한다.
		internal static void ApplyStatusActionSpeedBonus(SkillExecutionData snapshot, string statusId, float bonus)
	{
		snapshot.HasStatusActionSpeedBonus = true;
		if (string.IsNullOrWhiteSpace(statusId))
		{
			snapshot.StatusActionSpeedBonus += bonus;
			return;
		}
		snapshot.StatusActionSpeedBonusStatusId = statusId;
		float total = bonus;
		if (snapshot.statusActionSpeedBonuses.TryGetValue(statusId, out var value))
		{
			total += value;
		}
		snapshot.statusActionSpeedBonuses[statusId] = total;
	}

	/// 전달된 런타임 입력값을 사용해 StatusDurationBonus를 적용한다.
		internal static void ApplyStatusDurationBonus(SkillExecutionData snapshot, string statusId, float bonus)
	{
		if (!string.IsNullOrWhiteSpace(statusId) && !Mathf.Approximately(bonus, 0f))
		{
			float total = bonus;
			if (snapshot.statusDurationBonuses.TryGetValue(statusId, out var value))
			{
				total += value;
			}
			snapshot.statusDurationBonuses[statusId] = total;
		}
	}


        /// 양수 보정값을 유효한 기본값으로 정규화한다.
        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        private static bool applyingHitEnhancement;

        internal static bool ApplyAreaHits(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager roster,
            SkillTargetingSpec targeting,
            Vector2 center,
            float radius,
            bool coverAll,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec status,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData runtime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargets,
            SkillExecutionData executionData)
        {
            if (manager == null || sourceEntry == null || roster == null)
            {
                return false;
            }

            if (!coverAll && radius <= 0f)
            {
                var target = SkillTargeting.FindNearestTarget(sourceEntry, roster, targeting);
                return ApplyResolvedHits(
                    manager,
                    sourceEntry,
                    roster,
                    target != null ? new[] { target } : Array.Empty<CombatUnitEntry>(),
                    1,
                    damage,
                    attribute,
                    status,
                    source,
                    sourceSkillId,
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    executionData);
            }

            var candidates = SkillTargeting.TargetList(sourceEntry, roster, targeting);
            var radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null;
                if (!string.IsNullOrWhiteSpace(unitId) && !hitUnitIds.Add(unitId))
                {
                    continue;
                }
                if (!coverAll
                    && ((Vector2)target.Transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                eligibleTargets.Add(target);
            }

            return ApplyResolvedHits(
                manager,
                sourceEntry,
                roster,
                eligibleTargets,
                maxTargets,
                damage,
                attribute,
                status,
                source,
                sourceSkillId,
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
        }

        internal static bool ApplyResolvedHits(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager roster,
            IReadOnlyList<CombatUnitEntry> eligibleTargets,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec status,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData runtime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
        {
            if (manager == null || eligibleTargets == null || eligibleTargets.Count == 0)
            {
                return false;
            }

            var selectedTargets = new List<CombatUnitEntry>(eligibleTargets);
            if (maxTargets > 0 && maxTargets < selectedTargets.Count)
            {
                for (var i = 0; i < maxTargets; i++)
                {
                    var randomIndex = UnityEngine.Random.Range(i, selectedTargets.Count);
                    (selectedTargets[i], selectedTargets[randomIndex]) =
                        (selectedTargets[randomIndex], selectedTargets[i]);
                }
                selectedTargets.RemoveRange(maxTargets, selectedTargets.Count - maxTargets);
            }

            var routed = false;
            for (var i = 0; i < selectedTargets.Count; i++)
            {
                var target = selectedTargets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                var hitPosition = target.Transform != null
                    ? (Vector2)target.Transform.position
                    : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier = executionData != null
                    ? Mathf.Max(0f, executionData.DamageMultiplier)
                        * ConditionalDamageMultiplier(executionData, target.Model)
                    : 1f;
                var result = manager.ApplyDamage(
                    target.Model,
                    resolvedDamage,
                    attribute,
                    source,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    sourceSkillId,
                    finalDamageMultiplier: finalDamageMultiplier);
                if (!result.IsDead)
                {
                    StatusCombatRules.ApplyStatus(manager, target.Model, status, source);
                }
                ApplyHitEnhancements(
                    manager,
                    runtime != null ? roster : null,
                    runtime,
                    executionData,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        internal static void ResolveProjectileBranch(
            SkillExecutionData data,
            int projectileLaunchIndex,
            out float chance,
            out int count,
            out float damageMultiplier,
            out float searchRadius)
        {
            chance = 0f;
            count = 0;
            damageMultiplier = 1f;
            searchRadius = 0f;
            if (data == null || !data.HasBranchBehavior)
            {
                return;
            }

            chance = data.HasBranchChanceSet
                ? data.BranchChanceSet
                : data.BranchChanceBonus;
            if (data.HasBranchLaunchTrigger
                && projectileLaunchIndex > 0
                && projectileLaunchIndex % data.BranchLaunchPeriod == 0)
            {
                chance = data.BranchLaunchChanceSet;
            }

            count = data.HasBranchCount ? data.BranchCount : chance > 0f ? 1 : 0;
            searchRadius = data.HasBranchSearchRadius ? data.BranchSearchRadius : 4.5f;
            if (chance <= 0f || count <= 0 || searchRadius <= 0f)
            {
                chance = 0f;
                count = 0;
                searchRadius = 0f;
                return;
            }

            chance = Mathf.Clamp01(chance);
            count = Math.Max(1, count);
            damageMultiplier = data.HasBranchDamageMultiplier
                ? Mathf.Max(0f, data.BranchDamageMultiplier)
                : 1f;
            searchRadius = Mathf.Max(0f, searchRadius);
        }

        internal static float ProjectileDestroyBoundaryX(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float lifetime)
        {
            var normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;
            var maxTravelDistance = Mathf.Max(
                40f,
                Mathf.Max(0f, speed) * Mathf.Max(0.1f, lifetime) + 1f);
            return origin.x + normalizedDirection.x * maxTravelDistance;
        }

        /// 적중 공통 후속 효과와 OnHit 생명주기를 한 경로에서 적용한다.
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager,
            UnitSpawnManager roster,
            SkillExecutionData runtime,
            SkillExecutionData skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillId,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager != null && roster != null && source != null && hitTarget != null && hitTarget.Model != null)
            {
                var actionExecutionContext = new SkillExecutionContext(
                    manager,
                    roster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model,
                    publishSkillLifecycleEvents: runtime != null,
                    sourceSkillId: sourceSkillId);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnHit,
                    new SkillActionContext(
                        source,
                        sourceSkillId,
                        hitTarget.Model,
                        hitPosition,
                        primaryBaseDamage,
                        1,
                        skillData,
                        actionExecutionContext));
            }

            if (manager == null
                || roster == null
                || skillData == null
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingHitEnhancement)
            {
                return;
            }

            var hasReloadReduction = !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
                && skillData.ReloadReduceSecondsPerHit > 0f;
            if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
            {
                return;
            }

            var hitIndex = runtime != null
                ? runtime.AdvanceSkillHitCount()
                : 0;

            applyingHitEnhancement = true;
            try
            {
                if (hasReloadReduction && runtime != null && runtime.Owner != null && runtime.Owner.Skills != null)
                {
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillId(skillData.ReloadReduceTargetSkillId);
                    if (reloadSkill != null && reloadSkill.IsReloading)
                    {
                        reloadSkill.ReduceReloadRemaining(skillData.ReloadReduceSecondsPerHit);
                    }
                }

                var targetsHitUnit = string.IsNullOrWhiteSpace(skillData.OnHitAdditionalDamageTarget)
                    || string.Equals(skillData.OnHitAdditionalDamageTarget, "HitTarget", StringComparison.OrdinalIgnoreCase);
                if (skillData.HasOnHitAdditionalDamage
                    && skillData.OnHitAdditionalDamageMultiplier > 0f
                    && targetsHitUnit
                    && hitTarget.IsAlive
                    && UnityEngine.Random.value <= Mathf.Clamp01(skillData.OnHitAdditionalDamageChance))
                {
                    manager.ApplyDamage(
                        hitTarget.Model,
                        primaryBaseDamage,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier: skillData.OnHitAdditionalDamageMultiplier);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ChainTargets(
                        roster,
                        sourceEntry,
                        source,
                        hitTarget,
                        hitPosition,
                        skillData.OnHitChainSearchRadius);
                    var targetCount = Mathf.Min(skillData.OnHitChainTargetCount, chainTargets.Count);
                    for (var i = 0; i < targetCount; i++)
                    {
                        var chainTarget = chainTargets[i];
                        if (chainTarget != null && chainTarget.IsAlive && chainTarget.Model != null)
                        {
                            manager.ApplyDamage(
                                chainTarget.Model,
                                primaryBaseDamage,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true,
                                finalDamageMultiplier: skillData.OnHitChainDamageMultiplier);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /// 전달된 런타임 입력값을 사용해 ConditionalDamageMultiplier 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 ConditionalCritChanceBonus 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 BurstDamageMultiplier 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 BurstStatusStacksBonus 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 MeetsSourceStatusRequirements 조건을 평가하고 결과를 반환한다.
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

                if (!HasSourceStatus(
                    owner,
                    requirement.Value.Condition.StatusKind,
                    requirement.Value.Condition.MinimumStacks))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSourceStatus(
            UnitCombatState owner,
            StatusEffectKind statusKind,
            int minimumStacks)
        {
            if (statusKind == StatusEffectKind.None)
            {
                return true;
            }
            if (statusKind == StatusEffectKind.Shield)
            {
                return owner != null
                    && owner.Resources != null
                    && owner.Resources.CurrentShield > 0f;
            }
            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(statusKind) >= Mathf.Max(1, minimumStacks);
        }

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 RequiredStacks가 있는지 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 MatchesBurstProjectileIndex 조건을 평가하고 결과를 반환한다.
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
