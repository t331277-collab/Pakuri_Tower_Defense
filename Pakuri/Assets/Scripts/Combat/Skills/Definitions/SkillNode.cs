/*
 * 역할: 스킬 그래프 작업 계약.
 * 책임: 런타임 스킬 실행에서 사용하는 조건·배율·행동·후속 작업 값을 정의한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>DamageModifierOpKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    /// <summary><c>KillActionOpKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

    /// <summary><c>SkillActionOpKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillActionOpKind
    {
        DamageMultiplier,
        ShieldAmountMultiplier,
        CooldownMultiplier,
        MagazineBonus,
        ReloadTimeMultiplier,
        PierceBonus,
        RadiusMultiplier,
        RadiusBonus,
        DurationBonus,
        DurationMultiplier,
        DamageDelayMultiplier,
        AdditionalProjectileBonus,
        ShotIntervalMultiplier,
        StatusStackAmountBonus,
        StatusStackAmountSet,
        StatusMaxStacksBonus,
        TargetStatusStackDamageRateBonus,
        TriggerProcChanceBonus,
        HitTargetCountBonus,
        LineCastRepeatCountBonus,
        StatusActionSpeedBonus,
        StatusAttackPowerBonus,
        StatusAilmentResistanceBonus,
        StatusDamageBonusRate,
        StatusShieldReceivedBonus,
        StatusCriticalChanceBonus,
        StatusDamageTakenBonus,
        StatusFlatElementResistReduction,
        StatusDurationBonus,
        StatusElementDamageTakenBonus,
        StatusCriticalDamageTakenBonus,
        CritChanceBonus,
        CritDamageBonus,
        BeamWidthBonus,
        KnockbackDistanceMultiplier,
        TargetStatusStackDamageMultiplier,
        ConsumeTargetStatusRatioOverride
    }

    /// <summary><c>StatusStackCondition</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct StatusStackCondition
    {

        /// <summary><c>StatusStackCondition</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public StatusStackCondition(StatusEffectKind statusKind, int minimumStacks)
        {
            StatusKind = statusKind;
            MinimumStacks = minimumStacks;
        }

        public StatusEffectKind StatusKind { get; }
        public int MinimumStacks { get; }
    }

    /// <summary><c>CastConditionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct CastConditionOp
    {

        /// <summary><c>CastConditionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public CastConditionOp(float targetHealthRatioBonus)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    /// <summary><c>DamageModifierOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct DamageModifierOp
    {

        /// <summary><c>DamageModifierOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    /// <summary><c>CritModifierOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct CritModifierOp
    {

        /// <summary><c>CritModifierOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public CritModifierOp(float chanceBonus)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    /// <summary><c>KillActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct KillActionOp
    {

        /// <summary><c>KillActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public KillActionOp(KillActionOpKind kind, float ratioBonus, bool requiresExecute)
        {
            Kind = kind;
            RatioBonus = ratioBonus;
            RequiresExecute = requiresExecute;
        }

        public KillActionOpKind Kind { get; }
        public float RatioBonus { get; }
        public bool RequiresExecute { get; }
    }

    /// <summary><c>SkillActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct SkillActionOp
    {

        /// <summary><c>SkillActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SkillActionOp(
            SkillActionOpKind kind,
            float amount)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = string.Empty;
        }

        /// <summary><c>SkillActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SkillActionOp(
            SkillActionOpKind kind,
            int count)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = string.Empty;
        }

        /// <summary><c>SkillActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SkillActionOp(
            SkillActionOpKind kind,
            string referenceId,
            float amount)
        {
            if (referenceId == null)
            {
                referenceId = string.Empty;
            }

            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = referenceId;
        }

        /// <summary><c>SkillActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SkillActionOp(
            SkillActionOpKind kind,
            string referenceId,
            int count)
        {
            if (referenceId == null)
            {
                referenceId = string.Empty;
            }

            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = referenceId;
        }

        public SkillActionOpKind Kind { get; }
        public float Amount { get; }
        public int Count { get; }
        public string ReferenceId { get; }
    }

    /// <summary><c>ConsecutiveHitActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct ConsecutiveHitActionOp
    {

        /// <summary><c>ConsecutiveHitActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public ConsecutiveHitActionOp(
            float bonusRate,
            float maxBonus)
        {
            BonusRate = bonusRate;
            MaxBonus = maxBonus;
        }

        public float BonusRate { get; }
        public float MaxBonus { get; }
    }

    /// <summary><c>BranchDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct BranchDamageActionOp
    {

        /// <summary><c>BranchDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public BranchDamageActionOp(
            float chanceBonus,
            int branchCount,
            float damageMultiplier,
            float searchRadius)
        {
            ChanceBonus = chanceBonus;
            BranchCount = branchCount;
            DamageMultiplier = damageMultiplier;
            SearchRadius = searchRadius;
        }

        public float ChanceBonus { get; }
        public int BranchCount { get; }
        public float DamageMultiplier { get; }
        public float SearchRadius { get; }
    }

    /// <summary><c>ConditionalDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct ConditionalDamageActionOp
    {

        /// <summary><c>ConditionalDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public ConditionalDamageActionOp(
            float damageMultiplier,
            StatusEffectKind requiredStatus,
            int minimumStacks)
        {
            DamageMultiplier = damageMultiplier;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float DamageMultiplier { get; }
        public StatusStackCondition Condition { get; }
    }

    /// <summary><c>ConditionalCritChanceActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct ConditionalCritChanceActionOp
    {

        /// <summary><c>ConditionalCritChanceActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public ConditionalCritChanceActionOp(float chanceBonus, StatusEffectKind requiredStatus, int minimumStacks)
        {
            ChanceBonus = chanceBonus;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float ChanceBonus { get; }
        public StatusStackCondition Condition { get; }
    }

    /// <summary><c>BurstDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct BurstDamageActionOp
    {

        /// <summary><c>BurstDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public BurstDamageActionOp(int projectileIndex, float damageMultiplier)
        {
            ProjectileIndex = projectileIndex;
            DamageMultiplier = damageMultiplier;
        }

        public int ProjectileIndex { get; }
        public float DamageMultiplier { get; }
    }

    /// <summary><c>BurstStatusActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct BurstStatusActionOp
    {

        /// <summary><c>BurstStatusActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public BurstStatusActionOp(int projectileIndex, int stacksBonus)
        {
            ProjectileIndex = projectileIndex;
            StacksBonus = stacksBonus;
        }

        public int ProjectileIndex { get; }
        public int StacksBonus { get; }
    }

    /// <summary><c>FollowUpProjectileActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct FollowUpProjectileActionOp
    {

        /// <summary><c>FollowUpProjectileActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public FollowUpProjectileActionOp(int count, float delaySeconds, float damageMultiplier)
        {
            Count = count;
            DelaySeconds = delaySeconds;
            DamageMultiplier = damageMultiplier;
        }

        public int Count { get; }
        public float DelaySeconds { get; }
        public float DamageMultiplier { get; }
    }

    /// <summary><c>ThresholdStatusActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct ThresholdStatusActionOp
    {

        /// <summary><c>ThresholdStatusActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public ThresholdStatusActionOp(StatusEffectKind sourceStatus, int minimumStacks, StatusEffectKind appliedStatus)
        {
            Condition = new StatusStackCondition(sourceStatus, minimumStacks);
            AppliedStatus = appliedStatus;
        }

        public StatusStackCondition Condition { get; }
        public StatusEffectKind AppliedStatus { get; }
    }

    /// <summary><c>RepeatPerTargetActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct RepeatPerTargetActionOp
    {

        /// <summary><c>RepeatPerTargetActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public RepeatPerTargetActionOp(int count, float intervalSeconds, float damageMultiplier)
        {
            Count = count;
            IntervalSeconds = intervalSeconds;
            DamageMultiplier = damageMultiplier;
        }

        public int Count { get; }
        public float IntervalSeconds { get; }
        public float DamageMultiplier { get; }
    }

    /// <summary><c>RedistributeConsumedStatusActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct RedistributeConsumedStatusActionOp
    {

        /// <summary><c>RedistributeConsumedStatusActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public RedistributeConsumedStatusActionOp(float ratio, StatusEffectKind statusKind, float searchRadius, int targetCount)
        {
            Ratio = ratio;
            StatusKind = statusKind;
            SearchRadius = searchRadius;
            TargetCount = targetCount;
        }

        public float Ratio { get; }
        public StatusEffectKind StatusKind { get; }
        public float SearchRadius { get; }
        public int TargetCount { get; }
    }

    /// <summary><c>AdditionalDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct AdditionalDamageActionOp
    {

        /// <summary><c>AdditionalDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public AdditionalDamageActionOp(float chance, float multiplier, DamageAttribute attribute, string target)
        {
            Chance = chance;
            Multiplier = multiplier;
            Attribute = attribute;
            Target = target ?? string.Empty;
        }

        public float Chance { get; }
        public float Multiplier { get; }
        public DamageAttribute Attribute { get; }
        public string Target { get; }
    }

    /// <summary><c>CoreDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct CoreDamageActionOp
    {

        /// <summary><c>CoreDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public CoreDamageActionOp(string hitboxName, float multiplier)
        {
            HitboxName = hitboxName ?? string.Empty;
            Multiplier = multiplier;
        }

        public string HitboxName { get; }
        public float Multiplier { get; }
    }

    /// <summary><c>CoreAdditionalDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct CoreAdditionalDamageActionOp
    {

        /// <summary><c>CoreAdditionalDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public CoreAdditionalDamageActionOp(string hitboxName, float chance, float multiplier, DamageAttribute attribute)
        {
            HitboxName = hitboxName ?? string.Empty;
            Chance = chance;
            Multiplier = multiplier;
            Attribute = attribute;
        }

        public string HitboxName { get; }
        public float Chance { get; }
        public float Multiplier { get; }
        public DamageAttribute Attribute { get; }
    }

    /// <summary><c>HitChainDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct HitChainDamageActionOp
    {

        /// <summary><c>HitChainDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public HitChainDamageActionOp(int hitPeriod, int targetCount, float searchRadius, float multiplier, DamageAttribute attribute)
        {
            HitPeriod = hitPeriod;
            TargetCount = targetCount;
            SearchRadius = searchRadius;
            Multiplier = multiplier;
            Attribute = attribute;
        }

        public int HitPeriod { get; }
        public int TargetCount { get; }
        public float SearchRadius { get; }
        public float Multiplier { get; }
        public DamageAttribute Attribute { get; }
    }

    /// <summary><c>HitCountCooldownRefundActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct HitCountCooldownRefundActionOp
    {

        /// <summary><c>HitCountCooldownRefundActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public HitCountCooldownRefundActionOp(string targetSkillId, int minimumTargets, float ratio)
        {
            TargetSkillId = targetSkillId ?? string.Empty;
            MinimumTargets = minimumTargets;
            Ratio = ratio;
        }

        public string TargetSkillId { get; }
        public int MinimumTargets { get; }
        public float Ratio { get; }
    }

    /// <summary><c>ReloadReducePerHitActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct ReloadReducePerHitActionOp
    {

        /// <summary><c>ReloadReducePerHitActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public ReloadReducePerHitActionOp(string targetSkillId, float secondsPerHit)
        {
            TargetSkillId = targetSkillId ?? string.Empty;
            SecondsPerHit = secondsPerHit;
        }

        public string TargetSkillId { get; }
        public float SecondsPerHit { get; }
    }

    /// <summary><c>SourceStatusRequirementOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct SourceStatusRequirementOp
    {

        /// <summary><c>SourceStatusRequirementOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public SourceStatusRequirementOp(StatusEffectKind statusKind, int minimumStacks)
        {
            Condition = new StatusStackCondition(statusKind, minimumStacks);
        }

        public StatusStackCondition Condition { get; }
    }

    /// <summary><c>CountStatusDamageActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct CountStatusDamageActionOp
    {

        /// <summary><c>CountStatusDamageActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public CountStatusDamageActionOp(
            SkillMultiEffectTargetSide targetSide,
            StatusEffectKind statusKind,
            float amountPerCount,
            int maximumCount)
        {
            TargetSide = targetSide;
            StatusKind = statusKind;
            AmountPerCount = amountPerCount;
            MaximumCount = maximumCount;
        }

        public SkillMultiEffectTargetSide TargetSide { get; }
        public StatusEffectKind StatusKind { get; }
        public float AmountPerCount { get; }
        public int MaximumCount { get; }
    }

    /// <summary><c>StatusConditionalDamageTakenActionOp</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
    public readonly struct StatusConditionalDamageTakenActionOp
    {

        /// <summary><c>StatusConditionalDamageTakenActionOp</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public StatusConditionalDamageTakenActionOp(
            float bonus,
            StatusEffectKind requiredSourceStatus)
        {
            Bonus = bonus;
            RequiredSourceStatus = requiredSourceStatus;
        }

        public float Bonus { get; }
        public StatusEffectKind RequiredSourceStatus { get; }
    }

    /// <summary><c>SkillNode</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    public class SkillNode
    {
        private readonly object operation;
        public string TargetSkillId { get; internal set; }

        /// <summary><c>SkillNode</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        /// <summary><c>Operation</c>를 반환한다.</summary>
        internal T? GetOperation<T>() where T : struct
        {
            if (operation is T value)
            {
                return value;
            }

            return null;
        }

        /// <summary>전달된 <c>op</c> 값을 사용해 <c>FromOperation</c> 결과값을 생성해 반환한다.</summary>
        public static SkillNode FromOperation<T>(T op) where T : struct => new SkillNode(op);

        /// <summary>전달된 런타임 입력값을 사용해 <c>FromOperation</c> 결과값을 생성해 반환한다.</summary>
        public static SkillNode FromOperation<T>(T op, string targetSkillId) where T : struct
        {
            return new SkillNode(op) { TargetSkillId = targetSkillId ?? string.Empty };
        }

    }
}
