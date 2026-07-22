using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 실행에 사용할 능력치, 강화 선택, 노드 결과를 보관한다.
 */
namespace Pakuri.InGame
{

/*
 * 원본 스킬과 선택한 강화 노드를 합쳐 한 번의 스킬 실행에 사용할 값을 만든다.
 * 실행기는 이 객체에 저장된 최종 수치와 선택한 SkillNode를 읽어 같은 강화 결과를 사용한다.
 */
public class SkillExecutionData
{
	/*
	 * 적용한 선택지와 상태, Trigger별 누적 보너스를 이름으로 관리한다.
	 */
	private readonly HashSet<string> activeChoiceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	/*
	 * 대상 조건과 연속 발사 순서에 따라 실행 중 계산할 규칙을 보관한다.
	 */
	private readonly List<ConditionalDamageActionOp> conditionalDamageActions = new List<ConditionalDamageActionOp>();

	private readonly List<ConditionalCritChanceActionOp> conditionalCritChanceActions = new List<ConditionalCritChanceActionOp>();

	private readonly List<BurstDamageActionOp> burstDamageActions = new List<BurstDamageActionOp>();

	private readonly List<BurstStatusActionOp> burstStatusActions = new List<BurstStatusActionOp>();

	/*
	 * 선택지 값에서 만든 조건, 보정, 처치 행동과 정규화된 노드를 보관한다.
	 */
	private readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	private readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	private readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	private readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	/*
	 * 강화 수치를 적용할 원본 스킬을 나타낸다.
	 */
	public SkillDefinition Source { get; }

	public string SkillId { get; }

	/*
	 * 피해, 보호막, 재사용 대기시간과 투사체에 적용할 기본 강화 수치를 보관한다.
	 */
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

	/*
	 * 처형, 분기 공격, 치명타와 처치 후 행동에 필요한 값을 보관한다.
	 */
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

	public float ConsecutiveHitBonusRate { get; private set; }

	public float ConsecutiveHitMax { get; private set; }

	/*
	 * 상태 효과의 적용 확률, 중첩과 전투 능력치 보너스를 보관한다.
	 */
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

	/*
	 * 적중 시 추가 피해와 연쇄 공격에 필요한 값을 보관한다.
	 */
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

	/*
	 * 재장전·핵심 부위·적중 횟수에 연결되는 특수 강화 값을 보관한다.
	 */
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

	/*
	 * 대상별 반복 공격과 상태 임계치·소모·재분배에 필요한 값을 보관한다.
	 */
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

	/*
	 * 최종 이펙트와 실행 계획을 구성하는 읽기 전용 목록을 제공한다.
	 */
	public GameObject SkillEffectPrefab { get; private set; }

	public IReadOnlyList<CastConditionOp> CastConditionOps => castConditionOps;

	public IReadOnlyList<DamageModifierOp> DamageModifierOps => damageModifierOps;

	public IReadOnlyList<CritModifierOp> CritModifierOps => critModifierOps;

	public IReadOnlyList<KillActionOp> KillActionOps => killActionOps;

	internal IReadOnlyList<ConditionalDamageActionOp> ConditionalDamageActions => conditionalDamageActions;

	internal IReadOnlyList<ConditionalCritChanceActionOp> ConditionalCritChanceActions => conditionalCritChanceActions;

	internal IReadOnlyList<BurstDamageActionOp> BurstDamageActions => burstDamageActions;

	internal IReadOnlyList<BurstStatusActionOp> BurstStatusActions => burstStatusActions;

	/*
	 * 분기 공격에 필요한 확률, 횟수, 피해 또는 발사 주기가 하나라도 있는지 확인한다.
	 */
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

	/*
	 * 분기 공격을 주기적으로 발사할 조건이 완성되었는지 확인한다.
	 */
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

	/*
	 * 단일 추가 피해 또는 연쇄 추가 피해를 실행할 수 있는지 확인한다.
	 */
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

	/*
	 * 연쇄 공격에 필요한 주기, 대상 수, 탐색 범위와 피해 배율이 있는지 확인한다.
	 */
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

	/*
	 * 후속 투사체의 개수와 피해 배율이 실행 가능한 값인지 확인한다.
	 */
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

	/*
	 * 원본 스킬의 식별자, 기본 배율, 이펙트와 노드를 사용해 최초 실행 계획을 만든다.
	 */
	public SkillExecutionData(SkillDefinition source /* 복사하거나 변환할 스킬 실행 데이터 */)
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
			ApplyPlanNodes(source.NormalizedPlanNodes);
		}
	}

	/*
	 * 선택지의 정규화된 노드를 현재 수치와 실행 계획에 반영한다.
	 */
	public void ApplyChoiceSpec(SkillChoice spec /* 처리에 사용할 설정 */)
	{
		if (spec == null || !HasNormalizedPlanNodes(spec.Source))
		{
			return;
		}
		ApplyNodeBackedChoice(spec);
	}

	/*
	 * 전투 중 전달된 피해 배율을 현재 피해 배율에 추가로 곱한다.
	 */
	public void ApplyDynamicDamageMultiplier(float multiplier /* 값에 곱할 배율 */)
	{
		DamageMultiplier *= PositiveOrDefault(multiplier, 1f);
	}

	/*
	 * 현재 실행 데이터를 복사하고 복사본에만 별도 피해 배율을 적용한다.
	 */
	internal SkillExecutionData CopyWithDamageMultiplier(float multiplier /* 값에 곱할 배율 */)
	{
		SkillExecutionData copy = (SkillExecutionData)MemberwiseClone();
		copy.DamageMultiplier *= Mathf.Max(0f, multiplier);
		return copy;
	}

	/*
	 * 선택지 노드를 현재 스킬 대상으로 한정한 뒤 필드와 행동 노드에 반영한다.
	 */
	private void ApplyNodeBackedChoice(SkillChoice choiceSpec /* 적용하거나 검사할 스킬 선택지 */)
	{
		SkillChoiceDefinition choice = choiceSpec.Source;
		if (choice.SkillEffectPrefab != null)
		{
			SkillEffectPrefab = choice.SkillEffectPrefab;
		}
		SkillChoiceRuntimePlan plan = SkillNodeMapper.ResolveChoiceRuntimePlan(choiceSpec, SkillId);
		SkillNode[] nodes = plan.Nodes;
		ApplyPlanNodes(nodes);
	}

	/*
	 * 선택지에 적용할 정규화 노드가 하나 이상 있는지 확인한다.
	 */
	private static bool HasNormalizedPlanNodes(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */)
	{
		if (choice != null && choice.NormalizedPlanNodes != null)
		{
			return choice.NormalizedPlanNodes.Length != 0;
		}
		return false;
	}

	/*
	 * 선택지 노드의 단순 행동과 복합 행동을 현재 실행 데이터에 적용한다.
	 */
	private void ApplyPlanNodes(IReadOnlyList<SkillNode> nodes /* 노드 목록 */)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] == null)
			{
				continue;
			}

			/*
			 * SingleSkillRules가 시전·대상·처치 시점에 평가해야 하는 규칙은 여기서 계산 결과로
			 * 평탄화하지 않고 Op 목록에 보존한다. 이전 코드는 이 네 payload를 건너뛰어
			 * 정규화 노드가 실제 Single 규칙까지 전달되지 않았다.
			 */
			CastConditionOp? castCondition = nodes[i].CastCondition;
			if (castCondition.HasValue)
			{
				castConditionOps.Add(castCondition.Value);
			}

			DamageModifierOp? damageModifier = nodes[i].DamageModifier;
			if (damageModifier.HasValue)
			{
				damageModifierOps.Add(damageModifier.Value);
			}

			CritModifierOp? critModifier = nodes[i].CritModifier;
			if (critModifier.HasValue)
			{
				critModifierOps.Add(critModifier.Value);
			}

			KillActionOp? killAction = nodes[i].KillAction;
			if (killAction.HasValue)
			{
				killActionOps.Add(killAction.Value);
			}

			SkillActionOp? skillActionOp = nodes[i].Action;
			if (skillActionOp.HasValue)
			{
				ApplyPlanAction(skillActionOp.Value);
			}

			ConsecutiveHitActionOp? consecutiveHitAction = nodes[i].ConsecutiveHitAction;
			if (consecutiveHitAction.HasValue)
			{
				ApplyConsecutiveHitAction(consecutiveHitAction.Value);
			}

			BranchDamageActionOp? branchDamageAction = nodes[i].BranchDamageAction;
			if (branchDamageAction.HasValue)
			{
				ApplyBranchDamageAction(branchDamageAction.Value);
			}

			ConditionalDamageActionOp? conditionalDamageAction = nodes[i].ConditionalDamageAction;
			if (conditionalDamageAction.HasValue)
			{
				ApplyConditionalDamageAction(conditionalDamageAction.Value);
			}

			ConditionalCritChanceActionOp? conditionalCritAction = nodes[i].ConditionalCritChanceAction;
			if (conditionalCritAction.HasValue)
			{
				ApplyConditionalCritChanceAction(conditionalCritAction.Value);
			}

			BurstDamageActionOp? burstDamageAction = nodes[i].BurstDamageAction;
			if (burstDamageAction.HasValue)
			{
				ApplyBurstDamageAction(burstDamageAction.Value);
			}

			BurstStatusActionOp? burstStatusAction = nodes[i].BurstStatusAction;
			if (burstStatusAction.HasValue)
			{
				ApplyBurstStatusAction(burstStatusAction.Value);
			}

			StatusConditionalDamageTakenActionOp? statusDamageTakenAction = nodes[i].StatusConditionalDamageTakenAction;
			if (statusDamageTakenAction.HasValue)
			{
				ApplyStatusConditionalDamageTakenAction(statusDamageTakenAction.Value);
			}

			FollowUpProjectileActionOp? followUpAction = nodes[i].FollowUpProjectileAction;
			if (followUpAction.HasValue)
			{
				ApplyFollowUpProjectileAction(followUpAction.Value);
			}

			ThresholdStatusActionOp? thresholdStatusAction = nodes[i].ThresholdStatusAction;
			if (thresholdStatusAction.HasValue)
			{
				ApplyThresholdStatusAction(thresholdStatusAction.Value);
			}

			RepeatPerTargetActionOp? repeatAction = nodes[i].RepeatPerTargetAction;
			if (repeatAction.HasValue)
			{
				ApplyRepeatPerTargetAction(repeatAction.Value);
			}

			RedistributeConsumedStatusActionOp? redistributeAction = nodes[i].RedistributeConsumedStatusAction;
			if (redistributeAction.HasValue)
			{
				ApplyRedistributeConsumedStatusAction(redistributeAction.Value);
			}

			AdditionalDamageActionOp? additionalDamageAction = nodes[i].AdditionalDamageAction;
			if (additionalDamageAction.HasValue)
			{
				ApplyAdditionalDamageAction(additionalDamageAction.Value);
			}

			CoreDamageActionOp? coreDamageAction = nodes[i].CoreDamageAction;
			if (coreDamageAction.HasValue)
			{
				ApplyCoreDamageAction(coreDamageAction.Value);
			}

			CoreAdditionalDamageActionOp? coreAdditionalDamageAction = nodes[i].CoreAdditionalDamageAction;
			if (coreAdditionalDamageAction.HasValue)
			{
				ApplyCoreAdditionalDamageAction(coreAdditionalDamageAction.Value);
			}

			HitChainDamageActionOp? hitChainAction = nodes[i].HitChainDamageAction;
			if (hitChainAction.HasValue)
			{
				ApplyHitChainDamageAction(hitChainAction.Value);
			}

			HitCountCooldownRefundActionOp? hitCountRefundAction = nodes[i].HitCountCooldownRefundAction;
			if (hitCountRefundAction.HasValue)
			{
				ApplyHitCountCooldownRefundAction(hitCountRefundAction.Value);
			}

			ReloadReducePerHitActionOp? reloadReduceAction = nodes[i].ReloadReducePerHitAction;
			if (reloadReduceAction.HasValue)
			{
				ApplyReloadReducePerHitAction(reloadReduceAction.Value);
			}
		}
	}

	/*
	 * 행동 종류에 맞는 실행 데이터 속성이나 상태별 보너스에 값을 누적한다.
	 */
	private void ApplyPlanAction(SkillActionOp action /* 동작 */)
	{
		switch (action.Kind)
		{
		case SkillActionOpKind.DamageMultiplier:
			DamageMultiplier *= PositiveOrDefault(action.Amount, 1f);
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

	/*
	 * 연속 적중 피해 증가값을 현재 실행 데이터에 누적한다.
	 */
	private void ApplyConsecutiveHitAction(ConsecutiveHitActionOp action /* 연속 적중 피해 동작 */)
	{
		ConsecutiveHitBonusRate += Mathf.Max(0f, action.BonusRate);
		ConsecutiveHitMax += Mathf.Max(0f, action.MaxBonus);
	}

	/*
	 * 분기 공격 값을 현재 실행 데이터에 적용한다.
	 */
	private void ApplyBranchDamageAction(BranchDamageActionOp action /* 분기 피해 동작 */)
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

	/*
	 * 상태 효과 조건을 만족할 때 사용할 피해 배율을 등록한다.
	 */
	private void ApplyConditionalDamageAction(ConditionalDamageActionOp action /* 상태 조건 피해 동작 */)
	{
		if (action.RequiredStatus != StatusEffectKind.None
			&& action.MinimumStacks > 0
			&& action.DamageMultiplier > 0f)
		{
			conditionalDamageActions.Add(action);
		}
	}

	private void ApplyConditionalCritChanceAction(ConditionalCritChanceActionOp action)
	{
		if (action.Condition.StatusKind != StatusEffectKind.None
			&& action.Condition.MinimumStacks > 0
			&& !Mathf.Approximately(action.ChanceBonus, 0f))
		{
			conditionalCritChanceActions.Add(action);
		}
	}

	private void ApplyBurstDamageAction(BurstDamageActionOp action)
	{
		if (action.DamageMultiplier > 0f)
		{
			burstDamageActions.Add(action);
		}
	}

	private void ApplyBurstStatusAction(BurstStatusActionOp action)
	{
		if (action.StacksBonus != 0)
		{
			burstStatusActions.Add(action);
		}
	}

	/*
	 * 공격자 상태 효과 조건에 따른 받는 피해 증가값을 현재 실행 데이터에 적용한다.
	 */
	private void ApplyStatusConditionalDamageTakenAction(StatusConditionalDamageTakenActionOp action /* 공격자 상태 조건 받는 피해 동작 */)
	{
		HasStatusConditionalDamageTakenBonus = true;
		StatusConditionalDamageTakenBonus += action.Bonus;
		StatusConditionalSourceStatusKind = action.RequiredSourceStatus;
	}

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

	private void ApplyAdditionalDamageAction(AdditionalDamageActionOp action)
	{
		HasOnHitAdditionalDamage = true;
		OnHitAdditionalDamageChance = action.Chance;
		OnHitAdditionalDamageMultiplier = action.Multiplier;
		OnHitAdditionalDamageAttribute = action.Attribute;
		OnHitAdditionalDamageTarget = action.Target;
	}

	private void ApplyCoreDamageAction(CoreDamageActionOp action)
	{
		CoreHitboxName = action.HitboxName;
		HasCoreDamageMultiplier = true;
		CoreDamageMultiplier *= action.Multiplier;
	}

	private void ApplyCoreAdditionalDamageAction(CoreAdditionalDamageActionOp action)
	{
		CoreHitboxName = action.HitboxName;
		HasCoreOnHitAdditionalDamage = true;
		CoreOnHitAdditionalDamageChance = action.Chance;
		CoreOnHitAdditionalDamageMultiplier = action.Multiplier;
		CoreOnHitAdditionalDamageAttribute = action.Attribute;
	}

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

	private void ApplyReloadReducePerHitAction(ReloadReducePerHitActionOp action)
	{
		if (string.IsNullOrWhiteSpace(action.TargetSkillId))
		{
			return;
		}

		ReloadReduceTargetSkillId = action.TargetSkillId;
		ReloadReduceSecondsPerHit += action.SecondsPerHit;
	}

	/*
	 * 전체 상태 또는 지정한 상태의 행동 속도 보너스를 누적한다.
	 */
	private void ApplyStatusActionSpeedBonus(string statusId /* 상태 효과 식별자 */, float bonus /* 추가로 더할 수치 */)
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

	/*
	 * 지정한 상태 효과의 지속시간 보너스를 누적한다.
	 */
	private void ApplyStatusDurationBonus(string statusId /* 상태 효과 식별자 */, float bonus /* 추가로 더할 수치 */)
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

	/*
	 * 현재 실행 데이터에 적용된 선택지 식별자를 기록한다.
	 */
	public void AddActiveChoiceId(string choiceId /* 스킬 선택지 식별자 */)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			activeChoiceIds.Add(choiceId);
		}
	}

	/*
	 * 지정한 선택지가 현재 실행 데이터에 적용되었는지 확인한다.
	 */
	public bool HasActiveChoice(string choiceId /* 스킬 선택지 식별자 */)
	{
		if (!string.IsNullOrWhiteSpace(choiceId))
		{
			return activeChoiceIds.Contains(choiceId);
		}
		return false;
	}

	/*
	 * 지정한 상태 효과에 누적된 지속시간 보너스를 반환한다.
	 */
	public float ResolveStatusDurationBonus(string statusId /* 상태 효과 식별자 */)
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

	/*
	 * 전체 상태 보너스와 지정한 상태의 행동 속도 보너스를 합산한다.
	 */
	public float ResolveStatusActionSpeedBonus(string statusId /* 상태 효과 식별자 */)
	{
		float num = StatusActionSpeedBonus;
		if (!string.IsNullOrWhiteSpace(statusId) && statusActionSpeedBonuses.TryGetValue(statusId, out var value))
		{
			num += value;
		}
		return num;
	}

	/*
	 * 지정한 상태 효과의 최대 중첩 보너스를 반환한다.
	 */
	public int ResolveStatusMaxStacksBonus(string statusId /* 상태 효과 식별자 */)
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

	/*
	 * 대상 상태 중첩 하나당 추가되는 피해 비율을 반환한다.
	 */
	public float ResolveTargetStatusStackDamageRateBonus(string statusId /* 상태 효과 식별자 */)
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

	/*
	 * 지정한 Trigger에 누적된 발동 확률 보너스를 반환한다.
	 */
	public float ResolveTriggerProcChanceBonus(string triggerId /* 트리거 식별자 */)
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

	/*
	 * 양수인 값만 사용하고 그렇지 않으면 전달받은 기본값을 반환한다.
	 */
	private static float PositiveOrDefault(float value /* 처리할 값 */, float fallback /* 기본 결과가 없을 때 사용할 값 */)
	{
		if (!(value > 0f))
		{
			return fallback;
		}
		return value;
	}

}

}
