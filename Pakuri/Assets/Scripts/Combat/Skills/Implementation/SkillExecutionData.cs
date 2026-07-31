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

	internal readonly HashSet<string> activeChoiceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	internal readonly List<ConditionalDamageActionOp> conditionalDamageActions = new List<ConditionalDamageActionOp>();

	internal readonly List<ConditionalCritChanceActionOp> conditionalCritChanceActions = new List<ConditionalCritChanceActionOp>();

	internal readonly List<BurstDamageActionOp> burstDamageActions = new List<BurstDamageActionOp>();

	internal readonly List<BurstStatusActionOp> burstStatusActions = new List<BurstStatusActionOp>();

	internal readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();

	internal readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();

	internal readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();

	internal readonly List<KillActionOp> killActionOps = new List<KillActionOp>();

	internal readonly List<SkillCastEffect> castEffects = new List<SkillCastEffect>();

	internal readonly List<SkillReaction> reactions = new List<SkillReaction>();

	public SkillDefinition Source { get; }

	public string SkillId { get; }

	public float DamageMultiplier { get; internal set; }

	public bool HasRawDamageOverride { get; internal set; }

	public float RawDamageOverride { get; internal set; }

	public float ShieldAmountMultiplier { get; internal set; }

	public float CooldownMultiplier { get; internal set; }

	public float RadiusMultiplier { get; internal set; }

	public float DurationMultiplier { get; internal set; }

	public int MagazineBonus { get; internal set; }

	public int AdditionalProjectileBonus { get; internal set; }

	public int PierceBonus { get; internal set; }

	public float ReloadTimeMultiplier { get; internal set; }

	public float ShotIntervalMultiplier { get; internal set; }

	public int FollowUpProjectileCount { get; internal set; }

	public float FollowUpProjectileDelaySeconds { get; internal set; }

	public float FollowUpProjectileDamageMultiplier { get; internal set; } = 1f;

	public float RadiusBonus { get; internal set; }

	public float BeamWidthBonus { get; internal set; }

	public float KnockbackDistanceMultiplier { get; internal set; }

	public float DamageDelayMultiplier { get; internal set; }

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

	public float ConsecutiveHitBonusRate { get; internal set; }

	public float ConsecutiveHitMax { get; internal set; }

	public string StatusTag { get; internal set; }

	public float StatusChanceBonus { get; internal set; }

	public bool HasStatusActionSpeedBonus { get; internal set; }

	public string StatusActionSpeedBonusStatusId { get; internal set; }

	public float StatusActionSpeedBonus { get; internal set; }

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

	public string ReloadReduceTargetSkillId { get; internal set; }

	public float ReloadReduceSecondsPerHit { get; internal set; }

	public string CoreHitboxName { get; internal set; }

	public bool HasCoreDamageMultiplier { get; internal set; }

	public float CoreDamageMultiplier { get; internal set; } = 1f;

	public bool HasCoreOnHitAdditionalDamage { get; internal set; }

	public float CoreOnHitAdditionalDamageChance { get; internal set; }

	public float CoreOnHitAdditionalDamageMultiplier { get; internal set; } = 1f;

	public DamageAttribute CoreOnHitAdditionalDamageAttribute { get; internal set; }

	public string HitCountCooldownRefundTargetSkillId { get; internal set; }

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

	internal string PreparedSkillId { get; set; }

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

	internal ProjectileStatusHitSpec PreparedStatus { get; set; }

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

	internal ProjectileStatusHitSpec PreparedImpactStatus { get; set; }

	internal RuntimeSkillVisualSpec PreparedImpactRuntimeVisual { get; set; }

	internal SkillTargetingSpec PreparedImpactTargeting { get; set; }

	internal bool PreparedContactDamageEnabled { get; set; }

	internal bool PreparedStopOnFirstHit { get; set; }

	internal float PreparedImpactDelay { get; set; }

	internal bool PreparedHasImpactArea { get; set; }

	internal float PreparedImpactRadius { get; set; }

	internal float PreparedImpactDamage { get; set; }

	internal GameObject PreparedSkillEffectPrefab { get; set; }

	internal bool PreparedUsePrefabHitbox { get; set; }

	internal bool PreparedUsesHitTargetCount { get; set; }

	internal bool PreparedUsesResolvedDeployments { get; set; }

	internal bool PreparedPrefabHitboxAtOrigin { get; set; }

	internal float PreparedDamageDelay { get; set; }

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
		}
	}

	public UnitCombatState Owner { get; internal set; }

	public SkillDefinition Data => Source;

	public SkillSlot Slot => Source != null ? Source.Slot : default;

	/// 실행 주체와 정의를 연결해 사용 상태를 만든다.
	public SkillExecutionData(UnitCombatState owner, SkillDefinition source)
		: this(source)
	{
		Owner = owner;
		ResetRuntimeState();
	}

/// 스킬 정의와 소유자를 연결하고 초기 사용 상태를 준비한다.


    public float CooldownRemaining { get; private set; }
    public float CastRemaining { get; private set; }
    public float ActiveDurationRemaining { get; private set; }
    public float TickRemaining { get; private set; }
    public float ReloadRemaining { get; private set; }
    public int MagazineRemaining { get; private set; }
    public int ProjectileLaunchCount { get; private set; }
    public int SkillHitCount { get; private set; }
    internal SkillExecutionData ActiveExecutionData { get; private set; }

    private int effectiveMaxMagazineSize;
    private int effectiveBurstProjectileCount;
    private float effectiveReloadDuration;
    private float effectiveTickInterval;
    private float effectiveBurstInterval;
    private float effectiveCooldownDuration;
    private int queuedBurstShotsRemaining;
    private string consecutiveHitTargetUnitId;
    private int consecutiveHitRepeatCount;

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

    public bool CanCast => CanCastWithData(null);

    /// 모든 사용 제한과 누적 횟수를 초기 상태로 되돌린다.
    public void ResetRuntimeState()
    {
        effectiveMaxMagazineSize = CalculateMaxMagazineSize(Data);
        effectiveBurstProjectileCount = BurstProjectileCount(Data);
        effectiveReloadDuration = CalculateReloadDuration(Data);
        effectiveTickInterval = TickInterval(Data);
        effectiveBurstInterval = BurstInterval(Data);
        effectiveCooldownDuration = CooldownDuration(Data);
        CooldownRemaining = 0f;
        CastRemaining = 0f;
        ActiveDurationRemaining = 0f;
        TickRemaining = 0f;
        ReloadRemaining = 0f;
        MagazineRemaining = MaxMagazineSize;
        queuedBurstShotsRemaining = 0;
        ProjectileLaunchCount = 0;
        SkillHitCount = 0;
        ActiveExecutionData = null;
        consecutiveHitTargetUnitId = string.Empty;
        consecutiveHitRepeatCount = 0;
    }

    /// 발사 순번을 순환 범위 안에서 한 단계 진행한다.
    public int AdvanceProjectileLaunchCount()
    {
        if (ProjectileLaunchCount == int.MaxValue)
        {
            ProjectileLaunchCount = 0;
        }

        ProjectileLaunchCount++;
        return ProjectileLaunchCount;
    }

    /// 적중 순번을 순환 범위 안에서 한 단계 진행한다.
    public int AdvanceSkillHitCount()
    {
        if (SkillHitCount == int.MaxValue)
        {
            SkillHitCount = 0;
        }

        SkillHitCount++;
        return SkillHitCount;
    }

    /// 같은 대상을 연속 적중한 횟수에 따라 피해 배율을 계산한다.
    public float ConsecutiveHitDamageMultiplier(UnitCombatState target, SkillExecutionData snapshot)
    {
        if (target == null)
        {
            return 1f;
        }

        var projectileData = Data as ProjectileSkillDefinition;
        var bonusRate = 0f;
        var bonusMax = 0f;
        if (projectileData != null)
        {
            bonusRate = projectileData.ConsecutiveHitBonusRate;
            bonusMax = projectileData.ConsecutiveHitMax;
        }
        if (snapshot != null && snapshot.ConsecutiveHitBonusRate > 0f)
        {
            bonusRate = snapshot.ConsecutiveHitBonusRate;
        }
        if (snapshot != null && snapshot.ConsecutiveHitMax > 0f)
        {
            bonusMax = snapshot.ConsecutiveHitMax;
        }
        if (bonusRate <= 0f || bonusMax <= 0f)
        {
            return 1f;
        }

        var unitId = string.Empty;
        if (target.Identity != null)
        {
            unitId = target.Identity.UnitId;
        }
        if (string.IsNullOrWhiteSpace(unitId))
        {
            consecutiveHitTargetUnitId = string.Empty;
            consecutiveHitRepeatCount = 0;
            return 1f;
        }

        if (string.Equals(consecutiveHitTargetUnitId, unitId, StringComparison.Ordinal))
        {
            consecutiveHitRepeatCount = Math.Min(consecutiveHitRepeatCount + 1, int.MaxValue - 1);
        }
        else
        {
            consecutiveHitTargetUnitId = unitId;
            consecutiveHitRepeatCount = 0;
        }

        var bonus = Mathf.Min(
            Mathf.Max(0f, bonusMax),
            Mathf.Max(0f, bonusRate) * consecutiveHitRepeatCount);
        return 1f + bonus;
    }

    /// 시간 흐름에 따라 시전, 재사용, 재장전과 활성 대기값을 감소시킨다.
    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        var actionDeltaTime = deltaTime * StatusCombatRules.ActionSpeedMultiplier(Owner);
        CooldownRemaining = TickDown(CooldownRemaining, actionDeltaTime);
        CastRemaining = TickDown(CastRemaining, actionDeltaTime);
        ActiveDurationRemaining = TickDown(ActiveDurationRemaining, deltaTime);
        TickRemaining = TickDown(TickRemaining, actionDeltaTime);
        ReloadRemaining = TickDown(ReloadRemaining, deltaTime);

        if (UsesMagazine
            && MagazineRemaining <= 0
            && ReloadRemaining <= 0f
            && CooldownRemaining <= 0f
            && !IsBursting)
        {
            MagazineRemaining = MaxMagazineSize;
        }
    }

    /// 현재 보정값과 사용 제한을 반영해 시전 가능 여부를 판단한다.
    public bool CanCastWithData(SkillExecutionData snapshot)
    {
        RefreshRuntimeModifiers(snapshot);
        if (Data == null
            || !Data.IsActive
            || IsCasting
            || !IsCastIntervalReady())
        {
            return false;
        }

        if (IsBursting)
        {
            return !IsReloading;
        }

        return CooldownRemaining <= 0f
            && !IsReloading
            && HasMagazine;
    }

    /// 기본 보정값으로 시전 상태 진입을 시도한다.
    public bool TryBeginCast()
    {
        return TryBeginCast(null);
    }

    /// 확정된 보정값을 반영해 탄약과 대기 상태를 소비한다.
    public bool TryBeginCast(SkillExecutionData snapshot)
    {
        RefreshRuntimeModifiers(snapshot);
        if (IsBursting)
        {
            queuedBurstShotsRemaining = Math.Max(0, queuedBurstShotsRemaining - 1);
            if (IsBursting)
            {
                TickRemaining = effectiveBurstInterval;
            }
            else
            {
                TickRemaining = effectiveTickInterval;
                BeginRecoveryIfNeeded();
            }

            ActiveExecutionData = snapshot;
            return true;
        }

        if (!CanCastWithData(snapshot))
        {
            return false;
        }


        if (UsesMagazine)
        {
            MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
        }

        var timing = Data.Timing;
        ActiveDurationRemaining = Mathf.Max(0f, timing.ActiveDuration);
        queuedBurstShotsRemaining = Math.Max(0, effectiveBurstProjectileCount - 1);
        TickRemaining = effectiveTickInterval;
        if (IsBursting)
        {
            TickRemaining = effectiveBurstInterval;
        }

        if (!IsBursting)
        {
            BeginRecoveryIfNeeded();
        }

        ActiveExecutionData = snapshot;
        return true;
    }

    /// 진행 중인 지속 실행과 해당 실행 데이터를 종료한다.
    public void StopActive()
    {
        ActiveDurationRemaining = 0f;
        ActiveExecutionData = null;
    }

    /// 주기 효과를 다시 실행할 시점인지 확인한다.
    public bool IsTickReady()
    {
        return Data.Timing.TickInterval > 0f && TickRemaining <= 0f;
    }

    /// 다음 주기 실행까지의 대기시간을 다시 설정한다.
    public void ResetTickInterval()
    {
        TickRemaining = effectiveTickInterval;
    }

    /// 현재 연사 묶음에서 발사할 탄환 순번을 계산한다.
    public int CurrentBurstProjectileIndex()
    {
        if (effectiveBurstProjectileCount <= 1 || !IsBursting)
        {
            return 1;
        }

        return Mathf.Clamp(
            effectiveBurstProjectileCount - queuedBurstShotsRemaining + 1,
            1,
            effectiveBurstProjectileCount);
    }

    /// 재장전 대기시간을 줄이고 완료 시 탄창을 복구한다.
    public bool ReduceReloadRemaining(float seconds)
    {
        if (seconds <= 0f || ReloadRemaining <= 0f)
        {
            return false;
        }

        ReloadRemaining = Mathf.Max(0f, ReloadRemaining - seconds);
        if (ReloadRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && CooldownRemaining <= 0f && !IsBursting)
        {
            MagazineRemaining = MaxMagazineSize;
        }

        return true;
    }

    /// 재사용 대기시간을 줄이고 사용 가능 상태를 복구한다.
    public bool ReduceCooldownRemaining(float seconds)
    {
        if (seconds <= 0f || CooldownRemaining <= 0f)
        {
            return false;
        }

        CooldownRemaining = Mathf.Max(0f, CooldownRemaining - seconds);
        if (CooldownRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
        {
            MagazineRemaining = MaxMagazineSize;
        }

        return true;
    }

    /// 재사용 대기를 즉시 끝내고 필요한 탄창을 복구한다.
    public void ResetCooldown()
    {
        CooldownRemaining = 0f;
        if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
        {
            MagazineRemaining = MaxMagazineSize;
        }
    }

    /// 남은 시간을 0 아래로 내려가지 않게 감소시킨다.
    private static float TickDown(float value, float deltaTime)
    {
        if (value > 0f)
        {
            return Mathf.Max(0f, value - deltaTime);
        }

        return 0f;
    }

    /// 다음 발사 간격이 지났는지 확인한다.
    private bool IsCastIntervalReady()
    {
        return effectiveTickInterval <= 0f || TickRemaining <= 0f;
    }

    /// 이번 실행 보정에 맞춰 탄창, 연사와 대기시간 기준값을 갱신한다.
    private void RefreshRuntimeModifiers(SkillExecutionData snapshot)
    {
        var previousMax = effectiveMaxMagazineSize;
        var nextMax = CalculateMaxMagazineSize(Data);
        var nextBurst = BurstProjectileCount(Data);
        effectiveReloadDuration = CalculateReloadDuration(Data);
        effectiveTickInterval = TickInterval(Data);
        effectiveBurstInterval = BurstInterval(Data);
        effectiveCooldownDuration = CooldownDuration(Data);

        if (snapshot != null)
        {
            nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
            if (nextBurst > 1)
            {
                nextBurst += snapshot.AdditionalProjectileBonus;
            }

            effectiveReloadDuration *= Mathf.Max(0f, snapshot.ReloadTimeMultiplier);
            effectiveTickInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
            effectiveBurstInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
            effectiveCooldownDuration *= Mathf.Max(0f, snapshot.CooldownMultiplier);
        }

        effectiveMaxMagazineSize = nextMax;
        effectiveBurstProjectileCount = Math.Max(1, nextBurst);
        if (previousMax == effectiveMaxMagazineSize)
        {
            return;
        }

        if (effectiveMaxMagazineSize <= 0)
        {
            MagazineRemaining = 0;
            ReloadRemaining = 0f;
            return;
        }

        if (previousMax <= 0)
        {
            MagazineRemaining = effectiveMaxMagazineSize;
            return;
        }

        var delta = effectiveMaxMagazineSize - previousMax;
        MagazineRemaining = Mathf.Clamp(MagazineRemaining + delta, 0, effectiveMaxMagazineSize);
        if (MagazineRemaining > 0)
        {
            ReloadRemaining = 0f;
        }
    }

    /// 정의된 탄창 용량을 유효한 범위로 보정한다.
    private static int CalculateMaxMagazineSize(SkillDefinition data)
    {
        return Math.Max(0, data.MagazineCapacity);
    }

    /// 한 번의 시전에서 이어지는 발사 횟수를 구한다.
    private static int BurstProjectileCount(SkillDefinition data)
    {
        var projectile = data as ProjectileSkillDefinition;
        if (projectile != null && projectile.Projectile != null)
        {
            return Math.Max(1, projectile.Projectile.BurstProjectileCount);
        }

        return 1;
    }

    /// 재장전에 필요한 시간을 유효한 범위로 보정한다.
    private static float CalculateReloadDuration(SkillDefinition data)
    {
        return Mathf.Max(0f, data.ReloadSeconds);
    }

    /// 연속 실행 사이의 간격을 유효한 범위로 보정한다.
    private static float TickInterval(SkillDefinition data)
    {
        return Mathf.Max(0f, data.Timing.TickInterval);
    }

    /// 연사 간격을 구하고 별도 값이 없으면 기본 실행 간격을 사용한다.
    private static float BurstInterval(SkillDefinition data)
    {
        var projectile = data as ProjectileSkillDefinition;
        if (projectile != null && projectile.Projectile != null)
        {
            var burstInterval = projectile.Projectile.BurstIntervalSeconds;
            if (burstInterval > 0f)
            {
                return burstInterval;
            }
        }

        return TickInterval(data);
    }

    /// 재사용 대기시간을 유효한 범위로 보정한다.
    private static float CooldownDuration(SkillDefinition data)
    {
        return Mathf.Max(0f, data.Timing.Cooldown);
    }

    /// 탄창 소모 상태에 맞춰 재사용 또는 재장전을 시작한다.
    private void BeginRecoveryIfNeeded()
    {
        if (!UsesMagazine)
        {
            CooldownRemaining = effectiveCooldownDuration;
            return;
        }

        if (MagazineRemaining > 0)
        {
            return;
        }

        CooldownRemaining = effectiveCooldownDuration;
        if (ReloadDuration > 0f)
        {
            ReloadRemaining = ReloadDuration;
            return;
        }

        if (CooldownRemaining <= 0f)
        {
            MagazineRemaining = MaxMagazineSize;
        }
    }

	/// 실행 배율을 현재 값에 반영한다.
	internal void ApplyDynamicDamageMultiplier(float multiplier)
	{
		DamageMultiplier += PositiveOrDefault(multiplier, 1f) - 1f;
	}

	/// 외부 사건의 배율을 현재 값에 곱한다.
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

	/// 배율만 바꾼 실행값 사본을 만든다.
	internal SkillExecutionData CopyWithDamageMultiplier(float multiplier)
	{
		var copy = (SkillExecutionData)MemberwiseClone();
		copy.DamageMultiplier += Mathf.Max(0f, multiplier) - 1f;
		return copy;
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
