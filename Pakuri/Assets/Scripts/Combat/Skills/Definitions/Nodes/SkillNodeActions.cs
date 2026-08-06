/*
 * 역할: 스킬이 일으킬 변화의 설계값을 정의한다.
 * 책임: 피해, 상태, 후속 공격, 대기시간 변화가 실행될 조건과 수치를 제공한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 시전이나 지속 효과에서 이어질 실제 스킬 결과를 정의한다.
    public sealed class SkillCastEffect
    {
        public string EffectName;
        public float DelaySeconds;
        public SkillDefinition ResolvedDefinition;
        public float DamageMultiplier = 1f;
        public bool UseSourcePreparedAim;
        public bool UseSourcePreparedCenter;
        public StatusApplicationSpec OnHitStatusOverride;
        public SkillReactionCommand Command;
        public bool IsRecast;
        public float RadiusMultiplier = 1f;
        public float DurationSeconds;
        public bool InheritSnapshot = true;
        public int MaxGeneration = 1;
    }

    /// 후속 스킬 결과를 노드에서 해석할 값으로 전달한다.
    public readonly struct SkillCastEffectOp
    {
        /// 시전 뒤 이어질 결과를 하나의 규칙으로 고정한다.
        public SkillCastEffectOp(SkillCastEffect effect)
        {
            Effect = effect;
        }

        public SkillCastEffect Effect { get; }
    }

    /// 처치가 대기시간에 미치는 변화를 구분한다.
    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

    /// 일반 수치 변화가 영향을 줄 실행 항목을 구분한다.
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
        MagazineLastProjectileCritDamageBonus,
        FinalDamageModifier,
        CriticalFinalDamageModifier,
        BeamWidthBonus,
        KnockbackDistanceMultiplier,
        TargetStatusStackDamageMultiplier,
        ConsumeTargetStatusRatioOverride
    }

    /// 처치 뒤 적용할 대기시간 규칙을 나타낸다.
    public readonly struct KillActionOp
    {
        /// 처치 결과가 대기시간에 미칠 규칙을 고정한다.
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

    /// 하나의 수치 변화를 종류와 대상 기준으로 나타낸다.
    public readonly struct SkillActionOp
    {
        /// 배율과 시간처럼 연속값으로 표현되는 변화를 만든다.
        public SkillActionOp(SkillActionOpKind kind, float amount)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceName = string.Empty;
        }

        /// 발사 수와 중첩처럼 정수로 표현되는 변화를 만든다.
        public SkillActionOp(SkillActionOpKind kind, int count)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceName = string.Empty;
        }

        /// 특정 대상 규칙에 연결된 연속값 변화를 만든다.
        public SkillActionOp(SkillActionOpKind kind, string referenceName, float amount)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceName = referenceName ?? string.Empty;
        }

        /// 특정 대상 규칙에 연결된 횟수 변화를 만든다.
        public SkillActionOp(SkillActionOpKind kind, string referenceName, int count)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceName = referenceName ?? string.Empty;
        }

        public SkillActionOpKind Kind { get; }
        public float Amount { get; }
        public int Count { get; }
        public string ReferenceName { get; }
    }

    /// 같은 대상을 반복해서 맞힐 때 커질 피해를 나타낸다.
    public readonly struct ConsecutiveHitActionOp
    {
        /// 같은 대상을 거듭 맞힐수록 커지는 피해 규칙을 정의한다.
        public ConsecutiveHitActionOp(float bonusRate, float maxBonus)
        {
            BonusRate = bonusRate;
            MaxBonus = maxBonus;
        }

        public float BonusRate { get; }
        public float MaxBonus { get; }
    }

    /// 적중 뒤 주변 대상으로 이어질 피해를 나타낸다.
    public readonly struct BranchDamageActionOp
    {
        /// 적중 뒤 주변으로 이어지는 피해 규칙을 정의한다.
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

    /// 연사 순번에 따라 달라질 피해를 나타낸다.
    public readonly struct BurstDamageActionOp
    {
        /// 연사 순번에 따라 달라질 피해 규칙을 정의한다.
        public BurstDamageActionOp(int projectileIndex, float damageMultiplier)
        {
            ProjectileIndex = projectileIndex;
            DamageMultiplier = damageMultiplier;
        }

        public int ProjectileIndex { get; }
        public float DamageMultiplier { get; }
    }

    /// 연사 순번에 따라 달라질 상태 중첩을 나타낸다.
    public readonly struct BurstStatusActionOp
    {
        /// 연사 순번에 따라 달라질 상태 중첩 규칙을 정의한다.
        public BurstStatusActionOp(int projectileIndex, int stacksBonus)
        {
            ProjectileIndex = projectileIndex;
            StacksBonus = stacksBonus;
        }

        public int ProjectileIndex { get; }
        public int StacksBonus { get; }
    }

    /// 본 발사 뒤 이어질 투사체 계획을 나타낸다.
    public readonly struct FollowUpProjectileActionOp
    {
        /// 본 발사 뒤 이어질 투사체의 수와 시점을 정의한다.
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

    /// 상태 중첩이 다른 상태로 이어지는 기준을 나타낸다.
    public readonly struct ThresholdStatusActionOp
    {
        /// 충분한 상태 중첩이 다른 상태로 이어지는 조건을 정의한다.
        public ThresholdStatusActionOp(
            StatusEffectKind sourceStatus,
            int minimumStacks,
            StatusEffectKind appliedStatus)
        {
            Condition = new StatusStackCondition(sourceStatus, minimumStacks);
            AppliedStatus = appliedStatus;
        }

        public StatusStackCondition Condition { get; }
        public StatusEffectKind AppliedStatus { get; }
    }

    /// 같은 대상에 반복 적용할 방식과 피해를 나타낸다.
    public readonly struct RepeatPerTargetActionOp
    {
        /// 같은 대상에 다시 적용할 횟수와 간격을 정의한다.
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

    /// Zone 중심으로 대상을 일정 거리만큼 끌어당기는 규칙을 나타낸다.
    public readonly struct PullToCenterActionOp
    {
        public PullToCenterActionOp(float distancePerTick)
        {
            DistancePerTick = distancePerTick;
        }

        public float DistancePerTick { get; }
    }

    /// 소비한 상태를 처치 뒤 다시 나눌 방식을 나타낸다.
    public readonly struct RedistributeConsumedStatusActionOp
    {
        /// 처치 때 소비한 상태를 주변 대상에 나눌 규칙을 정의한다.
        public RedistributeConsumedStatusActionOp(
            float ratio,
            StatusEffectKind statusKind,
            float searchRadius,
            int targetCount)
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

    /// 적중 뒤 발생할 별도 피해의 조건을 나타낸다.
    public readonly struct AdditionalDamageActionOp
    {
        /// 적중 뒤 별도 피해가 발생할 조건과 속성을 정의한다.
        public AdditionalDamageActionOp(
            float chance,
            float multiplier,
            DamageAttribute attribute,
            string target)
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

    /// 특정 충돌 영역에 적용할 피해 보정을 나타낸다.
    public readonly struct CoreDamageActionOp
    {
        /// 특정 충돌 영역이 더 큰 피해로 이어지는 규칙을 정의한다.
        public CoreDamageActionOp(string hitboxName, float multiplier)
        {
            HitboxName = hitboxName ?? string.Empty;
            Multiplier = multiplier;
        }

        public string HitboxName { get; }
        public float Multiplier { get; }
    }

    /// 특정 충돌 영역에서 발생할 별도 피해를 나타낸다.
    public readonly struct CoreAdditionalDamageActionOp
    {
        /// 특정 충돌 영역에서 별도 피해가 발생할 조건을 정의한다.
        public CoreAdditionalDamageActionOp(
            string hitboxName,
            float chance,
            float multiplier,
            DamageAttribute attribute)
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

    /// 누적 적중이 주변 피해로 이어지는 방식을 나타낸다.
    public readonly struct HitChainDamageActionOp
    {
        /// 일정 적중마다 주변 대상으로 이어질 피해 규칙을 정의한다.
        public HitChainDamageActionOp(
            int hitPeriod,
            int targetCount,
            float searchRadius,
            float multiplier,
            DamageAttribute attribute)
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

    /// 다수 적중으로 돌려받을 대기시간을 나타낸다.
    public readonly struct HitCountCooldownRefundActionOp
    {
        /// 충분한 대상을 맞혔을 때 돌려받을 대기시간을 정의한다.
        public HitCountCooldownRefundActionOp(
            string targetSkillName,
            int minimumTargets,
            float ratio)
        {
            TargetSkillName = targetSkillName ?? string.Empty;
            MinimumTargets = minimumTargets;
            Ratio = ratio;
        }

        public string TargetSkillName { get; }
        public int MinimumTargets { get; }
        public float Ratio { get; }
    }

    /// 적중마다 줄어들 재장전 시간을 나타낸다.
    public readonly struct ReloadReducePerHitActionOp
    {
        /// 적중할 때마다 줄어들 재장전 시간을 정의한다.
        public ReloadReducePerHitActionOp(string targetSkillName, float secondsPerHit)
        {
            TargetSkillName = targetSkillName ?? string.Empty;
            SecondsPerHit = secondsPerHit;
        }

        public string TargetSkillName { get; }
        public float SecondsPerHit { get; }
    }

    /// 조건을 만족한 대상 수가 피해에 기여하는 방식을 나타낸다.
    public readonly struct CountStatusDamageActionOp
    {
        /// 조건을 만족한 대상 수가 피해에 기여하는 방식을 정의한다.
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
}
