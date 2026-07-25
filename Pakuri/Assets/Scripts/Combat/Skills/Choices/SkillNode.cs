using Pakuri.Combat;
using Pakuri.Data;

/*
 * GameDataCatalogBuilder가 만든 SkillNodeDefinition을 SkillNodeMapper가 전투용 값으로 바꾼다.
 * SkillNode는 값 하나를 보관하고, SkillExecutionData가 필요한 값을 읽어서 최종 스킬을 만들고 Executor 에 넘겨준다.
 즉 언제든 꺼내 쓸 수 있는 실행 가능한 규칙 값 보관
 */
namespace Pakuri.InGame
{
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
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

    /* 상태 종류와 필요한 최소 중첩 수를 함께 보관한다. */
    public readonly struct StatusStackCondition
    {
        public StatusStackCondition(StatusEffectKind statusKind, int minimumStacks)
        {
            StatusKind = statusKind;
            MinimumStacks = minimumStacks;
        }

        public StatusEffectKind StatusKind { get; }
        public int MinimumStacks { get; }
    }

    /* 단일 공격의 체력 비율 시전 조건에 더할 값을 보관한다. */
    public readonly struct CastConditionOp
    {
        public CastConditionOp(float targetHealthRatioBonus /* 대상 체력 비율 추가값 */)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    /* 보스·처형 조건에 적용할 피해 배율을 보관한다. */
    public readonly struct DamageModifierOp
    {
        public DamageModifierOp(DamageModifierOpKind kind /* 처리할 종류 */, float multiplier /* 값에 곱할 배율 */)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    /* 조건을 만족했을 때 더할 치명타 확률을 보관한다. */
    public readonly struct CritModifierOp
    {
        public CritModifierOp(float chanceBonus /* 확률 추가값 */)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    /* 처치 후 재사용 대기시간을 초기화하거나 일부 돌려주는 규칙을 보관한다. */
    public readonly struct KillActionOp
    {
        public KillActionOp(KillActionOpKind kind /* 처리할 종류 */, float ratioBonus /* 비율 추가값 */, bool requiresExecute /* 필요 처형 여부 */)
        {
            Kind = kind;
            RatioBonus = ratioBonus;
            RequiresExecute = requiresExecute;
        }

        public KillActionOpKind Kind { get; }
        public float RatioBonus { get; }
        public bool RequiresExecute { get; }
    }

    /*
     * 단순 강화 하나의 종류와 값을 보관한다.
     * 강화 종류에 따라 Amount, Count, ReferenceId 중 필요한 값만 사용한다.
     */
    public readonly struct SkillActionOp
    {
        /* 소수 배율이나 보너스만 필요한 행동을 만든다. */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            float amount /* 적용할 수치 */)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = string.Empty;
        }

        /* 탄창, 관통, 투사체 수처럼 정수 개수만 필요한 행동을 만든다. */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            int count /* 적용할 개수 */)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = string.Empty;
        }

        /* 특정 상태나 트리거 ID에 연결된 소수 보너스 행동을 만든다. */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            string referenceId /* 참조할 데이터 식별자 */,
            float amount /* 적용할 수치 */)
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

        /* 특정 상태 ID에 귀속되는 정수 보너스 행동을 만든다. */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            string referenceId /* 참조할 데이터 식별자 */,
            int count /* 적용할 개수 */)
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

    /*
     * 연속 적중 횟수에 따라 증가하는 피해 값을 보관한다.
     */
    public readonly struct ConsecutiveHitActionOp
    {
        public ConsecutiveHitActionOp(
            float bonusRate /* 적중당 피해 증가율 */,
            float maxBonus /* 최대 피해 증가율 */)
        {
            BonusRate = bonusRate;
            MaxBonus = maxBonus;
        }

        public float BonusRate { get; }
        public float MaxBonus { get; }
    }

    /*
     * 공격이 다른 대상으로 분기될 때 필요한 값을 보관한다.
     */
    public readonly struct BranchDamageActionOp
    {
        public BranchDamageActionOp(
            float chanceBonus /* 분기 확률 추가값 */,
            int branchCount /* 분기 횟수 */,
            float damageMultiplier /* 분기 피해 배율 */,
            float searchRadius /* 다음 대상을 찾을 반경 */)
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

    /*
     * 대상의 상태 효과 조건에 따라 적용할 피해 배율을 보관한다.
     */
    public readonly struct ConditionalDamageActionOp
    {
        public ConditionalDamageActionOp(
            float damageMultiplier /* 조건을 만족했을 때 적용할 피해 배율 */,
            StatusEffectKind requiredStatus /* 대상에게 필요한 상태 효과 */,
            int minimumStacks /* 필요한 최소 중첩 수 */)
        {
            DamageMultiplier = damageMultiplier;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float DamageMultiplier { get; }
        public StatusStackCondition Condition { get; }
    }

    /* 대상 상태 중첩 조건을 만족할 때 더할 치명타 확률을 보관한다. */
    public readonly struct ConditionalCritChanceActionOp
    {
        public ConditionalCritChanceActionOp(float chanceBonus, StatusEffectKind requiredStatus, int minimumStacks)
        {
            ChanceBonus = chanceBonus;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float ChanceBonus { get; }
        public StatusStackCondition Condition { get; }
    }

    /* 연속 발사 중 지정한 투사체 순서에 적용할 피해 배율을 보관한다. 순서 0은 마지막 투사체다. */
    public readonly struct BurstDamageActionOp
    {
        public BurstDamageActionOp(int projectileIndex, float damageMultiplier)
        {
            ProjectileIndex = projectileIndex;
            DamageMultiplier = damageMultiplier;
        }

        public int ProjectileIndex { get; }
        public float DamageMultiplier { get; }
    }

    /* 연속 발사 중 지정한 투사체 순서에 더할 상태 중첩 수를 보관한다. 순서 0은 마지막 투사체다. */
    public readonly struct BurstStatusActionOp
    {
        public BurstStatusActionOp(int projectileIndex, int stacksBonus)
        {
            ProjectileIndex = projectileIndex;
            StacksBonus = stacksBonus;
        }

        public int ProjectileIndex { get; }
        public int StacksBonus { get; }
    }

    /* 기본 발사 뒤 생성할 후속 투사체의 개수, 간격, 피해 배율을 보관한다. */
    public readonly struct FollowUpProjectileActionOp
    {
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

    /* 대상 상태가 임계 중첩에 도달했을 때 새로 적용할 상태를 보관한다. */
    public readonly struct ThresholdStatusActionOp
    {
        public ThresholdStatusActionOp(StatusEffectKind sourceStatus, int minimumStacks, StatusEffectKind appliedStatus)
        {
            Condition = new StatusStackCondition(sourceStatus, minimumStacks);
            AppliedStatus = appliedStatus;
        }

        public StatusStackCondition Condition { get; }
        public StatusEffectKind AppliedStatus { get; }
    }

    /* 같은 대상에게 예약할 반복 공격 횟수, 간격, 피해 배율을 보관한다. */
    public readonly struct RepeatPerTargetActionOp
    {
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

    /* 처치 시 소비한 상태 일부를 주변 대상에게 재분배하는 규칙을 보관한다. */
    public readonly struct RedistributeConsumedStatusActionOp
    {
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

    /* 적중 시 확률적으로 발생하는 추가 피해 규칙을 보관한다. */
    public readonly struct AdditionalDamageActionOp
    {
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

    /* 지정한 핵심 충돌 영역에 적용할 피해 배율을 보관한다. */
    public readonly struct CoreDamageActionOp
    {
        public CoreDamageActionOp(string hitboxName, float multiplier)
        {
            HitboxName = hitboxName ?? string.Empty;
            Multiplier = multiplier;
        }

        public string HitboxName { get; }
        public float Multiplier { get; }
    }

    /* 지정한 핵심 충돌 영역 적중 시 발생할 추가 피해 규칙을 보관한다. */
    public readonly struct CoreAdditionalDamageActionOp
    {
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

    /* 일정 적중 횟수마다 주변 대상으로 이어지는 연쇄 피해 규칙을 보관한다. */
    public readonly struct HitChainDamageActionOp
    {
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

    /* 한 번의 공격이 일정 수 이상 적중했을 때 대상 스킬의 쿨다운을 환급하는 규칙을 보관한다. */
    public readonly struct HitCountCooldownRefundActionOp
    {
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

    /* 적중마다 대상 스킬의 재장전 시간을 감소시키는 규칙을 보관한다. */
    public readonly struct ReloadReducePerHitActionOp
    {
        public ReloadReducePerHitActionOp(string targetSkillId, float secondsPerHit)
        {
            TargetSkillId = targetSkillId ?? string.Empty;
            SecondsPerHit = secondsPerHit;
        }

        public string TargetSkillId { get; }
        public float SecondsPerHit { get; }
    }

    /* 강화 선택지를 적용하기 전에 시전자가 만족해야 하는 상태 조건을 보관한다. */
    public readonly struct SourceStatusRequirementOp
    {
        public SourceStatusRequirementOp(StatusEffectKind statusKind, int minimumStacks)
        {
            Condition = new StatusStackCondition(statusKind, minimumStacks);
        }

        public StatusStackCondition Condition { get; }
    }

    /*
     * 지정된 진영의 상태 효과 개수에 따라 증가하는 피해 값을 보관한다.
     */
    public readonly struct CountStatusDamageActionOp
    {
        public CountStatusDamageActionOp(
            SkillMultiEffectTargetSide targetSide /* 상태 효과를 확인할 대상 진영 */,
            StatusEffectKind statusKind /* 셀 상태 효과 종류 */,
            float amountPerCount /* 상태 효과 하나당 피해 증가량 */,
            int maximumCount /* 피해 계산에 사용할 최대 개수 */)
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

    /*
     * 공격자에게 특정 상태 효과가 있을 때 받는 피해 증가값을 보관한다.
     */
    public readonly struct StatusConditionalDamageTakenActionOp
    {
        public StatusConditionalDamageTakenActionOp(
            float bonus /* 받는 피해 증가값 */,
            StatusEffectKind requiredSourceStatus /* 공격자에게 필요한 상태 효과 */)
        {
            Bonus = bonus;
            RequiredSourceStatus = requiredSourceStatus;
        }

        public float Bonus { get; }
        public StatusEffectKind RequiredSourceStatus { get; }
    }

    /*
     * 전투용 실행 값 하나를 보관한다.
     * SkillNodeMapper가 만들고 SkillExecutionData와 스킬 규칙 코드가 필요한 형식으로 꺼낸다.
     */
    public class SkillNode
    {
        private readonly object operation;

        // CSV Graph Handler에서 변환된 강타입 실행 값 하나를 보관하는 부분을 구현.
        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        /* 저장된 실행 값이 요청한 형식이면 반환한다. */
        internal T? GetOperation<T>() where T : struct
        {
            if (operation is T value)
            {
                return value;
            }

            return null;
        }
        /* 실행 값 하나를 SkillNode로 감싼다. */
        public static SkillNode FromOperation<T>(T op) where T : struct => new SkillNode(op);

    }
}
