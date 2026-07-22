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
	 * 대상이 필요한 상태 중첩을 가지고 있을 때 적용할 피해 배율을 보관한다.
	 */
	private readonly struct ConditionalDamageRule
	{
		public float DamageMultiplier { get; }

		public StatusEffectKind StatusKind { get; }

		public int MinStacks { get; }

		/*
		 * ConditionalDamageRule에 필요한 값을 초기화한다.
		 */
		public ConditionalDamageRule(float damageMultiplier /* 피해량에 곱할 배율 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minStacks /* 최소 중첩 수 */)
		{
			DamageMultiplier = damageMultiplier;
			StatusKind = statusKind;
			MinStacks = minStacks;
		}
	}

	/*
	 * 대상의 상태 중첩 조건에 따라 추가할 치명타 확률을 보관한다.
	 */
	private readonly struct ConditionalCritChanceRule
	{
		public float CritChanceBonus { get; }

		public StatusEffectKind StatusKind { get; }

		public int MinStacks { get; }

		/*
		 * ConditionalCritChanceRule에 필요한 값을 초기화한다.
		 */
		public ConditionalCritChanceRule(float critChanceBonus /* 추가 치명타 확률 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minStacks /* 최소 중첩 수 */)
		{
			CritChanceBonus = critChanceBonus;
			StatusKind = statusKind;
			MinStacks = minStacks;
		}
	}

	/*
	 * 연속 발사 중 특정 투사체에 적용할 피해 배율을 보관한다.
	 */
	private readonly struct BurstDamageRule
	{
		public int ProjectileIndex { get; }

		public float DamageMultiplier { get; }

		/*
		 * BurstDamageRule에 필요한 값을 초기화한다.
		 */
		public BurstDamageRule(int projectileIndex /* 투사체 순서 번호 */, float damageMultiplier /* 피해량에 곱할 배율 */)
		{
			ProjectileIndex = projectileIndex;
			DamageMultiplier = damageMultiplier;
		}
	}

	/*
	 * 연속 발사 중 특정 투사체가 추가할 상태 중첩을 보관한다.
	 */
	private readonly struct BurstStatusRule
	{
		public int ProjectileIndex { get; }

		public int StacksBonus { get; }

		/*
		 * BurstStatusRule에 필요한 값을 초기화한다.
		 */
		public BurstStatusRule(int projectileIndex /* 투사체 순서 번호 */, int stacksBonus /* 중첩 수 추가값 */)
		{
			ProjectileIndex = projectileIndex;
			StacksBonus = stacksBonus;
		}
	}

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
	private readonly List<ConditionalDamageRule> conditionalDamageRules = new List<ConditionalDamageRule>();

	private readonly List<ConditionalCritChanceRule> conditionalCritChanceRules = new List<ConditionalCritChanceRule>();

	private readonly List<BurstDamageRule> burstDamageRules = new List<BurstDamageRule>();

	private readonly List<BurstStatusRule> burstStatusRules = new List<BurstStatusRule>();

	/*
	 * 선택지 값에서 만든 조건, 보정, 처치 행동과 정규화된 노드를 보관한다.
	 */
	private readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	private readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	private readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	private readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	private readonly List<SkillNode> normalizedPlanNodes = new List<SkillNode>();

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

	public IReadOnlyList<SkillNode> NormalizedPlanNodes => normalizedPlanNodes;

	/*
	 * 기본 효과와 현재 유닛이 선택한 노드 효과를 실행 목록으로 반환한다.
	 */
	public SkillEffectDefinition[] CollectEffects(SkillEffectDefinition[] baseEffects /* 스킬 기본 효과 목록 */)
	{
		var effects = new List<SkillEffectDefinition>();
		if (baseEffects != null)
		{
			for (var i = 0; i < baseEffects.Length; i++)
			{
				if (baseEffects[i] != null)
				{
					effects.Add(baseEffects[i]);
				}
			}
		}

		for (var i = 0; i < normalizedPlanNodes.Count; i++)
		{
			var node = normalizedPlanNodes[i];
			if (node != null && node.Effect != null)
			{
				effects.Add(node.Effect);
			}
		}
		return effects.ToArray();
	}

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
		BossDamageMultiplier = 1f;
		BranchDamageMultiplier = 1f;
		OnHitAdditionalDamageMultiplier = 1f;
		OnHitChainDamageMultiplier = 1f;
		if (source != null)
		{
			SkillEffectPrefab = source.SkillEffectPrefab;
			AddNormalizedPlanNodes(source.NormalizedPlanNodes);
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
		ApplyNodeBackedChoiceDefinition(spec.Source);
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
	private void ApplyNodeBackedChoiceDefinition(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */)
	{
		if (choice.SkillEffectPrefab != null)
		{
			SkillEffectPrefab = choice.SkillEffectPrefab;
		}
		SkillNodeDefinition[] array = SkillNodeMapper.FilterSkillNodeDefinitionsForTarget(choice.NormalizedPlanNodes, SkillId);
		SkillChoice spec = new SkillChoice
		{
			Source = new SkillChoiceDefinition()
		};
		SkillChoiceCompiler.ApplyChoiceFieldNodes(spec, array);
		ApplyNodeBackedChoiceFields(spec);
		SkillNode[] nodes = SkillNodeMapper.MapSkillNodeDefinitions(array);
		AddNormalizedPlanNodes(nodes);
		ApplyPlanActionNodes(nodes);
		RefreshSingleOperationBridges();
	}

	/*
	 * 선택지 컴파일 결과 중 개별 속성으로 표현되는 특수 강화 값을 누적한다.
	 */
	private void ApplyNodeBackedChoiceFields(SkillChoice spec /* 처리에 사용할 설정 */)
	{
		SkillChoiceDefinition source = spec.Source;
		CritChanceBonus += source.CritChanceBonus;
		CritDamageBonus += source.CritDamageBonus;
		BeamWidthBonus += source.BeamWidthBonus;
		if (source.HasKnockbackDistanceMultiplier)
		{
			KnockbackDistanceMultiplier *= source.KnockbackDistanceMultiplier;
		}
		if (!string.IsNullOrWhiteSpace(source.ReloadReduceTargetSkillId))
		{
			ReloadReduceTargetSkillId = source.ReloadReduceTargetSkillId;
			ReloadReduceSecondsPerHit += source.ReloadReduceSecondsPerHit;
		}
		if (source.HasCoreDamageMultiplier)
		{
			CoreHitboxName = source.CoreHitboxName;
			HasCoreDamageMultiplier = true;
			CoreDamageMultiplier *= source.CoreDamageMultiplier;
		}
		if (source.HasCoreOnHitAdditionalDamage)
		{
			CoreHitboxName = source.CoreHitboxName;
			HasCoreOnHitAdditionalDamage = true;
			CoreOnHitAdditionalDamageChance = source.CoreOnHitAdditionalDamageChance;
			CoreOnHitAdditionalDamageMultiplier = source.CoreOnHitAdditionalDamageMultiplier;
			CoreOnHitAdditionalDamageAttribute = source.CoreOnHitAdditionalDamageAttribute;
		}
		if (!string.IsNullOrWhiteSpace(source.HitCountCooldownRefundTargetSkillId))
		{
			HitCountCooldownRefundTargetSkillId = source.HitCountCooldownRefundTargetSkillId;
			HitCountCooldownRefundMinTargets = source.HitCountCooldownRefundMinTargets;
			HitCountCooldownRefundRatio = source.HitCountCooldownRefundRatio;
		}
		if (source.HasOnHitAdditionalDamage)
		{
			HasOnHitAdditionalDamage = true;
			OnHitAdditionalDamageChance = source.OnHitAdditionalDamageChance;
			OnHitAdditionalDamageMultiplier = source.OnHitAdditionalDamageMultiplier;
			OnHitAdditionalDamageAttribute = source.OnHitAdditionalDamageAttribute;
			OnHitAdditionalDamageTarget = source.OnHitAdditionalDamageTarget;
		}
		if (source.OnHitChainHitPeriod > 0)
		{
			OnHitChainHitPeriod = source.OnHitChainHitPeriod;
			OnHitChainTargetCount = source.OnHitChainTargetCount;
			OnHitChainSearchRadius = source.OnHitChainSearchRadius;
			OnHitChainDamageMultiplier = source.OnHitChainDamageMultiplier;
			OnHitChainDamageAttribute = source.OnHitChainDamageAttribute;
		}
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
		if (source.ThresholdStatusKind != StatusEffectKind.None && source.ThresholdStatusMinStacks > 0 && source.ThresholdApplyStatusKind != StatusEffectKind.None)
		{
			ThresholdStatusKind = source.ThresholdStatusKind;
			ThresholdStatusMinStacks = source.ThresholdStatusMinStacks;
			ThresholdApplyStatusKind = source.ThresholdApplyStatusKind;
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
		if (!Mathf.Approximately(source.ConditionalCritChanceBonus, 0f) && source.ConditionalCritTargetStatusKind != StatusEffectKind.None && source.ConditionalCritTargetStatusMinStacks > 0)
		{
			conditionalCritChanceRules.Add(new ConditionalCritChanceRule(source.ConditionalCritChanceBonus, source.ConditionalCritTargetStatusKind, source.ConditionalCritTargetStatusMinStacks));
		}
		if (source.RedistributeConsumedStatusRatioOnKill > 0f && source.RedistributeConsumedStatusKind != StatusEffectKind.None && source.RedistributeConsumedStatusSearchRadius > 0f)
		{
			RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(source.RedistributeConsumedStatusRatioOnKill);
			RedistributeConsumedStatusKind = source.RedistributeConsumedStatusKind;
			RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, source.RedistributeConsumedStatusSearchRadius);
			RedistributeConsumedStatusTargetCount = Mathf.Max(0, source.RedistributeConsumedStatusTargetCount);
		}
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
	private void ApplyPlanActionNodes(IReadOnlyList<SkillNode> nodes /* 노드 목록 */)
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

			StatusConditionalDamageTakenActionOp? statusDamageTakenAction = nodes[i].StatusConditionalDamageTakenAction;
			if (statusDamageTakenAction.HasValue)
			{
				ApplyStatusConditionalDamageTakenAction(statusDamageTakenAction.Value);
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
		AddConditionalDamageRule(action.DamageMultiplier, action.RequiredStatus, action.MinimumStacks);
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
	 * 상태 종류와 최소 중첩 조건을 만족할 때 사용할 피해 배율 규칙을 추가한다.
	 */
	private void AddConditionalDamageRule(float multiplier /* 값에 곱할 배율 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minStacks /* 최소 중첩 수 */)
	{
		if (statusKind != StatusEffectKind.None && !(multiplier <= 0f))
		{
			conditionalDamageRules.Add(new ConditionalDamageRule(multiplier, statusKind, Mathf.Max(1, minStacks)));
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
	 * 대상이 만족하는 상태 중첩 규칙의 피해 배율을 모두 곱해 반환한다.
	 */
	public float ResolveConditionalDamageMultiplier(UnitCombatState target /* 효과를 받을 대상 유닛 */)
	{
		if (target == null || conditionalDamageRules.Count == 0)
		{
			return 1f;
		}
		float num = 1f;
		for (int i = 0; i < conditionalDamageRules.Count; i++)
		{
			ConditionalDamageRule conditionalDamageRule = conditionalDamageRules[i];
			if (HasRequiredStacks(target, conditionalDamageRule.StatusKind, conditionalDamageRule.MinStacks))
			{
				num *= PositiveOrDefault(conditionalDamageRule.DamageMultiplier, 1f);
			}
		}
		return num;
	}

	/*
	 * 대상이 만족하는 상태 중첩 규칙의 치명타 확률 보너스를 모두 더해 반환한다.
	 */
	public float ResolveConditionalCritChanceBonus(UnitCombatState target /* 효과를 받을 대상 유닛 */)
	{
		if (target == null || conditionalCritChanceRules.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < conditionalCritChanceRules.Count; i++)
		{
			ConditionalCritChanceRule conditionalCritChanceRule = conditionalCritChanceRules[i];
			if (HasRequiredStacks(target, conditionalCritChanceRule.StatusKind, conditionalCritChanceRule.MinStacks))
			{
				num += conditionalCritChanceRule.CritChanceBonus;
			}
		}
		return num;
	}

	/*
	 * 현재 투사체 순서에 맞는 연속 발사 피해 배율을 모두 곱해 반환한다.
	 */
	public float ResolveBurstDamageMultiplier(int projectileIndex /* 투사체 순서 번호 */, int burstProjectileCount /* 연속 발사 투사체 개수 */)
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

	/*
	 * 현재 투사체 순서에 맞는 연속 발사 상태 중첩 보너스를 모두 더해 반환한다.
	 */
	public int ResolveBurstStatusStacksBonus(int projectileIndex /* 투사체 순서 번호 */, int burstProjectileCount /* 연속 발사 투사체 개수 */)
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

	/*
	 * 대상이 지정한 상태 또는 보호막의 최소 중첩 조건을 만족하는지 확인한다.
	 */
	private static bool HasRequiredStacks(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks /* 최소 중첩 수 */)
	{
		if (target == null || minimumStacks <= 0 || statusKind == StatusEffectKind.None)
		{
			return false;
		}
		if (statusKind == StatusEffectKind.Shield)
		{
			if (target.Resources != null)
			{
				return target.Resources.CurrentShield > 0f;
			}
			return false;
		}
		if (target.Statuses != null)
		{
			return target.Statuses.GetStacks(statusKind) >= minimumStacks;
		}
		return false;
	}

	/*
	 * 설정한 순서가 현재 투사체와 같은지 확인하며 0은 마지막 투사체로 처리한다.
	 */
	private static bool MatchesBurstProjectileIndex(int configuredIndex /* 설정된 순서 번호 */, int projectileIndex /* 투사체 순서 번호 */, int burstProjectileCount /* 연속 발사 투사체 개수 */)
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

	/*
	 * 누적된 단일 속성 값을 실행기가 사용하는 조건·보정·처치 행동 목록으로 다시 만든다.
	 */
	private void RefreshSingleOperationBridges()
	{
		castConditionOps.Clear();
		damageModifierOps.Clear();
		critModifierOps.Clear();
		killActionOps.Clear();
		if (!Mathf.Approximately(ExecuteHealthRatioBonus, 0f))
		{
			castConditionOps.Add(new CastConditionOp(ExecuteHealthRatioBonus));
		}
		if (!Mathf.Approximately(BossDamageMultiplier, 1f))
		{
			damageModifierOps.Add(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, BossDamageMultiplier));
		}
		if (!Mathf.Approximately(ExecuteCritChanceBonus, 0f))
		{
			critModifierOps.Add(new CritModifierOp(ExecuteCritChanceBonus));
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

	/*
	 * null이 아닌 정규화 노드만 현재 실행 계획 원본 목록에 추가한다.
	 */
	private void AddNormalizedPlanNodes(IReadOnlyList<SkillNode> nodes /* 노드 목록 */)
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
