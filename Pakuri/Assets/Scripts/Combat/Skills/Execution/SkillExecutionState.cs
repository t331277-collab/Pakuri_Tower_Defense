/*
 * 역할: 한 스킬의 확정값과 전투 진행 상태를 전달한다.
 * 규칙 계산 결과와 시전 준비값, 쿨다운, 탄창, 연사 상태를 원본 정의와 분리해 보관한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// 같은 스킬 정의에서 소유자별 실행값과 진행 상태를 분리한다.
public class SkillExecutionState
{

	internal readonly HashSet<string> activeChoiceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, float> targetStatusStackDamageMultiplierBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> statusActionSpeedMultipliers = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly List<ConditionalDamageActionOp> conditionalDamageActions = new List<ConditionalDamageActionOp>();

	internal readonly List<ConditionalStatusGroupDamageActionOp> conditionalStatusGroupDamageActions = new List<ConditionalStatusGroupDamageActionOp>();

	internal readonly List<ConditionalCritChanceActionOp> conditionalCritChanceActions = new List<ConditionalCritChanceActionOp>();

	internal readonly List<ConditionalCritActionOp> conditionalCritActions = new List<ConditionalCritActionOp>();

	internal readonly List<ConditionalFinalDamageActionOp> conditionalFinalDamageActions = new List<ConditionalFinalDamageActionOp>();

	internal readonly List<BurstDamageActionOp> burstDamageActions = new List<BurstDamageActionOp>();

	internal readonly List<BurstStatusActionOp> burstStatusActions = new List<BurstStatusActionOp>();

	internal readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	internal readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	internal readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	internal readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	internal readonly List<SkillCastEffect> castEffects = new List<SkillCastEffect>();

	internal readonly List<SkillReaction> reactions = new List<SkillReaction>();

	public SkillDefinition Source { get; }

	public string SkillName { get; }

	public float DamageMultiplier { get; internal set; }

	public bool HasRawDamageOverride { get; internal set; }

	public float RawDamageOverride { get; internal set; }

	public bool HasDamageAttributeOverride { get; internal set; }

	public DamageAttribute DamageAttributeOverride { get; internal set; }

	public float ShieldAmountMultiplier { get; internal set; }

	public float CooldownMultiplier { get; internal set; }

	public float RadiusMultiplier { get; internal set; }

	public bool HasRadiusMultiplierOverride { get; internal set; }

	public float RadiusMultiplierOverride { get; internal set; }

	public float DurationMultiplier { get; internal set; }

	public int MagazineBonus { get; internal set; }

	public int AdditionalProjectileBonus { get; internal set; }

	public int PierceBonus { get; internal set; }

	public float ReloadTimeMultiplier { get; internal set; }

	public float ShotIntervalMultiplier { get; internal set; }

	public int FollowUpProjectileCount { get; internal set; }

	public float FollowUpProjectileDelaySeconds { get; internal set; }

	public float FollowUpProjectileDamageMultiplier { get; internal set; } = 1f;

	public bool FollowUpProjectileFirstMagazineOnly { get; internal set; }

	public float MagazineLastProjectileDamageMultiplier { get; internal set; } = 1f;

	public float RadiusBonus { get; internal set; }

	public float BeamWidthBonus { get; internal set; }

	public float KnockbackDistanceMultiplier { get; internal set; }

	public float DamageDelayMultiplier { get; internal set; }

	public bool HasDamageDelayOverride { get; internal set; }

	public float DamageDelayOverride { get; internal set; }

	public int ArrivalFragmentCount { get; internal set; }

	public float ArrivalFragmentDelaySeconds { get; internal set; }

	public float ArrivalFragmentSearchRadius { get; internal set; }

	public float ArrivalFragmentRawDamage { get; internal set; }

	public float DurationBonus { get; internal set; }

	public float BranchChanceBonus { get; internal set; }

	public bool HasBranchChanceSet { get; internal set; }

	public float BranchChanceSet { get; internal set; }

	public bool HasBranchCount { get; internal set; }

	public int BranchCount { get; internal set; }

	public bool HasBranchDamageMultiplier { get; internal set; }

	public float BranchDamageMultiplier { get; internal set; }

	public bool HasBranchSearchRadius { get; internal set; }

	public float BranchSearchRadius { get; internal set; }

	public int BranchLaunchPeriod { get; internal set; }

	public bool HasBranchLaunchChanceSet { get; internal set; }

	public float BranchLaunchChanceSet { get; internal set; }

	public int HitTargetCountBonus { get; internal set; }

	public int LineCastRepeatCountBonus { get; internal set; }

	public float CritChanceBonus { get; internal set; }

	public float CritDamageBonus { get; internal set; }

	public float MagazineLastProjectileCritDamageBonus { get; internal set; }

	public float FinalDamageModifier { get; internal set; }

	public float CriticalFinalDamageModifier { get; internal set; }

	public float ConsecutiveHitBonusRate { get; internal set; }

	public float ConsecutiveHitMax { get; internal set; }

	public string StatusTag { get; internal set; }

	public float StatusChanceBonus { get; internal set; }

	public bool HasStatusActionSpeedBonus { get; internal set; }

	public string StatusActionSpeedBonusStatusName { get; internal set; }

	public float StatusActionSpeedBonus { get; internal set; }

	public float StatusActionSpeedMultiplier { get; internal set; } = 1f;

	public bool HasStatusAttackPowerBonus { get; internal set; }

	public float StatusAttackPowerBonus { get; internal set; }

	public int StatusStacksBonus { get; internal set; }

	public bool HasStatusStacksSet { get; internal set; }

	public int StatusStacksSet { get; internal set; }

	public bool HasStatusElementDamageTakenBonus { get; internal set; }

	public float StatusElementDamageTakenBonus { get; internal set; }

	public bool HasStatusCriticalDamageTakenBonus { get; internal set; }

	public float StatusCriticalDamageTakenBonus { get; internal set; }

	public bool HasStatusAilmentResistanceBonus { get; internal set; }

	public float StatusAilmentResistanceBonus { get; internal set; }

	public bool HasStatusDamageBonusRate { get; internal set; }

	public float StatusDamageBonusRate { get; internal set; }

	public bool HasStatusShieldReceivedBonus { get; internal set; }

	public float StatusShieldReceivedBonus { get; internal set; }

	public bool HasStatusCriticalChanceBonus { get; internal set; }

	public float StatusCriticalChanceBonus { get; internal set; }

	public bool HasStatusDamageTakenBonus { get; internal set; }

	public float StatusDamageTakenBonus { get; internal set; }

	public bool HasStatusFlatElementResistReduction { get; internal set; }

	public float StatusFlatElementResistReduction { get; internal set; }

	public bool HasStatusConditionalDamageTakenBonus { get; internal set; }

	public float StatusConditionalDamageTakenBonus { get; internal set; }

	public StatusEffectKind StatusConditionalSourceStatusKind { get; internal set; }

	public bool HasOnHitAdditionalDamage { get; internal set; }

	public float OnHitAdditionalDamageChance { get; internal set; }

	public float OnHitAdditionalDamageMultiplier { get; internal set; }

	public DamageAttribute OnHitAdditionalDamageAttribute { get; internal set; }

	public string OnHitAdditionalDamageTarget { get; internal set; }

	public int OnHitChainHitPeriod { get; internal set; }

	public int OnHitChainTargetCount { get; internal set; }

	public float OnHitChainSearchRadius { get; internal set; }

	public float OnHitChainDamageMultiplier { get; internal set; }

	public DamageAttribute OnHitChainDamageAttribute { get; internal set; }

	public string ReloadReduceTargetSkillName { get; internal set; }

	public float ReloadReduceSecondsPerHit { get; internal set; }

	public string CoreHitboxName { get; internal set; }

	public bool HasCoreDamageMultiplier { get; internal set; }

	public float CoreDamageMultiplier { get; internal set; } = 1f;

	public bool HasCoreOnHitAdditionalDamage { get; internal set; }

	public float CoreOnHitAdditionalDamageChance { get; internal set; }

	public float CoreOnHitAdditionalDamageMultiplier { get; internal set; } = 1f;

	public DamageAttribute CoreOnHitAdditionalDamageAttribute { get; internal set; }

	public string HitCountCooldownRefundTargetSkillName { get; internal set; }

	public int HitCountCooldownRefundMinTargets { get; internal set; }

	public float HitCountCooldownRefundRatio { get; internal set; }

	public int RepeatCountPerTarget { get; internal set; }

	public float RepeatIntervalSeconds { get; internal set; }

	public float RepeatDamageMultiplier { get; internal set; } = 1f;

	public StatusEffectKind ThresholdStatusKind { get; internal set; }

	public int ThresholdStatusMinStacks { get; internal set; }

	public StatusEffectKind ThresholdApplyStatusKind { get; internal set; }

	public float TargetStatusStackDamageMultiplier { get; internal set; } = 1f;

	public bool HasConsumeTargetStatusRatioOverride { get; internal set; }

	public float ConsumeTargetStatusRatioOverride { get; internal set; }

	public bool HasConsumeTargetStatusStacksOverride { get; internal set; }

	public int ConsumeTargetStatusStacksOverride { get; internal set; }

	public float RedistributeConsumedStatusRatioOnKill { get; internal set; }

	public StatusEffectKind RedistributeConsumedStatusKind { get; internal set; }

	public float RedistributeConsumedStatusSearchRadius { get; internal set; }

	public int RedistributeConsumedStatusTargetCount { get; internal set; }

	public GameObject SkillEffectPrefab { get; internal set; }

	internal string PreparedSkillName { get; set; }

	internal SkillTargetingSpec PreparedTargeting { get; set; }

	internal RuntimeSkillVisualSpec PreparedRuntimeVisual { get; set; }

	internal Vector2 PreparedOrigin { get; set; }

	internal Vector2 PreparedDirection { get; set; }

	internal IReadOnlyList<Vector2> PreparedDirections { get; set; } = Array.Empty<Vector2>();

	internal IReadOnlyList<float> PreparedBoundaries { get; set; } = Array.Empty<float>();

	internal IReadOnlyList<float> PreparedBranchChances { get; set; } = Array.Empty<float>();

	internal IReadOnlyList<int> PreparedBranchCounts { get; set; } = Array.Empty<int>();

	internal IReadOnlyList<float> PreparedBranchDamageMultipliers { get; set; } = Array.Empty<float>();

	internal IReadOnlyList<float> PreparedBranchSearchRadii { get; set; } = Array.Empty<float>();

	internal IReadOnlyList<Vector2> PreparedCenters { get; set; } = Array.Empty<Vector2>();

	internal float PreparedDamage { get; set; }

	internal DamageAttribute PreparedDamageAttribute { get; set; }

	internal StatusApplicationSpec PreparedStatus { get; set; }

	internal StatusApplicationSpec OnHitStatusOverride { get; set; }

	internal float PreparedLength { get; set; }

	internal float PreparedWidth { get; set; }

	internal float PreparedKnockbackDistance { get; set; }

	internal float PreparedDuration { get; set; }

	internal float PreparedTickInterval { get; set; }

	internal float PreparedRepeatInterval { get; set; }

	internal bool PreparedCriticalAllowed { get; set; }

	internal float PreparedRadius { get; set; }

	internal float PreparedBaseRadius { get; set; }

	internal float PullDistancePerTick { get; set; }

	internal float PreparedVisualRadiusMultiplier { get; set; } = 1f;

	internal int PreparedHitTargetCount { get; set; } = int.MaxValue;

	internal bool PreparedCoverAll { get; set; }

	internal bool PreparedIsRecast { get; set; }

	internal int PreparedRecastGeneration { get; set; }

	internal float PreparedProjectileSpeed { get; set; }

	internal int PreparedPierceCount { get; set; }

	internal float PreparedProjectileLifetime { get; set; }

	internal int PreparedBurstProjectileCount { get; set; } = 1;

	internal int PreparedBurstProjectileIndex { get; set; } = 1;

	internal float PreparedBurstDamageMultiplier { get; set; } = 1f;

	internal bool PreparedMagazineLastProjectile { get; set; }

	internal bool PreparedMagazineFirstProjectile { get; set; }

	internal bool PreparedContactDamageEnabled { get; set; }

	internal float PreparedArrivalDelay { get; set; }

	internal SingleSkillDefinition PreparedArrivalSkill { get; set; }

	internal bool PreparedHasProjectileTargetPoint { get; set; }

	internal Vector2 PreparedProjectileTargetPoint { get; set; }

	internal GameObject PreparedSkillEffectPrefab { get; set; }

	internal bool PreparedUsePrefabHitbox { get; set; }

	internal bool PreparedUsesHitTargetCount { get; set; }

	internal bool PreparedUsesResolvedDeployments { get; set; }

	internal bool PreparedPrefabHitboxAtOrigin { get; set; }

	internal StatusEffectKind PreparedTargetStatusStackStatusKind { get; set; }

	internal int PreparedTargetStatusStackMaxStacks { get; set; }

	internal float PreparedTargetStatusStackDamage { get; set; }

	internal float PreparedTargetStatusStackDamageRateBonus { get; set; }

	internal StatusEffectKind PreparedConsumeTargetStatusKind { get; set; }

	internal float PreparedConsumeTargetStatusRatio { get; set; }

	internal int PreparedConsumeTargetStatusStacks { get; set; }

	internal float PreparedExecuteHealthRatioThreshold { get; set; }

	internal float PreparedExecuteDamageMultiplier { get; set; } = 1f;

	internal float PreparedKillCooldownRefundRatio { get; set; }

	internal float PreparedBossDamageMultiplier { get; set; } = 1f;

	internal BuffEffectKind PreparedBuffEffectKind { get; set; }

	internal IReadOnlyList<CombatUnitEntry> PreparedTargets { get; set; } = Array.Empty<CombatUnitEntry>();

	internal float PreparedHealAmount { get; set; }

	internal float PreparedShieldAmount { get; set; }

	internal float PreparedShieldTargetMaxHealthRatio { get; set; }

	internal StatusRuntimeData PreparedShieldStatusData { get; set; }

	internal bool PreparedAttachVisualToCaster { get; set; }

	internal float PreparedChargeTargetMaxHealthRatio { get; set; }

	internal float PreparedChargeRampSeconds { get; set; }

	internal float PreparedChargeMaxMoveSpeedMultiplier { get; set; } = 1f;

	public IReadOnlyList<CastConditionOp> CastConditionOps => castConditionOps;

	public IReadOnlyList<DamageModifierOp> DamageModifierOps => damageModifierOps;

	public IReadOnlyList<CritModifierOp> CritModifierOps => critModifierOps;

	public IReadOnlyList<KillActionOp> KillActionOps => killActionOps;

	public IReadOnlyList<SkillCastEffect> CastEffects => castEffects;

	public IReadOnlyList<SkillReaction> Reactions => reactions;

	internal IReadOnlyList<ConditionalDamageActionOp> ConditionalDamageActions => conditionalDamageActions;

	internal IReadOnlyList<ConditionalStatusGroupDamageActionOp> ConditionalStatusGroupDamageActions => conditionalStatusGroupDamageActions;

	internal IReadOnlyList<ConditionalCritChanceActionOp> ConditionalCritChanceActions => conditionalCritChanceActions;

	internal IReadOnlyList<ConditionalCritActionOp> ConditionalCritActions => conditionalCritActions;

	internal IReadOnlyList<ConditionalFinalDamageActionOp> ConditionalFinalDamageActions => conditionalFinalDamageActions;

	internal IReadOnlyDictionary<string, float> TargetStatusStackDamageMultiplierBonuses => targetStatusStackDamageMultiplierBonuses;

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

	/// 원본 정의를 바꾸지 않고 규칙 계산을 시작할 기준값을 만든다.
	public SkillExecutionState(SkillDefinition source)
	{
		Source = source;
		SkillName = string.Empty;
		if (source != null)
		{
			SkillName = source.SkillName;
		}
		DamageMultiplier = 1f;
		FinalDamageModifier = 1f;
		CriticalFinalDamageModifier = 1f;
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
		}
	}

	public UnitCombatState Owner { get; internal set; }

	public SkillDefinition Data => Source;

	public SkillSlot Slot => Source != null ? Source.Slot : default;

	/// 소유자마다 독립적으로 진행될 스킬 상태를 시작한다.
	public SkillExecutionState(UnitCombatState owner, SkillDefinition source)
		: this(source)
	{
		Owner = owner;
		SkillExecutionRules.InitializeRuntimeValues(this, null);
		SkillExecution.ResetRuntimeState(this);
	}

    public float CooldownRemaining { get; internal set; }
    public float CastRemaining { get; internal set; }
    public float ActiveDurationRemaining { get; internal set; }
    public float TickRemaining { get; internal set; }
    public float ReloadRemaining { get; internal set; }
    public int MagazineRemaining { get; internal set; }
    public int ProjectileLaunchCount { get; internal set; }
    public int SkillHitCount { get; internal set; }
    internal SkillExecutionState ActiveExecutionData { get; set; }
    internal bool IsTrigger { get; set; }

    internal int effectiveMaxMagazineSize;
    internal int effectiveBurstProjectileCount;
    internal float effectiveReloadDuration;
    internal float effectiveTickInterval;
    internal float effectiveBurstInterval;
    internal float effectiveCooldownDuration;
    internal int queuedBurstShotsRemaining;
    internal string consecutiveHitTargetUnitName;
    internal int consecutiveHitRepeatCount;

    public bool IsCasting => CastRemaining > 0f;
    public bool IsActive => ActiveDurationRemaining > 0f;
    public bool IsReloading => ReloadRemaining > 0f;
    public bool IsBursting => queuedBurstShotsRemaining > 0;
    public int MaxMagazineSize => effectiveMaxMagazineSize;
    public float ReloadDuration => effectiveReloadDuration;
    public float EffectiveCooldownDuration => effectiveCooldownDuration;
    public int EffectiveBurstProjectileCount => effectiveBurstProjectileCount;
    public bool UsesMagazine => MaxMagazineSize > 0;
    public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;

	/// 전투 중 추가 피해 배율을 현재 값에 반영한다.
	internal void ApplyDynamicDamageMultiplier(float multiplier)
	{
		DamageMultiplier *= PositiveOrDefault(multiplier, 1f);
	}

	/// 외부 사건의 피해 배율을 현재 값에 곱한다.
	internal void ScaleDamageMultiplier(float multiplier)
	{
		DamageMultiplier *= Mathf.Max(0f, multiplier);
	}

	/// 사건이 정한 원시 피해를 실행값에 기록한다.
	internal void SetRawDamageOverride(float rawDamage)
	{
		HasRawDamageOverride = true;
		RawDamageOverride = Mathf.Max(0f, rawDamage);
	}

	/// 피해 배율만 다른 실행값 사본을 만든다.
	internal SkillExecutionState CopyWithDamageMultiplier(float multiplier)
	{
		var copy = (SkillExecutionState)MemberwiseClone();
		copy.DamageMultiplier *= Mathf.Max(0f, multiplier);
		return copy;
	}

	/// 현재 실행값에 반영된 학습 선택을 기록한다.
	public void AddActiveChoiceName(string choiceName)
	{
		if (!string.IsNullOrWhiteSpace(choiceName))
		{
			activeChoiceNames.Add(choiceName);
		}
	}

	/// 현재 실행이 특정 학습 선택의 영향을 받는지 확인한다.
	public bool HasActiveChoice(string choiceName)
	{
		if (!string.IsNullOrWhiteSpace(choiceName))
		{
			return activeChoiceNames.Contains(choiceName);
		}
		return false;
	}

	/// 상태 지속시간 보정량을 읽는다.
	public float StatusDurationBonus(string statusName)
	{
		if (string.IsNullOrWhiteSpace(statusName))
		{
			return 0f;
		}
		if (!statusDurationBonuses.TryGetValue(statusName, out var value))
		{
			return 0f;
		}
		return value;
	}

	/// 상태 행동 속도 보정량을 읽는다.
	public float GetStatusActionSpeedBonus(string statusName)
	{
		float num = StatusActionSpeedBonus;
		if (!string.IsNullOrWhiteSpace(statusName) && statusActionSpeedBonuses.TryGetValue(statusName, out var value))
		{
			num += value;
		}
		return num;
	}

	/// 상태 최대 중첩 보정량을 읽는다.
	public int StatusMaxStacksBonus(string statusName)
	{
		if (string.IsNullOrWhiteSpace(statusName))
		{
			return 0;
		}
		if (!statusMaxStacksBonuses.TryGetValue(statusName, out var value))
		{
			return 0;
		}
		return value;
	}

	/// 대상 상태 중첩 피해 보정량을 읽는다.
	public float TargetStatusStackDamageRateBonus(string statusName)
	{
		if (string.IsNullOrWhiteSpace(statusName))
		{
			return 0f;
		}
		if (!targetStatusStackDamageRateBonuses.TryGetValue(statusName, out var value))
		{
			return 0f;
		}
		return value;
	}

	/// 대상 상태 중첩당 전체 스킬 피해 배율 보정량을 읽는다.
	public float TargetStatusStackDamageMultiplierBonus(string statusName)
	{
		if (string.IsNullOrWhiteSpace(statusName)
			|| !targetStatusStackDamageMultiplierBonuses.TryGetValue(statusName, out var value))
		{
			return 0f;
		}
		return value;
	}

	/// 상태 효과의 행동속도 곱연산 보정을 읽는다.
	public float GetStatusActionSpeedMultiplier(string statusName)
	{
		if (!string.IsNullOrWhiteSpace(statusName)
			&& statusActionSpeedMultipliers.TryGetValue(statusName, out var value))
		{
			return StatusActionSpeedMultiplier * value;
		}
		return StatusActionSpeedMultiplier;
	}

	/// 반응 발동 확률 보정량을 읽는다.
	public float TriggerProcChanceBonus(string triggerName)
	{
		if (string.IsNullOrWhiteSpace(triggerName))
		{
			return 0f;
		}
		if (!triggerProcChanceBonuses.TryGetValue(triggerName, out var value))
		{
			return 0f;
		}
		return value;
	}

	/// 보정값을 허용 범위의 기본값으로 정규화한다.
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
