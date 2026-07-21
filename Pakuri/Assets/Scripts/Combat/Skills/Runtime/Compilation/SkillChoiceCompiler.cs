using System;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * Enhancement와 Master 선택지를 전투용 데이터와 실행 노드로 변환한다.
 * 스킬 전체 변환을 조율하는 SkillRuntimeCompiler와 달리 선택지 변환과 호환 값 적용만 담당한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillChoiceCompiler
    {
	internal static SkillChoiceRuntimeData[] Compile(SkillChoiceDefinition[] source)
	{
		SkillChoiceRuntimeData[] array = new SkillChoiceRuntimeData[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			SkillChoiceDefinition skillChoiceDefinition = source[i];
			array[i] = new SkillChoiceRuntimeData
			{
				Source = skillChoiceDefinition,
				PlanNodes = SkillNodeMapper.MapSkillNodeDefinitions(skillChoiceDefinition.NormalizedPlanNodes)
			};
		}
		return array;
	}

	internal static void ApplyNormalizedChoiceCompatibilityNodes(SkillChoiceRuntimeData spec, SkillNodeDefinition[] nodes)
	{
		if (spec == null || nodes == null || nodes.Length == 0)
		{
			return;
		}
		foreach (SkillNodeDefinition skillNodeDefinition in nodes)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && RequiresChoiceSpecCompatibility(skillNodeDefinition.HandlerId))
			{
				ApplyNormalizedChoiceNode(spec, skillNodeDefinition);
			}
		}
	}

	private static bool RequiresChoiceSpecCompatibility(string handlerId)
	{
		if (!string.Equals(handlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(handlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static void ApplyNormalizedChoiceNode(SkillChoiceRuntimeData spec, SkillNodeDefinition node)
	{
		SkillChoiceDefinition source = spec.Source;
		string a = node.HandlerId ?? string.Empty;
		if (string.Equals(a, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasDamageMultiplier = true;
			source.DamageMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasShieldAmountMultiplier = true;
			spec.ShieldAmountMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.CountStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.CountTargetSide = SkillNodeMapper.GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies);
			source.DamageMultiplierPerCount += SkillNodeMapper.GetFloatParam(node, "amount_per_count", 0f);
			source.CountMax = SkillNodeMapper.GetIntParam(node, "max_count", source.CountMax);
		}
		else if (string.Equals(a, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasCooldownMultiplier = true;
			source.CooldownMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.CritChanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "CritDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.CritDamageBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasMagazineBonus = true;
			source.MagazineBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasReloadTimeMultiplier = true;
			source.ReloadTimeMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.PierceBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HitTargetCountBonus += SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasRadiusMultiplier = true;
			source.RadiusMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.RadiusBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.BeamWidthBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasKnockbackDistanceMultiplier = true;
			source.KnockbackDistanceMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase))
		{
			source.ReloadReduceTargetSkillId = SkillNodeMapper.GetParam(node, "target_skill_id");
			source.ReloadReduceSecondsPerHit += SkillNodeMapper.GetFloatParam(node, "seconds_per_hit", 0f);
		}
		else if (string.Equals(a, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.CoreHitboxName = SkillNodeMapper.GetParam(node, "hitbox_name");
			source.HasCoreDamageMultiplier = true;
			source.CoreDamageMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.CoreHitboxName = SkillNodeMapper.GetParam(node, "hitbox_name");
			source.HasCoreOnHitAdditionalDamage = true;
			source.CoreOnHitAdditionalDamageChance = SkillNodeMapper.GetFloatParam(node, "chance", 1f);
			source.CoreOnHitAdditionalDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.CoreOnHitAdditionalDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
		}
		else if (string.Equals(a, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			source.HitCountCooldownRefundTargetSkillId = SkillNodeMapper.GetParam(node, "target_skill_id");
			source.HitCountCooldownRefundMinTargets = SkillNodeMapper.GetIntParam(node, "min_targets", 0);
			source.HitCountCooldownRefundRatio = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
		}
		else if (string.Equals(a, "DurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.DurationBonus += SkillNodeMapper.GetFloatParam(node, "bonus_seconds", 0f);
		}
		else if (string.Equals(a, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasDamageDelayMultiplier = true;
			source.DamageDelayMultiplier *= SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.ConsecutiveHitBonusRate += SkillNodeMapper.GetFloatParam(node, "bonus_rate", 0f);
			source.ConsecutiveHitMax += SkillNodeMapper.GetFloatParam(node, "max_bonus", 0f);
		}
		else if (string.Equals(a, "BurstDamageRule", StringComparison.OrdinalIgnoreCase))
		{
			source.HasBurstDamageProjectileIndex = true;
			source.BurstDamageProjectileIndex = SkillNodeMapper.GetIntParam(node, "projectile_index", 0);
			source.HasBurstDamageMultiplier = true;
			source.BurstDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase))
		{
			source.FollowUpProjectileCount = SkillNodeMapper.GetIntParam(node, "count", 0);
			source.FollowUpProjectileDelaySeconds = SkillNodeMapper.GetFloatParam(node, "delay_seconds", 0f);
			source.FollowUpProjectileDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "damage_multiplier", 1f);
		}
		else if (string.Equals(a, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase))
		{
			source.ThresholdStatusId = SkillNodeMapper.GetParam(node, "source_status_id");
			source.ThresholdStatusMinStacks = SkillNodeMapper.GetIntParam(node, "min_stacks", 0);
			source.ThresholdApplyStatusId = SkillNodeMapper.GetParam(node, "apply_status_id");
		}
		else if (string.Equals(a, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			source.HasTargetStatusStackDamageMultiplier = true;
			source.TargetStatusStackDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
		}
		else if (string.Equals(a, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase))
		{
			source.HasConsumeTargetStatusRatioOverride = true;
			source.ConsumeTargetStatusRatioOverride = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
		}
		else if (string.Equals(a, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasBurstStatusProjectileIndex = true;
			source.BurstStatusProjectileIndex = SkillNodeMapper.GetIntParam(node, "projectile_index", 0);
			source.BurstStatusStacksBonus = SkillNodeMapper.GetIntParam(node, "bonus", 0);
		}
		else if (string.Equals(a, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusActionSpeedBonus = true;
			spec.StatusActionSpeedBonusStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.StatusActionSpeedBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusAttackPowerBonus = true;
			source.StatusAttackPowerBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusAilmentResistanceBonus = true;
			source.StatusAilmentResistanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusDamageBonusRate = true;
			spec.StatusDamageBonusRate += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusShieldReceivedBonus = true;
			spec.StatusShieldReceivedBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusCriticalChanceBonus = true;
			spec.StatusCriticalChanceBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusDamageTakenBonus = true;
			spec.StatusDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			spec.HasStatusFlatElementResistReduction = true;
			spec.StatusFlatElementResistReduction += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.StatusDurationBonusStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.StatusDurationBonus += SkillNodeMapper.GetFloatParam(node, "bonus_seconds", 0f);
		}
		else if (string.Equals(a, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusConditionalDamageTakenBonus = true;
			source.StatusConditionalSourceStatusId = SkillNodeMapper.GetParam(node, "source_status_id");
			source.StatusConditionalDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusElementDamageTakenBonus = true;
			source.StatusElementDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.HasStatusCriticalDamageTakenBonus = true;
			source.StatusCriticalDamageTakenBonus += SkillNodeMapper.GetFloatParam(node, "bonus", 0f);
		}
		else if (string.Equals(a, "AdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.HasOnHitAdditionalDamage = true;
			source.OnHitAdditionalDamageChance = SkillNodeMapper.GetFloatParam(node, "chance", 1f);
			source.OnHitAdditionalDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.OnHitAdditionalDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
			string param = SkillNodeMapper.GetParam(node, "target");
			source.OnHitAdditionalDamageTarget = (string.IsNullOrWhiteSpace(param) ? SkillNodeMapper.GetParam(node, "target_side") : param);
		}
		else if (string.Equals(a, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase))
		{
			source.OnHitChainHitPeriod = SkillNodeMapper.GetIntParam(node, "hit_count", 0);
			source.OnHitChainTargetCount = SkillNodeMapper.GetIntParam(node, "max_targets", source.OnHitChainTargetCount);
			source.OnHitChainSearchRadius = SkillNodeMapper.GetFloatParam(node, "radius", source.OnHitChainSearchRadius);
			source.OnHitChainDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "multiplier", 1f);
			source.OnHitChainDamageAttribute = SkillNodeMapper.GetEnumParam(node, "attribute", DamageAttribute.Physical);
		}
		else if (string.Equals(a, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase))
		{
			source.RepeatCountPerTarget = SkillNodeMapper.GetIntParam(node, "repeat_count", 0);
			source.RepeatIntervalSeconds = SkillNodeMapper.GetFloatParam(node, "repeat_interval_seconds", 0f);
			source.RepeatDamageMultiplier = SkillNodeMapper.GetFloatParam(node, "repeat_damage_multiplier", 1f);
		}
		else if (string.Equals(a, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			source.ConditionalCritChanceBonus += SkillNodeMapper.GetFloatParam(node, "crit_chance_bonus", 0f);
			source.ConditionalCritTargetStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.ConditionalCritTargetStatusMinStacks = SkillNodeMapper.GetIntParam(node, "min_stacks", 0);
		}
		else if (string.Equals(a, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			source.RedistributeConsumedStatusRatioOnKill = SkillNodeMapper.GetFloatParam(node, "ratio", 0f);
			source.RedistributeConsumedStatusId = SkillNodeMapper.GetParam(node, "status_id");
			source.RedistributeConsumedStatusSearchRadius = SkillNodeMapper.GetFloatParam(node, "radius", 0f);
			source.RedistributeConsumedStatusTargetCount = SkillNodeMapper.GetIntParam(node, "target_count", 0);
		}
	}
    }
}
