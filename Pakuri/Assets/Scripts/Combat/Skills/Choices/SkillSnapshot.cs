using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 실행 순간의 능력치, 강화 선택, 노드 결과를 고정해 보관한다.
 */
namespace Pakuri.InGame
{

public sealed class SkillSnapshot
{
	private readonly struct ConditionalDamageRule
	{
		public float DamageMultiplier { get; }

		public string StatusId { get; }

		public int MinStacks { get; }

		public ConditionalDamageRule(float damageMultiplier, string statusId, int minStacks)
		{
			DamageMultiplier = damageMultiplier;
			StatusId = statusId;
			MinStacks = minStacks;
		}
	}

	private readonly struct ConditionalCritChanceRule
	{
		public float CritChanceBonus { get; }

		public string StatusId { get; }

		public int MinStacks { get; }

		public ConditionalCritChanceRule(float critChanceBonus, string statusId, int minStacks)
		{
			CritChanceBonus = critChanceBonus;
			StatusId = statusId;
			MinStacks = minStacks;
		}
	}

	private readonly struct BurstDamageRule
	{
		public int ProjectileIndex { get; }

		public float DamageMultiplier { get; }

		public BurstDamageRule(int projectileIndex, float damageMultiplier)
		{
			ProjectileIndex = projectileIndex;
			DamageMultiplier = damageMultiplier;
		}
	}

	private readonly struct BurstStatusRule
	{
		public int ProjectileIndex { get; }

		public int StacksBonus { get; }

		public BurstStatusRule(int projectileIndex, int stacksBonus)
		{
			ProjectileIndex = projectileIndex;
			StacksBonus = stacksBonus;
		}
	}

	private readonly HashSet<string> activeChoiceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly List<ConditionalDamageRule> conditionalDamageRules = new List<ConditionalDamageRule>();

	private readonly List<ConditionalCritChanceRule> conditionalCritChanceRules = new List<ConditionalCritChanceRule>();

	private readonly List<BurstDamageRule> burstDamageRules = new List<BurstDamageRule>();

	private readonly List<BurstStatusRule> burstStatusRules = new List<BurstStatusRule>();

	private readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	private readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	private readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	private readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	private readonly List<SkillNode> normalizedPlanNodes = new List<SkillNode>();

	public SkillRuntimeData Source { get; }

	public string SkillId { get; }

	public SkillNodePlan Plan { get; private set; }

	public float DamageMultiplier { get; private set; }

	public float ShieldAmountMultiplier { get; private set; }

	public float CooldownMultiplier { get; private set; }

	public float RadiusMultiplier { get; private set; }

	public float DurationMultiplier { get; private set; }

	public float BaseDamageBonus { get; private set; }

	public int MagazineBonus { get; private set; }

	public int AdditionalProjectileBonus { get; private set; }

	public int PierceBonus { get; private set; }

	public float ReloadTimeMultiplier { get; private set; }

	public float ShotIntervalMultiplier { get; private set; }

	public int FollowUpProjectileCount { get; private set; }

	public float FollowUpProjectileDelaySeconds { get; private set; }

	public float FollowUpProjectileDamageMultiplier { get; private set; } = 1f;

	public float RadiusBonus { get; private set; }

	public float BeamWidthBonus { get; private set; }

	public float KnockbackDistanceMultiplier { get; private set; }

	public float DamageDelayMultiplier { get; private set; }

	public float ExecuteHealthRatioBonus { get; private set; }

	public float DurationBonus { get; private set; }

	public float BranchChanceBonus { get; private set; }

	public bool HasBranchChanceSet { get; private set; }

	public float BranchChanceSet { get; private set; }

	public bool HasBranchCount { get; private set; }

	public int BranchCount { get; private set; }

	public bool HasBranchDamageMultiplier { get; private set; }

	public float BranchDamageMultiplier { get; private set; }

	public bool HasBranchSearchRadius { get; private set; }

	public float BranchSearchRadius { get; private set; }

	public int BranchLaunchPeriod { get; private set; }

	public bool HasBranchLaunchChanceSet { get; private set; }

	public float BranchLaunchChanceSet { get; private set; }

	public int HitTargetCountBonus { get; private set; }

	public float CritChanceBonus { get; private set; }

	public float CritDamageBonus { get; private set; }

	public float ExecuteCritChanceBonus { get; private set; }

	public float ConsecutiveHitBonusRate { get; private set; }

	public float ConsecutiveHitMax { get; private set; }

	public float BossDamageMultiplier { get; private set; }

	public float KillCooldownRefundRatioBonus { get; private set; }

	public bool KillResetsCooldown { get; private set; }

	public bool KillResetsCooldownRequiresExecute { get; private set; }

	public string StatusTag { get; private set; }

	public float StatusChanceBonus { get; private set; }

	public bool HasStatusActionSpeedBonus { get; private set; }

	public string StatusActionSpeedBonusStatusId { get; private set; }

	public float StatusActionSpeedBonus { get; private set; }

	public bool HasStatusAttackPowerBonus { get; private set; }

	public float StatusAttackPowerBonus { get; private set; }

	public int StatusStacksBonus { get; private set; }

	public bool HasStatusStacksSet { get; private set; }

	public int StatusStacksSet { get; private set; }

	public bool HasStatusElementDamageTakenBonus { get; private set; }

	public float StatusElementDamageTakenBonus { get; private set; }

	public bool HasStatusCriticalDamageTakenBonus { get; private set; }

	public float StatusCriticalDamageTakenBonus { get; private set; }

	public bool HasStatusAilmentResistanceBonus { get; private set; }

	public float StatusAilmentResistanceBonus { get; private set; }

	public bool HasStatusDamageBonusRate { get; private set; }

	public float StatusDamageBonusRate { get; private set; }

	public bool HasStatusShieldReceivedBonus { get; private set; }

	public float StatusShieldReceivedBonus { get; private set; }

	public bool HasStatusCriticalChanceBonus { get; private set; }

	public float StatusCriticalChanceBonus { get; private set; }

	public bool HasStatusDamageTakenBonus { get; private set; }

	public float StatusDamageTakenBonus { get; private set; }

	public bool HasStatusFlatElementResistReduction { get; private set; }

	public float StatusFlatElementResistReduction { get; private set; }

	public bool HasStatusConditionalDamageTakenBonus { get; private set; }

	public float StatusConditionalDamageTakenBonus { get; private set; }

	public string StatusConditionalSourceStatusId { get; private set; }

	public bool HasOnHitAdditionalDamage { get; private set; }

	public float OnHitAdditionalDamageChance { get; private set; }

	public float OnHitAdditionalDamageMultiplier { get; private set; }

	public DamageAttribute OnHitAdditionalDamageAttribute { get; private set; }

	public string OnHitAdditionalDamageTarget { get; private set; }

	public int OnHitChainHitPeriod { get; private set; }

	public int OnHitChainTargetCount { get; private set; }

	public float OnHitChainSearchRadius { get; private set; }

	public float OnHitChainDamageMultiplier { get; private set; }

	public DamageAttribute OnHitChainDamageAttribute { get; private set; }

	public string ReloadReduceTargetSkillId { get; private set; }

	public float ReloadReduceSecondsPerHit { get; private set; }

	public string CoreHitboxName { get; private set; }

	public bool HasCoreDamageMultiplier { get; private set; }

	public float CoreDamageMultiplier { get; private set; } = 1f;

	public bool HasCoreOnHitAdditionalDamage { get; private set; }

	public float CoreOnHitAdditionalDamageChance { get; private set; }

	public float CoreOnHitAdditionalDamageMultiplier { get; private set; } = 1f;

	public DamageAttribute CoreOnHitAdditionalDamageAttribute { get; private set; }

	public string HitCountCooldownRefundTargetSkillId { get; private set; }

	public int HitCountCooldownRefundMinTargets { get; private set; }

	public float HitCountCooldownRefundRatio { get; private set; }

	public int RepeatCountPerTarget { get; private set; }

	public float RepeatIntervalSeconds { get; private set; }

	public float RepeatDamageMultiplier { get; private set; } = 1f;

	public string ThresholdStatusId { get; private set; }

	public int ThresholdStatusMinStacks { get; private set; }

	public string ThresholdApplyStatusId { get; private set; }

	public float TargetStatusStackDamageMultiplier { get; private set; } = 1f;

	public bool HasConsumeTargetStatusRatioOverride { get; private set; }

	public float ConsumeTargetStatusRatioOverride { get; private set; }

	public bool HasConsumeTargetStatusStacksOverride { get; private set; }

	public int ConsumeTargetStatusStacksOverride { get; private set; }

	public float RedistributeConsumedStatusRatioOnKill { get; private set; }

	public string RedistributeConsumedStatusId { get; private set; }

	public float RedistributeConsumedStatusSearchRadius { get; private set; }

	public int RedistributeConsumedStatusTargetCount { get; private set; }

	public GameObject SkillEffectPrefab { get; private set; }

	public IReadOnlyList<CastConditionOp> CastConditionOps => castConditionOps;

	public IReadOnlyList<DamageModifierOp> DamageModifierOps => damageModifierOps;

	public IReadOnlyList<CritModifierOp> CritModifierOps => critModifierOps;

	public IReadOnlyList<KillActionOp> KillActionOps => killActionOps;

	public IReadOnlyList<SkillNode> NormalizedPlanNodes => normalizedPlanNodes;

	public bool HasBranchBehavior
	{
		get
		{
			if (!(BranchChanceBonus > 0f) && !HasBranchChanceSet && !HasBranchCount && !HasBranchDamageMultiplier && !HasBranchSearchRadius)
			{
				return HasBranchLaunchTrigger;
			}
			return true;
		}
	}

	public bool HasBranchLaunchTrigger
	{
		get
		{
			if (BranchLaunchPeriod > 0)
			{
				return HasBranchLaunchChanceSet;
			}
			return false;
		}
	}

	public bool HasOnHitAdditionalDamageBehavior
	{
		get
		{
			if (!HasOnHitAdditionalDamage)
			{
				return HasOnHitChainDamageBehavior;
			}
			return true;
		}
	}

	public bool HasOnHitChainDamageBehavior
	{
		get
		{
			if (OnHitChainHitPeriod > 0 && OnHitChainTargetCount > 0 && OnHitChainSearchRadius > 0f)
			{
				return OnHitChainDamageMultiplier > 0f;
			}
			return false;
		}
	}

	public bool HasFollowUpProjectile
	{
		get
		{
			if (FollowUpProjectileCount > 0)
			{
				return FollowUpProjectileDamageMultiplier > 0f;
			}
			return false;
		}
	}

	public SkillSnapshot(SkillRuntimeData source)
	{
		Source = source;
		SkillId = ((source != null) ? source.SkillId : string.Empty);
		DamageMultiplier = 1f;
		ShieldAmountMultiplier = 1f;
		CooldownMultiplier = 1f;
		RadiusMultiplier = 1f;
		DurationMultiplier = 1f;
		KnockbackDistanceMultiplier = 1f;
		DamageDelayMultiplier = 1f;
		ReloadTimeMultiplier = 1f;
		ShotIntervalMultiplier = 1f;
		BossDamageMultiplier = 1f;
		BranchDamageMultiplier = 1f;
		OnHitAdditionalDamageMultiplier = 1f;
		OnHitChainDamageMultiplier = 1f;
		SkillEffectPrefab = source?.SkillEffectPrefab;
		AddNormalizedPlanNodes(source?.NormalizedPlanNodes);
		RebuildExecutionPlan();
	}

	public void ApplyChoiceSpec(SkillChoiceRuntimeData spec)
	{
		if (spec == null || !HasNormalizedPlanNodes(spec.Source))
		{
			return;
		}
		ApplyNodeBackedChoiceDefinition(spec.Source);
	}

	public void ApplyDynamicDamageMultiplier(float multiplier)
	{
		DamageMultiplier *= PositiveOrDefault(multiplier, 1f);
	}

	internal SkillSnapshot CopyWithDamageMultiplier(float multiplier)
	{
		SkillSnapshot copy = (SkillSnapshot)MemberwiseClone();
		copy.DamageMultiplier *= Mathf.Max(0f, multiplier);
		return copy;
	}

	private void ApplyNodeBackedChoiceDefinition(SkillChoiceDefinition choice)
	{
		if (choice.SkillEffectPrefab != null)
		{
			SkillEffectPrefab = choice.SkillEffectPrefab;
		}
		SkillNodeDefinition[] array = SkillNodeMapper.FilterSkillNodeDefinitionsForTarget(choice.NormalizedPlanNodes, SkillId);
		SkillChoiceRuntimeData spec = new SkillChoiceRuntimeData
		{
			Source = new SkillChoiceDefinition()
		};
		SkillChoiceCompiler.ApplyNormalizedChoiceCompatibilityNodes(spec, array);
		ApplyNodeBackedChoiceFields(spec);
		SkillNode[] nodes = SkillNodeMapper.MapSkillNodeDefinitions(array);
		AddNormalizedPlanNodes(nodes);
		ApplyPlanActionNodes(nodes);
		RefreshSingleOperationBridges();
		RebuildExecutionPlan();
	}

	private void ApplyNodeBackedChoiceFields(SkillChoiceRuntimeData spec)
	{
		SkillChoiceDefinition source = spec.Source;
		if (source.HasBurstDamageMultiplier && source.BurstDamageMultiplier > 0f && source.HasBurstDamageProjectileIndex)
		{
			burstDamageRules.Add(new BurstDamageRule(source.BurstDamageProjectileIndex, source.BurstDamageMultiplier));
		}
		if (source.HasBurstStatusProjectileIndex && source.BurstStatusStacksBonus != 0)
		{
			burstStatusRules.Add(new BurstStatusRule(source.BurstStatusProjectileIndex, source.BurstStatusStacksBonus));
		}
		if (source.FollowUpProjectileCount > 0)
		{
			FollowUpProjectileCount = source.FollowUpProjectileCount;
			FollowUpProjectileDelaySeconds = Mathf.Max(0f, source.FollowUpProjectileDelaySeconds);
			FollowUpProjectileDamageMultiplier = Mathf.Max(0f, source.FollowUpProjectileDamageMultiplier);
		}
		if (!string.IsNullOrWhiteSpace(source.ThresholdStatusId) && source.ThresholdStatusMinStacks > 0 && !string.IsNullOrWhiteSpace(source.ThresholdApplyStatusId))
		{
			ThresholdStatusId = source.ThresholdStatusId;
			ThresholdStatusMinStacks = source.ThresholdStatusMinStacks;
			ThresholdApplyStatusId = source.ThresholdApplyStatusId;
		}
		if (source.HasTargetStatusStackDamageMultiplier && source.TargetStatusStackDamageMultiplier > 0f)
		{
			TargetStatusStackDamageMultiplier *= PositiveOrDefault(source.TargetStatusStackDamageMultiplier, 1f);
		}
		if (source.HasConsumeTargetStatusRatioOverride)
		{
			HasConsumeTargetStatusRatioOverride = true;
			ConsumeTargetStatusRatioOverride = Mathf.Clamp01(source.ConsumeTargetStatusRatioOverride);
		}
		if (source.RepeatCountPerTarget > 0)
		{
			RepeatCountPerTarget += source.RepeatCountPerTarget;
			RepeatIntervalSeconds = Mathf.Max(RepeatIntervalSeconds, source.RepeatIntervalSeconds);
			if (source.RepeatDamageMultiplier > 0f)
			{
				RepeatDamageMultiplier *= PositiveOrDefault(source.RepeatDamageMultiplier, 1f);
			}
		}
		if (!Mathf.Approximately(source.ConditionalCritChanceBonus, 0f) && !string.IsNullOrWhiteSpace(source.ConditionalCritTargetStatusId) && source.ConditionalCritTargetStatusMinStacks > 0)
		{
			conditionalCritChanceRules.Add(new ConditionalCritChanceRule(source.ConditionalCritChanceBonus, source.ConditionalCritTargetStatusId, source.ConditionalCritTargetStatusMinStacks));
		}
		if (source.RedistributeConsumedStatusRatioOnKill > 0f && !string.IsNullOrWhiteSpace(source.RedistributeConsumedStatusId) && source.RedistributeConsumedStatusSearchRadius > 0f)
		{
			RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(source.RedistributeConsumedStatusRatioOnKill);
			RedistributeConsumedStatusId = source.RedistributeConsumedStatusId;
			RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, source.RedistributeConsumedStatusSearchRadius);
			RedistributeConsumedStatusTargetCount = Mathf.Max(0, source.RedistributeConsumedStatusTargetCount);
		}
	}

	private static bool HasNormalizedPlanNodes(SkillChoiceDefinition choice)
	{
		if (choice != null && choice.NormalizedPlanNodes != null)
		{
			return choice.NormalizedPlanNodes.Length != 0;
		}
		return false;
	}

	private void ApplyPlanActionNodes(IReadOnlyList<SkillNode> nodes)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			SkillActionOp? skillActionOp = ((nodes[i] != null) ? nodes[i].Action : ((SkillActionOp?)null));
			if (skillActionOp.HasValue)
			{
				ApplyPlanAction(skillActionOp.Value);
			}
		}
	}

	private void ApplyPlanAction(SkillActionOp action)
	{
		switch (action.Kind)
		{
		case SkillActionOpKind.DamageMultiplier:
			DamageMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.ShieldAmountMultiplier:
			ShieldAmountMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.CooldownMultiplier:
			CooldownMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.MagazineBonus:
			MagazineBonus += action.IntValue;
			break;
		case SkillActionOpKind.ReloadTimeMultiplier:
			ReloadTimeMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.PierceBonus:
			PierceBonus += action.IntValue;
			break;
		case SkillActionOpKind.RadiusMultiplier:
			RadiusMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.RadiusBonus:
			RadiusBonus += action.FloatValue;
			break;
		case SkillActionOpKind.DurationBonus:
			DurationBonus += action.FloatValue;
			break;
		case SkillActionOpKind.DurationMultiplier:
			DurationMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.DamageDelayMultiplier:
			DamageDelayMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.AdditionalProjectileBonus:
			AdditionalProjectileBonus += action.IntValue;
			break;
		case SkillActionOpKind.ShotIntervalMultiplier:
			ShotIntervalMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
			break;
		case SkillActionOpKind.ConsecutiveHitDamageBonus:
			ConsecutiveHitBonusRate += Mathf.Max(0f, action.FloatValue);
			ConsecutiveHitMax += Mathf.Max(0f, action.SecondaryFloatValue);
			break;
		case SkillActionOpKind.BranchDamage:
			BranchChanceBonus += action.FloatValue;
			if (action.IntValue > 0)
			{
				HasBranchCount = true;
				BranchCount = action.IntValue;
			}
			if (action.SecondaryFloatValue > 0f)
			{
				HasBranchDamageMultiplier = true;
				BranchDamageMultiplier = action.SecondaryFloatValue;
			}
			if (action.ThirdFloatValue > 0f)
			{
				HasBranchSearchRadius = true;
				BranchSearchRadius = action.ThirdFloatValue;
			}
			break;
		case SkillActionOpKind.StatusStackAmountBonus:
			StatusStacksBonus += action.IntValue;
			break;
		case SkillActionOpKind.StatusStackAmountSet:
			HasStatusStacksSet = true;
			StatusStacksSet = Mathf.Max(0, action.IntValue);
			break;
		case SkillActionOpKind.StatusMaxStacksBonus:
			if (!string.IsNullOrWhiteSpace(action.StringValue) && action.IntValue != 0)
			{
				statusMaxStacksBonuses.TryGetValue(action.StringValue, out var value3);
				statusMaxStacksBonuses[action.StringValue] = value3 + action.IntValue;
			}
			break;
		case SkillActionOpKind.ConditionalDamageMultiplier:
			AddConditionalDamageRule(action.FloatValue, action.StringValue, action.IntValue);
			break;
		case SkillActionOpKind.TargetStatusStackDamageRateBonus:
			if (!string.IsNullOrWhiteSpace(action.StringValue) && !Mathf.Approximately(action.FloatValue, 0f))
			{
				targetStatusStackDamageRateBonuses.TryGetValue(action.StringValue, out var value2);
				targetStatusStackDamageRateBonuses[action.StringValue] = value2 + action.FloatValue;
			}
			break;
		case SkillActionOpKind.TriggerProcChanceBonus:
			if (!string.IsNullOrWhiteSpace(action.StringValue) && !Mathf.Approximately(action.FloatValue, 0f))
			{
				triggerProcChanceBonuses.TryGetValue(action.StringValue, out var value);
				triggerProcChanceBonuses[action.StringValue] = value + action.FloatValue;
			}
			break;
		case SkillActionOpKind.HitTargetCountBonus:
			HitTargetCountBonus += action.IntValue;
			break;
		case SkillActionOpKind.StatusActionSpeedBonus:
			ApplyStatusActionSpeedBonus(action.StringValue, action.FloatValue);
			break;
		case SkillActionOpKind.StatusAttackPowerBonus:
			HasStatusAttackPowerBonus = true;
			StatusAttackPowerBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusAilmentResistanceBonus:
			HasStatusAilmentResistanceBonus = true;
			StatusAilmentResistanceBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusDamageBonusRate:
			HasStatusDamageBonusRate = true;
			StatusDamageBonusRate += action.FloatValue;
			break;
		case SkillActionOpKind.StatusShieldReceivedBonus:
			HasStatusShieldReceivedBonus = true;
			StatusShieldReceivedBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusCriticalChanceBonus:
			HasStatusCriticalChanceBonus = true;
			StatusCriticalChanceBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusDamageTakenBonus:
			HasStatusDamageTakenBonus = true;
			StatusDamageTakenBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusFlatElementResistReduction:
			HasStatusFlatElementResistReduction = true;
			StatusFlatElementResistReduction += action.FloatValue;
			break;
		case SkillActionOpKind.StatusDurationBonus:
			ApplyStatusDurationBonus(action.StringValue, action.FloatValue);
			break;
		case SkillActionOpKind.StatusConditionalDamageTakenBonus:
			HasStatusConditionalDamageTakenBonus = true;
			StatusConditionalDamageTakenBonus += action.FloatValue;
			StatusConditionalSourceStatusId = action.StringValue;
			break;
		case SkillActionOpKind.StatusElementDamageTakenBonus:
			HasStatusElementDamageTakenBonus = true;
			StatusElementDamageTakenBonus += action.FloatValue;
			break;
		case SkillActionOpKind.StatusCriticalDamageTakenBonus:
			HasStatusCriticalDamageTakenBonus = true;
			StatusCriticalDamageTakenBonus += action.FloatValue;
			break;
		case SkillActionOpKind.CountStatusDamageMultiplier:
			break;
		}
	}

	private void ApplyStatusActionSpeedBonus(string statusId, float bonus)
	{
		HasStatusActionSpeedBonus = true;
		if (string.IsNullOrWhiteSpace(statusId))
		{
			StatusActionSpeedBonus += bonus;
			return;
		}
		StatusActionSpeedBonusStatusId = statusId;
		statusActionSpeedBonuses[statusId] = (statusActionSpeedBonuses.TryGetValue(statusId, out var value) ? (value + bonus) : bonus);
	}

	private void ApplyStatusDurationBonus(string statusId, float bonus)
	{
		if (!string.IsNullOrWhiteSpace(statusId) && !Mathf.Approximately(bonus, 0f))
		{
			statusDurationBonuses[statusId] = (statusDurationBonuses.TryGetValue(statusId, out var value) ? (value + bonus) : bonus);
		}
	}

	private void AddConditionalDamageRule(float multiplier, string statusId, int minStacks)
	{
		if (!string.IsNullOrWhiteSpace(statusId) && !(multiplier <= 0f))
		{
			conditionalDamageRules.Add(new ConditionalDamageRule(multiplier, statusId, Mathf.Max(1, minStacks)));
		}
	}

	public void AddActiveChoiceId(string choiceId)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			activeChoiceIds.Add(choiceId);
		}
	}

	public bool HasActiveChoice(string choiceId)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			return activeChoiceIds.Contains(choiceId);
		}
		return false;
	}

	public float ResolveStatusDurationBonus(string statusId)
	{
		if (string.IsNullOrWhiteSpace(statusId))
		{
			return 0f;
		}
		if (!statusDurationBonuses.TryGetValue(statusId, out var value))
		{
			return 0f;
		}
		return value;
	}

	public float ResolveStatusActionSpeedBonus(string statusId)
	{
		float num = StatusActionSpeedBonus;
		if (!string.IsNullOrWhiteSpace(statusId) && statusActionSpeedBonuses.TryGetValue(statusId, out var value))
		{
			num += value;
		}
		return num;
	}

	public int ResolveStatusMaxStacksBonus(string statusId)
	{
		if (string.IsNullOrWhiteSpace(statusId))
		{
			return 0;
		}
		if (!statusMaxStacksBonuses.TryGetValue(statusId, out var value))
		{
			return 0;
		}
		return value;
	}

	public float ResolveTargetStatusStackDamageRateBonus(string statusId)
	{
		if (string.IsNullOrWhiteSpace(statusId))
		{
			return 0f;
		}
		if (!targetStatusStackDamageRateBonuses.TryGetValue(statusId, out var value))
		{
			return 0f;
		}
		return value;
	}

	public float ResolveTriggerProcChanceBonus(string triggerId)
	{
		if (string.IsNullOrWhiteSpace(triggerId))
		{
			return 0f;
		}
		if (!triggerProcChanceBonuses.TryGetValue(triggerId, out var value))
		{
			return 0f;
		}
		return value;
	}

	public float ResolveConditionalDamageMultiplier(UnitCombatState target)
	{
		if (target == null || conditionalDamageRules.Count == 0)
		{
			return 1f;
		}
		float num = 1f;
		for (int i = 0; i < conditionalDamageRules.Count; i++)
		{
			ConditionalDamageRule conditionalDamageRule = conditionalDamageRules[i];
			if (HasRequiredStacks(target, conditionalDamageRule.StatusId, conditionalDamageRule.MinStacks))
			{
				num *= PositiveOrDefault(conditionalDamageRule.DamageMultiplier, 1f);
			}
		}
		return num;
	}

	public float ResolveConditionalCritChanceBonus(UnitCombatState target)
	{
		if (target == null || conditionalCritChanceRules.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < conditionalCritChanceRules.Count; i++)
		{
			ConditionalCritChanceRule conditionalCritChanceRule = conditionalCritChanceRules[i];
			if (HasRequiredStacks(target, conditionalCritChanceRule.StatusId, conditionalCritChanceRule.MinStacks))
			{
				num += conditionalCritChanceRule.CritChanceBonus;
			}
		}
		return num;
	}

	public float ResolveBurstDamageMultiplier(int projectileIndex, int burstProjectileCount)
	{
		if (projectileIndex <= 0 || burstDamageRules.Count == 0)
		{
			return 1f;
		}
		float num = 1f;
		for (int i = 0; i < burstDamageRules.Count; i++)
		{
			BurstDamageRule burstDamageRule = burstDamageRules[i];
			if (MatchesBurstProjectileIndex(burstDamageRule.ProjectileIndex, projectileIndex, burstProjectileCount))
			{
				num *= PositiveOrDefault(burstDamageRule.DamageMultiplier, 1f);
			}
		}
		return num;
	}

	public int ResolveBurstStatusStacksBonus(int projectileIndex, int burstProjectileCount)
	{
		if (projectileIndex <= 0 || burstStatusRules.Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < burstStatusRules.Count; i++)
		{
			BurstStatusRule burstStatusRule = burstStatusRules[i];
			if (MatchesBurstProjectileIndex(burstStatusRule.ProjectileIndex, projectileIndex, burstProjectileCount))
			{
				num += burstStatusRule.StacksBonus;
			}
		}
		return num;
	}

	private static bool HasRequiredStacks(UnitCombatState target, string statusId, int minimumStacks)
	{
		if (target == null || minimumStacks <= 0 || string.IsNullOrWhiteSpace(statusId))
		{
			return false;
		}
		if (!StatusEffectLookup.TryParse(statusId, out var kind))
		{
			return false;
		}
		if (kind == StatusEffectKind.Shield)
		{
			if (target.Resources != null)
			{
				return target.Resources.CurrentShield > 0f;
			}
			return false;
		}
		if (target.Statuses != null)
		{
			return target.Statuses.GetStacks(kind) >= minimumStacks;
		}
		return false;
	}

	private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
	{
		if (configuredIndex == 0)
		{
			if (burstProjectileCount > 0)
			{
				return projectileIndex == burstProjectileCount;
			}
			return false;
		}
		return configuredIndex == projectileIndex;
	}

	private static float PositiveOrDefault(float value, float fallback)
	{
		if (!(value > 0f))
		{
			return fallback;
		}
		return value;
	}

	private void RefreshSingleOperationBridges()
	{
		castConditionOps.Clear();
		damageModifierOps.Clear();
		critModifierOps.Clear();
		killActionOps.Clear();
		if (!Mathf.Approximately(ExecuteHealthRatioBonus, 0f))
		{
			castConditionOps.Add(new CastConditionOp(CastConditionOpKind.TargetHealthRatioBonus, ExecuteHealthRatioBonus));
		}
		if (!Mathf.Approximately(BossDamageMultiplier, 1f))
		{
			damageModifierOps.Add(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, BossDamageMultiplier));
		}
		if (!Mathf.Approximately(ExecuteCritChanceBonus, 0f))
		{
			critModifierOps.Add(new CritModifierOp(CritModifierOpKind.ExecuteChanceBonus, ExecuteCritChanceBonus));
		}
		if (KillResetsCooldown)
		{
			killActionOps.Add(new KillActionOp(KillActionOpKind.CooldownReset, 0f, KillResetsCooldownRequiresExecute));
		}
		if (!Mathf.Approximately(KillCooldownRefundRatioBonus, 0f))
		{
			killActionOps.Add(new KillActionOp(KillActionOpKind.CooldownRefundBonus, KillCooldownRefundRatioBonus, requiresExecute: false));
		}
	}

	private void RebuildExecutionPlan()
	{
		Plan = SkillNodeCompiler.Compile(Source, this, normalizedPlanNodes);
	}

	private void AddNormalizedPlanNodes(IReadOnlyList<SkillNode> nodes)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] != null)
			{
				normalizedPlanNodes.Add(nodes[i]);
			}
		}
	}
}

}
