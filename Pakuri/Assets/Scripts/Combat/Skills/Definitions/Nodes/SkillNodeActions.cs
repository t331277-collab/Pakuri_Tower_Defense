/*
 * 역할: 스킬 노드 행동 계약.
 * 책임: 피해·상태·후속타·재사용 대기시간 변경 행동값을 정의한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 일반 시전 또는 패시브 활성화에 함께 실행할 기존 family 효과값.
    public sealed class SkillCastEffect
    {
        public string EffectId;
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

    public readonly struct SkillCastEffectOp
    {
        /// 시전 후속 효과의 의미를 보관한다.
        public SkillCastEffectOp(SkillCastEffect effect)
        {
            Effect = effect;
        }

        public SkillCastEffect Effect { get; }
    }

    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

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

    public readonly struct KillActionOp
    {
        /// 처치 결과 보정의 의미를 보관한다.
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

    public readonly struct SkillActionOp
    {
        /// 수치형 스킬 행동의 의미를 보관한다.
        public SkillActionOp(SkillActionOpKind kind, float amount)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = string.Empty;
        }

        /// 횟수형 스킬 행동의 의미를 보관한다.
        public SkillActionOp(SkillActionOpKind kind, int count)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = string.Empty;
        }

        /// 대상과 수치를 가진 스킬 행동의 의미를 보관한다.
        public SkillActionOp(SkillActionOpKind kind, string referenceId, float amount)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = referenceId ?? string.Empty;
        }

        /// 대상과 횟수를 가진 스킬 행동의 의미를 보관한다.
        public SkillActionOp(SkillActionOpKind kind, string referenceId, int count)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = referenceId ?? string.Empty;
        }

        public SkillActionOpKind Kind { get; }
        public float Amount { get; }
        public int Count { get; }
        public string ReferenceId { get; }
    }

    public readonly struct ConsecutiveHitActionOp
    {
        /// 연속 적중 보정의 의미를 보관한다.
        public ConsecutiveHitActionOp(float bonusRate, float maxBonus)
        {
            BonusRate = bonusRate;
            MaxBonus = maxBonus;
        }

        public float BonusRate { get; }
        public float MaxBonus { get; }
    }

    public readonly struct BranchDamageActionOp
    {
        /// 분기 피해의 의미를 보관한다.
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

    public readonly struct BurstDamageActionOp
    {
        /// 폭발 피해의 의미를 보관한다.
        public BurstDamageActionOp(int projectileIndex, float damageMultiplier)
        {
            ProjectileIndex = projectileIndex;
            DamageMultiplier = damageMultiplier;
        }

        public int ProjectileIndex { get; }
        public float DamageMultiplier { get; }
    }

    public readonly struct BurstStatusActionOp
    {
        /// 폭발 상태 보정의 의미를 보관한다.
        public BurstStatusActionOp(int projectileIndex, int stacksBonus)
        {
            ProjectileIndex = projectileIndex;
            StacksBonus = stacksBonus;
        }

        public int ProjectileIndex { get; }
        public int StacksBonus { get; }
    }

    public readonly struct FollowUpProjectileActionOp
    {
        /// 후속 투사체의 의미를 보관한다.
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

    public readonly struct ThresholdStatusActionOp
    {
        /// 임계 상태 적용의 의미를 보관한다.
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

    public readonly struct RepeatPerTargetActionOp
    {
        /// 대상별 반복 실행의 의미를 보관한다.
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

    public readonly struct RedistributeConsumedStatusActionOp
    {
        /// 소비 상태 재분배의 의미를 보관한다.
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

    public readonly struct AdditionalDamageActionOp
    {
        /// 추가 피해의 의미를 보관한다.
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

    public readonly struct CoreDamageActionOp
    {
        /// 핵심 피해의 의미를 보관한다.
        public CoreDamageActionOp(string hitboxName, float multiplier)
        {
            HitboxName = hitboxName ?? string.Empty;
            Multiplier = multiplier;
        }

        public string HitboxName { get; }
        public float Multiplier { get; }
    }

    public readonly struct CoreAdditionalDamageActionOp
    {
        /// 핵심 적중 추가 피해의 의미를 보관한다.
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

    public readonly struct HitChainDamageActionOp
    {
        /// 연쇄 적중 피해의 의미를 보관한다.
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

    public readonly struct HitCountCooldownRefundActionOp
    {
        /// 적중 수 기반 대기 환급의 의미를 보관한다.
        public HitCountCooldownRefundActionOp(
            string targetSkillId,
            int minimumTargets,
            float ratio)
        {
            TargetSkillId = targetSkillId ?? string.Empty;
            MinimumTargets = minimumTargets;
            Ratio = ratio;
        }

        public string TargetSkillId { get; }
        public int MinimumTargets { get; }
        public float Ratio { get; }
    }

    public readonly struct ReloadReducePerHitActionOp
    {
        /// 적중당 재장전 감소의 의미를 보관한다.
        public ReloadReducePerHitActionOp(string targetSkillId, float secondsPerHit)
        {
            TargetSkillId = targetSkillId ?? string.Empty;
            SecondsPerHit = secondsPerHit;
        }

        public string TargetSkillId { get; }
        public float SecondsPerHit { get; }
    }

    public readonly struct CountStatusDamageActionOp
    {
        /// 상태 중첩 기반 피해의 의미를 보관한다.
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
