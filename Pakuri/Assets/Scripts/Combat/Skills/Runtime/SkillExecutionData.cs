/*
 * 역할: 확정 스킬 런타임 상태.
 * 책임: 실행 가능한 스킬 값·재사용 대기·대상·전달·배율·시전별 상태를 보관한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// SkillExecutionData가 나타내는 런타임 값을 보관한다.
public class SkillExecutionData
{

	private readonly HashSet<string> activeChoiceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly List<ConditionalDamageActionOp> conditionalDamageActions = new List<ConditionalDamageActionOp>();

	private readonly List<ConditionalCritChanceActionOp> conditionalCritChanceActions = new List<ConditionalCritChanceActionOp>();

	private readonly List<BurstDamageActionOp> burstDamageActions = new List<BurstDamageActionOp>();

	private readonly List<BurstStatusActionOp> burstStatusActions = new List<BurstStatusActionOp>();

	private readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	private readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	private readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	private readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	public SkillDefinition Source { get; }

	public string SkillId { get; }

	public float DamageMultiplier { get; private set; }

	public bool HasRawDamageOverride { get; private set; }

	public float RawDamageOverride { get; private set; }

	public float ShieldAmountMultiplier { get; private set; }

	public float CooldownMultiplier { get; private set; }

	public float RadiusMultiplier { get; private set; }

	public float DurationMultiplier { get; private set; }

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

	public int LineCastRepeatCountBonus { get; private set; }

	public float CritChanceBonus { get; private set; }

	public float CritDamageBonus { get; private set; }

	public float ConsecutiveHitBonusRate { get; private set; }

	public float ConsecutiveHitMax { get; private set; }

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

	public StatusEffectKind StatusConditionalSourceStatusKind { get; private set; }

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

	public StatusEffectKind ThresholdStatusKind { get; private set; }

	public int ThresholdStatusMinStacks { get; private set; }

	public StatusEffectKind ThresholdApplyStatusKind { get; private set; }

	public float TargetStatusStackDamageMultiplier { get; private set; } = 1f;

	public bool HasConsumeTargetStatusRatioOverride { get; private set; }

	public float ConsumeTargetStatusRatioOverride { get; private set; }

	public bool HasConsumeTargetStatusStacksOverride { get; private set; }

	public int ConsumeTargetStatusStacksOverride { get; private set; }

	public float RedistributeConsumedStatusRatioOnKill { get; private set; }

	public StatusEffectKind RedistributeConsumedStatusKind { get; private set; }

	public float RedistributeConsumedStatusSearchRadius { get; private set; }

	public int RedistributeConsumedStatusTargetCount { get; private set; }

	public GameObject SkillEffectPrefab { get; private set; }

	public IReadOnlyList<CastConditionOp> CastConditionOps => castConditionOps;

	public IReadOnlyList<DamageModifierOp> DamageModifierOps => damageModifierOps;

	public IReadOnlyList<CritModifierOp> CritModifierOps => critModifierOps;

	public IReadOnlyList<KillActionOp> KillActionOps => killActionOps;

	internal IReadOnlyList<ConditionalDamageActionOp> ConditionalDamageActions => conditionalDamageActions;

	internal IReadOnlyList<ConditionalCritChanceActionOp> ConditionalCritChanceActions => conditionalCritChanceActions;

	internal IReadOnlyList<BurstDamageActionOp> BurstDamageActions => burstDamageActions;

	internal IReadOnlyList<BurstStatusActionOp> BurstStatusActions => burstStatusActions;

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

	/// SkillExecutionData 인스턴스를 전달된 런타임 입력값으로 초기화한다.
	public SkillExecutionData(SkillDefinition source)
	{
		Source = source;
		SkillId = string.Empty;
		if (source != null)
		{
			SkillId = source.SkillId;
		}
		DamageMultiplier = 1f;
		ShieldAmountMultiplier = 1f;
		CooldownMultiplier = 1f;
		RadiusMultiplier = 1f;
		DurationMultiplier = 1f;
		KnockbackDistanceMultiplier = 1f;
		DamageDelayMultiplier = 1f;
		ReloadTimeMultiplier = 1f;
		ShotIntervalMultiplier = 1f;
		BranchDamageMultiplier = 1f;
		OnHitAdditionalDamageMultiplier = 1f;
		OnHitChainDamageMultiplier = 1f;
		if (source != null)
		{
			SkillEffectPrefab = source.SkillEffectPrefab;
			ApplyNodes(source.Nodes);
		}
	}

	/// 전달된 spec 값을 사용해 ChoiceSpec를 적용한다.
	public void ApplyChoiceSpec(SkillChoice spec)
	{
		if (spec == null || spec.Nodes == null || spec.Nodes.Length == 0)
		{
			return;
		}
		ApplyNodeBackedChoice(spec);
	}

	/// 전달된 multiplier 값을 사용해 DynamicDamageMultiplier를 적용한다.
	public void ApplyDynamicDamageMultiplier(float multiplier)
	{
		DamageMultiplier += PositiveOrDefault(multiplier, 1f) - 1f;
	}

	/// 전달된 rawDamage 값을 사용해 RawDamageOverride를 갱신한다.
	internal void SetRawDamageOverride(float rawDamage)
	{
		HasRawDamageOverride = true;
		RawDamageOverride = Mathf.Max(0f, rawDamage);
	}

	/// 전달된 multiplier 값을 사용해 WithDamageMultiplier를 복사한다.
	internal SkillExecutionData CopyWithDamageMultiplier(float multiplier)
	{
		SkillExecutionData copy = (SkillExecutionData)MemberwiseClone();
		copy.DamageMultiplier += Mathf.Max(0f, multiplier) - 1f;
		return copy;
	}

	/// 전달된 choiceSpec 값을 사용해 NodeBackedChoice를 적용한다.
	private void ApplyNodeBackedChoice(SkillChoice choiceSpec)
	{
		if (choiceSpec.SkillEffectPrefab != null)
		{
			SkillEffectPrefab = choiceSpec.SkillEffectPrefab;
		}
		ApplyNodes(choiceSpec.Nodes, SkillId);
	}

	/// 전달된 런타임 입력값을 사용해 Nodes를 적용한다.
	internal void ApplyNodes(IReadOnlyList<SkillNode> nodes, string targetSkillId = null)
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
				castConditionOps.Add(castCondition.Value);
			}

			DamageModifierOp? damageModifier = nodes[i].GetOperation<DamageModifierOp>();
			if (damageModifier.HasValue)
			{
				damageModifierOps.Add(damageModifier.Value);
			}

			CritModifierOp? critModifier = nodes[i].GetOperation<CritModifierOp>();
			if (critModifier.HasValue)
			{
				critModifierOps.Add(critModifier.Value);
			}

			KillActionOp? killAction = nodes[i].GetOperation<KillActionOp>();
			if (killAction.HasValue)
			{
				killActionOps.Add(killAction.Value);
			}

			SkillActionOp? skillActionOp = nodes[i].GetOperation<SkillActionOp>();
			if (skillActionOp.HasValue)
			{
				ApplyNodeAction(skillActionOp.Value);
			}

			ConsecutiveHitActionOp? consecutiveHitAction = nodes[i].GetOperation<ConsecutiveHitActionOp>();
			if (consecutiveHitAction.HasValue)
			{
				ApplyConsecutiveHitAction(consecutiveHitAction.Value);
			}

			BranchDamageActionOp? branchDamageAction = nodes[i].GetOperation<BranchDamageActionOp>();
			if (branchDamageAction.HasValue)
			{
				ApplyBranchDamageAction(branchDamageAction.Value);
			}

			ConditionalDamageActionOp? conditionalDamageAction = nodes[i].GetOperation<ConditionalDamageActionOp>();
			if (conditionalDamageAction.HasValue)
			{
				ApplyConditionalDamageAction(conditionalDamageAction.Value);
			}

			ConditionalCritChanceActionOp? conditionalCritAction = nodes[i].GetOperation<ConditionalCritChanceActionOp>();
			if (conditionalCritAction.HasValue)
			{
				ApplyConditionalCritChanceAction(conditionalCritAction.Value);
			}

			BurstDamageActionOp? burstDamageAction = nodes[i].GetOperation<BurstDamageActionOp>();
			if (burstDamageAction.HasValue)
			{
				ApplyBurstDamageAction(burstDamageAction.Value);
			}

			BurstStatusActionOp? burstStatusAction = nodes[i].GetOperation<BurstStatusActionOp>();
			if (burstStatusAction.HasValue)
			{
				ApplyBurstStatusAction(burstStatusAction.Value);
			}

			StatusConditionalDamageTakenActionOp? statusDamageTakenAction = nodes[i].GetOperation<StatusConditionalDamageTakenActionOp>();
			if (statusDamageTakenAction.HasValue)
			{
				ApplyStatusConditionalDamageTakenAction(statusDamageTakenAction.Value);
			}

			FollowUpProjectileActionOp? followUpAction = nodes[i].GetOperation<FollowUpProjectileActionOp>();
			if (followUpAction.HasValue)
			{
				ApplyFollowUpProjectileAction(followUpAction.Value);
			}

			ThresholdStatusActionOp? thresholdStatusAction = nodes[i].GetOperation<ThresholdStatusActionOp>();
			if (thresholdStatusAction.HasValue)
			{
				ApplyThresholdStatusAction(thresholdStatusAction.Value);
			}

			RepeatPerTargetActionOp? repeatAction = nodes[i].GetOperation<RepeatPerTargetActionOp>();
			if (repeatAction.HasValue)
			{
				ApplyRepeatPerTargetAction(repeatAction.Value);
			}

			RedistributeConsumedStatusActionOp? redistributeAction = nodes[i].GetOperation<RedistributeConsumedStatusActionOp>();
			if (redistributeAction.HasValue)
			{
				ApplyRedistributeConsumedStatusAction(redistributeAction.Value);
			}

			AdditionalDamageActionOp? additionalDamageAction = nodes[i].GetOperation<AdditionalDamageActionOp>();
			if (additionalDamageAction.HasValue)
			{
				ApplyAdditionalDamageAction(additionalDamageAction.Value);
			}

			CoreDamageActionOp? coreDamageAction = nodes[i].GetOperation<CoreDamageActionOp>();
			if (coreDamageAction.HasValue)
			{
				ApplyCoreDamageAction(coreDamageAction.Value);
			}

			CoreAdditionalDamageActionOp? coreAdditionalDamageAction = nodes[i].GetOperation<CoreAdditionalDamageActionOp>();
			if (coreAdditionalDamageAction.HasValue)
			{
				ApplyCoreAdditionalDamageAction(coreAdditionalDamageAction.Value);
			}

			HitChainDamageActionOp? hitChainAction = nodes[i].GetOperation<HitChainDamageActionOp>();
			if (hitChainAction.HasValue)
			{
				ApplyHitChainDamageAction(hitChainAction.Value);
			}

			HitCountCooldownRefundActionOp? hitCountRefundAction = nodes[i].GetOperation<HitCountCooldownRefundActionOp>();
			if (hitCountRefundAction.HasValue)
			{
				ApplyHitCountCooldownRefundAction(hitCountRefundAction.Value);
			}

			ReloadReducePerHitActionOp? reloadReduceAction = nodes[i].GetOperation<ReloadReducePerHitActionOp>();
			if (reloadReduceAction.HasValue)
			{
				ApplyReloadReducePerHitAction(reloadReduceAction.Value);
			}
		}
	}

	/// 전달된 action 값을 사용해 NodeAction를 적용한다.
	private void ApplyNodeAction(SkillActionOp action)
	{
		switch (action.Kind)
		{
		case SkillActionOpKind.DamageMultiplier:
			DamageMultiplier += PositiveOrDefault(action.Amount, 1f) - 1f;
			break;
		case SkillActionOpKind.ShieldAmountMultiplier:
			ShieldAmountMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.CooldownMultiplier:
			CooldownMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.MagazineBonus:
			MagazineBonus += action.Count;
			break;
		case SkillActionOpKind.ReloadTimeMultiplier:
			ReloadTimeMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.PierceBonus:
			PierceBonus += action.Count;
			break;
		case SkillActionOpKind.RadiusMultiplier:
			RadiusMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.RadiusBonus:
			RadiusBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationBonus:
			DurationBonus += action.Amount;
			break;
		case SkillActionOpKind.DurationMultiplier:
			DurationMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.DamageDelayMultiplier:
			DamageDelayMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.AdditionalProjectileBonus:
			AdditionalProjectileBonus += action.Count;
			break;
		case SkillActionOpKind.ShotIntervalMultiplier:
			ShotIntervalMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.StatusStackAmountBonus:
			StatusStacksBonus += action.Count;
			break;
		case SkillActionOpKind.StatusStackAmountSet:
			HasStatusStacksSet = true;
			StatusStacksSet = Mathf.Max(0, action.Count);
			break;
		case SkillActionOpKind.StatusMaxStacksBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && action.Count != 0)
			{
				statusMaxStacksBonuses.TryGetValue(action.ReferenceId, out var value3);
				statusMaxStacksBonuses[action.ReferenceId] = value3 + action.Count;
			}
			break;
		case SkillActionOpKind.TargetStatusStackDamageRateBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && !Mathf.Approximately(action.Amount, 0f))
			{
				targetStatusStackDamageRateBonuses.TryGetValue(action.ReferenceId, out var value2);
				targetStatusStackDamageRateBonuses[action.ReferenceId] = value2 + action.Amount;
			}
			break;
		case SkillActionOpKind.TriggerProcChanceBonus:
			if (!string.IsNullOrWhiteSpace(action.ReferenceId) && !Mathf.Approximately(action.Amount, 0f))
			{
				triggerProcChanceBonuses.TryGetValue(action.ReferenceId, out var value);
				triggerProcChanceBonuses[action.ReferenceId] = value + action.Amount;
			}
			break;
		case SkillActionOpKind.HitTargetCountBonus:
			HitTargetCountBonus += action.Count;
			break;
		case SkillActionOpKind.LineCastRepeatCountBonus:
			LineCastRepeatCountBonus += action.Count;
			break;
		case SkillActionOpKind.StatusActionSpeedBonus:
			ApplyStatusActionSpeedBonus(action.ReferenceId, action.Amount);
			break;
		case SkillActionOpKind.StatusAttackPowerBonus:
			HasStatusAttackPowerBonus = true;
			StatusAttackPowerBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusAilmentResistanceBonus:
			HasStatusAilmentResistanceBonus = true;
			StatusAilmentResistanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageBonusRate:
			HasStatusDamageBonusRate = true;
			StatusDamageBonusRate += action.Amount;
			break;
		case SkillActionOpKind.StatusShieldReceivedBonus:
			HasStatusShieldReceivedBonus = true;
			StatusShieldReceivedBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalChanceBonus:
			HasStatusCriticalChanceBonus = true;
			StatusCriticalChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusDamageTakenBonus:
			HasStatusDamageTakenBonus = true;
			StatusDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusFlatElementResistReduction:
			HasStatusFlatElementResistReduction = true;
			StatusFlatElementResistReduction += action.Amount;
			break;
		case SkillActionOpKind.StatusDurationBonus:
			ApplyStatusDurationBonus(action.ReferenceId, action.Amount);
			break;
		case SkillActionOpKind.StatusElementDamageTakenBonus:
			HasStatusElementDamageTakenBonus = true;
			StatusElementDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.StatusCriticalDamageTakenBonus:
			HasStatusCriticalDamageTakenBonus = true;
			StatusCriticalDamageTakenBonus += action.Amount;
			break;
		case SkillActionOpKind.CritChanceBonus:
			CritChanceBonus += action.Amount;
			break;
		case SkillActionOpKind.CritDamageBonus:
			CritDamageBonus += action.Amount;
			break;
		case SkillActionOpKind.BeamWidthBonus:
			BeamWidthBonus += action.Amount;
			break;
		case SkillActionOpKind.KnockbackDistanceMultiplier:
			KnockbackDistanceMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.TargetStatusStackDamageMultiplier:
			TargetStatusStackDamageMultiplier *= PositiveOrDefault(action.Amount, 1f);
			break;
		case SkillActionOpKind.ConsumeTargetStatusRatioOverride:
			HasConsumeTargetStatusRatioOverride = true;
			ConsumeTargetStatusRatioOverride = Mathf.Clamp01(action.Amount);
			break;
		}
	}

	/// 전달된 action 값을 사용해 ConsecutiveHitAction를 적용한다.
	private void ApplyConsecutiveHitAction(ConsecutiveHitActionOp action)
	{
		ConsecutiveHitBonusRate += Mathf.Max(0f, action.BonusRate);
		ConsecutiveHitMax += Mathf.Max(0f, action.MaxBonus);
	}

	/// 전달된 action 값을 사용해 BranchDamageAction를 적용한다.
	private void ApplyBranchDamageAction(BranchDamageActionOp action)
	{
		BranchChanceBonus += action.ChanceBonus;
		if (action.BranchCount > 0)
		{
			HasBranchCount = true;
			BranchCount = action.BranchCount;
		}
		if (action.DamageMultiplier > 0f)
		{
			HasBranchDamageMultiplier = true;
			BranchDamageMultiplier = action.DamageMultiplier;
		}
		if (action.SearchRadius > 0f)
		{
			HasBranchSearchRadius = true;
			BranchSearchRadius = action.SearchRadius;
		}
	}

	/// 전달된 action 값을 사용해 ConditionalDamageAction를 적용한다.
	private void ApplyConditionalDamageAction(ConditionalDamageActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& action.DamageMultiplier > 0f)
		{
			conditionalDamageActions.Add(action);
		}
	}

	/// 전달된 action 값을 사용해 ConditionalCritChanceAction를 적용한다.
	private void ApplyConditionalCritChanceAction(ConditionalCritChanceActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& !Mathf.Approximately(action.ChanceBonus, 0f))
		{
			conditionalCritChanceActions.Add(action);
		}
	}

	/// 전달된 action 값을 사용해 BurstDamageAction를 적용한다.
	private void ApplyBurstDamageAction(BurstDamageActionOp action)
	{
		if (action.DamageMultiplier > 0f)
		{
			burstDamageActions.Add(action);
		}
	}

	/// 전달된 action 값을 사용해 BurstStatusAction를 적용한다.
	private void ApplyBurstStatusAction(BurstStatusActionOp action)
	{
		if (action.StacksBonus != 0)
		{
			burstStatusActions.Add(action);
		}
	}

	/// 전달된 action 값을 사용해 StatusConditionalDamageTakenAction를 적용한다.
	private void ApplyStatusConditionalDamageTakenAction(StatusConditionalDamageTakenActionOp action)
	{
		HasStatusConditionalDamageTakenBonus = true;
		StatusConditionalDamageTakenBonus += action.Bonus;
		StatusConditionalSourceStatusKind = action.RequiredSourceStatus;
	}

	/// 전달된 action 값을 사용해 FollowUpProjectileAction를 적용한다.
	private void ApplyFollowUpProjectileAction(FollowUpProjectileActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		FollowUpProjectileCount = action.Count;
		FollowUpProjectileDelaySeconds = Mathf.Max(0f, action.DelaySeconds);
		FollowUpProjectileDamageMultiplier = Mathf.Max(0f, action.DamageMultiplier);
	}

	/// 전달된 action 값을 사용해 ThresholdStatusAction를 적용한다.
	private void ApplyThresholdStatusAction(ThresholdStatusActionOp action)
	{
		if (action.Condition.StatusKind == StatusEffectKind.None
			|| action.Condition.MinimumStacks <= 0
			|| action.AppliedStatus == StatusEffectKind.None)
		{
			return;
		}

		ThresholdStatusKind = action.Condition.StatusKind;
		ThresholdStatusMinStacks = action.Condition.MinimumStacks;
		ThresholdApplyStatusKind = action.AppliedStatus;
	}

	/// 전달된 action 값을 사용해 RepeatPerTargetAction를 적용한다.
	private void ApplyRepeatPerTargetAction(RepeatPerTargetActionOp action)
	{
		if (action.Count <= 0)
		{
			return;
		}

		RepeatCountPerTarget += action.Count;
		RepeatIntervalSeconds = Mathf.Max(RepeatIntervalSeconds, action.IntervalSeconds);
		if (action.DamageMultiplier > 0f)
		{
			RepeatDamageMultiplier *= action.DamageMultiplier;
		}
	}

	/// 전달된 action 값을 사용해 RedistributeConsumedStatusAction를 적용한다.
	private void ApplyRedistributeConsumedStatusAction(RedistributeConsumedStatusActionOp action)
	{
		if (action.Ratio <= 0f || action.StatusKind == StatusEffectKind.None || action.SearchRadius <= 0f)
		{
			return;
		}

		RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(action.Ratio);
		RedistributeConsumedStatusKind = action.StatusKind;
		RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, action.SearchRadius);
		RedistributeConsumedStatusTargetCount = Mathf.Max(0, action.TargetCount);
	}

	/// 전달된 action 값을 사용해 AdditionalDamageAction를 적용한다.
	private void ApplyAdditionalDamageAction(AdditionalDamageActionOp action)
	{
		HasOnHitAdditionalDamage = true;
		OnHitAdditionalDamageChance = action.Chance;
		OnHitAdditionalDamageMultiplier = action.Multiplier;
		OnHitAdditionalDamageAttribute = action.Attribute;
		OnHitAdditionalDamageTarget = action.Target;
	}

	/// 전달된 action 값을 사용해 CoreDamageAction를 적용한다.
	private void ApplyCoreDamageAction(CoreDamageActionOp action)
	{
		CoreHitboxName = action.HitboxName;
		HasCoreDamageMultiplier = true;
		CoreDamageMultiplier *= action.Multiplier;
	}

	/// 전달된 action 값을 사용해 CoreAdditionalDamageAction를 적용한다.
	private void ApplyCoreAdditionalDamageAction(CoreAdditionalDamageActionOp action)
	{
		CoreHitboxName = action.HitboxName;
		HasCoreOnHitAdditionalDamage = true;
		CoreOnHitAdditionalDamageChance = action.Chance;
		CoreOnHitAdditionalDamageMultiplier = action.Multiplier;
		CoreOnHitAdditionalDamageAttribute = action.Attribute;
	}

	/// 전달된 action 값을 사용해 HitChainDamageAction를 적용한다.
	private void ApplyHitChainDamageAction(HitChainDamageActionOp action)
	{
		if (action.HitPeriod <= 0)
		{
			return;
		}

		OnHitChainHitPeriod = action.HitPeriod;
		OnHitChainTargetCount = action.TargetCount;
		OnHitChainSearchRadius = action.SearchRadius;
		OnHitChainDamageMultiplier = action.Multiplier;
		OnHitChainDamageAttribute = action.Attribute;
	}

	/// 전달된 action 값을 사용해 HitCountCooldownRefundAction를 적용한다.
	private void ApplyHitCountCooldownRefundAction(HitCountCooldownRefundActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillId))
		{
			return;
		}

		HitCountCooldownRefundTargetSkillId = action.TargetSkillId;
		HitCountCooldownRefundMinTargets = action.MinimumTargets;
		HitCountCooldownRefundRatio = action.Ratio;
	}

	/// 전달된 action 값을 사용해 ReloadReducePerHitAction를 적용한다.
	private void ApplyReloadReducePerHitAction(ReloadReducePerHitActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillId))
		{
			return;
		}

		ReloadReduceTargetSkillId = action.TargetSkillId;
		ReloadReduceSecondsPerHit += action.SecondsPerHit;
	}

	/// 전달된 런타임 입력값을 사용해 StatusActionSpeedBonus를 적용한다.
	private void ApplyStatusActionSpeedBonus(string statusId, float bonus)
	{
		HasStatusActionSpeedBonus = true;
		if (string.IsNullOrWhiteSpace(statusId))
		{
			StatusActionSpeedBonus += bonus;
			return;
		}
		StatusActionSpeedBonusStatusId = statusId;
		float total = bonus;
		if (statusActionSpeedBonuses.TryGetValue(statusId, out var value))
		{
			total += value;
		}
		statusActionSpeedBonuses[statusId] = total;
	}

	/// 전달된 런타임 입력값을 사용해 StatusDurationBonus를 적용한다.
	private void ApplyStatusDurationBonus(string statusId, float bonus)
	{
		if (!string.IsNullOrWhiteSpace(statusId) && !Mathf.Approximately(bonus, 0f))
		{
			float total = bonus;
			if (statusDurationBonuses.TryGetValue(statusId, out var value))
			{
				total += value;
			}
			statusDurationBonuses[statusId] = total;
		}
	}

	/// 전달된 choiceId 값을 사용해 ActiveChoiceId를 소유한 런타임 상태에 추가한다.
	public void AddActiveChoiceId(string choiceId)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			activeChoiceIds.Add(choiceId);
		}
	}

	/// 전달된 choiceId 값을 사용해 소유한 런타임 상태에 ActiveChoice가 있는지 반환한다.
	public bool HasActiveChoice(string choiceId)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			return activeChoiceIds.Contains(choiceId);
		}
		return false;
	}

	/// 전달된 statusId 값을 사용해 StatusDurationBonus 결과값을 생성해 반환한다.
	public float StatusDurationBonus(string statusId)
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

	/// 전달된 statusId 값을 사용해 StatusActionSpeedBonus를 반환한다.
	public float GetStatusActionSpeedBonus(string statusId)
	{
		float num = StatusActionSpeedBonus;
		if (!string.IsNullOrWhiteSpace(statusId) && statusActionSpeedBonuses.TryGetValue(statusId, out var value))
		{
			num += value;
		}
		return num;
	}

	/// 전달된 statusId 값을 사용해 StatusMaxStacksBonus 결과값을 생성해 반환한다.
	public int StatusMaxStacksBonus(string statusId)
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

	/// 전달된 statusId 값을 사용해 TargetStatusStackDamageRateBonus 결과값을 생성해 반환한다.
	public float TargetStatusStackDamageRateBonus(string statusId)
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

	/// 전달된 triggerId 값을 사용해 TriggerProcChanceBonus 결과값을 생성해 반환한다.
	public float TriggerProcChanceBonus(string triggerId)
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

	/// 전달된 런타임 입력값을 사용해 PositiveOrDefault 결과값을 생성해 반환한다.
	private static float PositiveOrDefault(float value, float fallback)
	{
		if (!(value > 0f))
		{
			return fallback;
		}
		return value;
	}

}

}
