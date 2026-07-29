using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * GameDataCatalogBuilder가 작성 Node를 전투용 강타입 값으로 바꾼다.
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

    /* 상태 종류와 필요한 최소 중첩 수를 함께 보관한다. */
    public readonly struct StatusStackCondition
    {
        /* 상태 조건 판정에 사용할 상태 종류와 최소 중첩을 묶는다. */
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
        /* 단일 공격의 기존 체력 비율 시전 조건에 더할 값을 기록한다. */
        public CastConditionOp(float targetHealthRatioBonus /* 대상 체력 비율 추가값 */)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    /* 보스·처형 조건에 적용할 피해 배율을 보관한다. */
    public readonly struct DamageModifierOp
    {
        /* 조건부 피해 보정의 종류와 적용 배율을 기록한다. */
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
        /* 조건을 만족했을 때 더할 치명타 확률을 기록한다. */
        public CritModifierOp(float chanceBonus /* 확률 추가값 */)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    /* 처치 후 재사용 대기시간을 초기화하거나 일부 돌려주는 규칙을 보관한다. */
    public readonly struct KillActionOp
    {
        /* 처치 후 실행할 쿨다운 행동과 필요한 조건값을 기록한다. */
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
        /* 같은 대상을 연속 적중할 때 누적할 피해 증가율과 상한을 기록한다. */
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
        /* 분기 공격의 확률, 개수, 피해 배율과 탐색 반경을 기록한다. */
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
        /* 대상 상태 중첩 조건과 조건 충족 시 피해 배율을 기록한다. */
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
        /* 대상 상태 중첩 조건과 조건 충족 시 치명타 확률 보너스를 기록한다. */
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
        /* 연속 발사 중 피해 배율을 적용할 투사체 순번을 기록한다. */
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
        /* 연속 발사 중 상태 중첩을 더할 투사체 순번을 기록한다. */
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
        /* 후속 투사체의 개수, 지연시간과 피해 배율을 기록한다. */
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
        /* 시전자 상태 임계치를 만족했을 때 적용할 상태를 기록한다. */
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
        /* 대상마다 반복할 횟수, 간격과 반복 피해 배율을 기록한다. */
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
        /* 처치 시 소비 상태를 주변 대상에게 재분배할 규칙을 기록한다. */
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
        /* 명중 시 추가 피해의 확률, 배율, 속성과 대상 방식을 기록한다. */
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
        /* 지정한 핵심 히트박스에 적용할 피해 배율을 기록한다. */
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
        /* 핵심 히트박스 명중 시 발생할 추가 피해 규칙을 기록한다. */
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
        /* 일정 명중 주기마다 발생할 연쇄 피해 규칙을 기록한다. */
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
        /* 요구 명중 대상 수를 달성했을 때 돌려줄 스킬 쿨다운 비율을 기록한다. */
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
        /* 명중마다 지정 스킬의 재장전 시간을 줄일 값을 기록한다. */
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
        /* Choice나 Node 실행에 필요한 시전자 상태와 최소 중첩을 기록한다. */
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
        /* 상태 수에 비례한 피해 계산에 사용할 상태 분류와 배율을 기록한다. */
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
        /* 공격자 상태 조건을 만족할 때 대상이 받을 피해 보너스를 기록한다. */
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

    public readonly struct ApplyDamageNodeOp
    {
        /* runtime 피해 Node가 계산에 사용할 피해식, 반경과 값 출처를 기록한다. */
        public ApplyDamageNodeOp(
            DamageAttribute attribute,
            float baseDamage,
            float attackPowerCoefficient,
            float spellPowerCoefficient,
            float damageMultiplier,
            float radius,
            float tickIntervalSeconds,
            NodeDamageValueSource valueSource,
            float valueSourceMultiplier,
            DamageAttribute trackedAttribute)
        {
            Attribute = attribute;
            BaseDamage = baseDamage;
            AttackPowerCoefficient = attackPowerCoefficient;
            SpellPowerCoefficient = spellPowerCoefficient;
            DamageMultiplier = damageMultiplier;
            Radius = radius;
            TickIntervalSeconds = tickIntervalSeconds;
            ValueSource = valueSource;
            ValueSourceMultiplier = valueSourceMultiplier;
            TrackedAttribute = trackedAttribute;
        }

        public DamageAttribute Attribute { get; }
        public float BaseDamage { get; }
        public float AttackPowerCoefficient { get; }
        public float SpellPowerCoefficient { get; }
        public float DamageMultiplier { get; }
        public float Radius { get; }
        public float TickIntervalSeconds { get; }
        public NodeDamageValueSource ValueSource { get; }
        public float ValueSourceMultiplier { get; }
        public DamageAttribute TrackedAttribute { get; }
    }

    public enum NodeDamageValueSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    public readonly struct ApplyStatusNodeOp
    {
        /* 적용할 상태 종류와 선택적인 대상·병합 정책을 기록한다. */
        public ApplyStatusNodeOp(
            StatusEffectKind statusKind,
            StatusTargetScope targetScope = StatusTargetScope.Unspecified,
            StatusMergePolicy mergePolicy = StatusMergePolicy.Unspecified)
        {
            StatusKind = statusKind;
            TargetScope = targetScope;
            MergePolicy = mergePolicy;
        }

        public StatusEffectKind StatusKind { get; }
        public StatusTargetScope TargetScope { get; }
        public StatusMergePolicy MergePolicy { get; }
    }

    public readonly struct ApplyShieldNodeOp
    {
        /* 보호막의 기본량과 주문력 계수를 기록한다. */
        public ApplyShieldNodeOp(float baseAmount, float spellPowerCoefficient)
        {
            BaseAmount = baseAmount;
            SpellPowerCoefficient = spellPowerCoefficient;
        }

        public float BaseAmount { get; }
        public float SpellPowerCoefficient { get; }
    }

    public enum StatusMutationKind
    {
        ActionSpeedBonus,
        MoveSpeedBonus,
        AttackPowerBonus,
        SpellPowerBonus,
        DamageBonusRate,
        ShieldReceivedBonus,
        CriticalChanceBonus,
        CriticalDamageBonus,
        CriticalResistanceBonus,
        DamageTakenBonus,
        ElementResistReduction,
        FlatElementResistReduction,
        ElementDamageTakenBonus,
        ConditionalStatusChanceBonus,
        RuntimeKindFilter,
        OutgoingAdditionalDamage
    }

    public readonly struct StatusMutationNodeOp
    {
        /* 상태 데이터에서 변경할 항목과 값·참조 속성을 기록한다. */
        public StatusMutationNodeOp(
            StatusMutationKind kind,
            float amount,
            DamageAttribute attribute,
            string referenceId = "",
            DamageAttribute secondaryAttribute = DamageAttribute.Physical,
            StatusEffectKind[] conditionalStatusKinds = null,
            SkillRuntimeKindCondition[] incomingRuntimeKinds = null,
            SkillRuntimeKindCondition[] outgoingRuntimeKinds = null)
        {
            Kind = kind;
            Amount = amount;
            Attribute = attribute;
            ReferenceId = referenceId ?? string.Empty;
            SecondaryAttribute = secondaryAttribute;
            ConditionalStatusKinds = conditionalStatusKinds ?? System.Array.Empty<StatusEffectKind>();
            IncomingRuntimeKinds = incomingRuntimeKinds ?? System.Array.Empty<SkillRuntimeKindCondition>();
            OutgoingRuntimeKinds = outgoingRuntimeKinds ?? System.Array.Empty<SkillRuntimeKindCondition>();
        }

        public StatusMutationKind Kind { get; }
        public float Amount { get; }
        public DamageAttribute Attribute { get; }
        public string ReferenceId { get; }
        public DamageAttribute SecondaryAttribute { get; }
        public StatusEffectKind[] ConditionalStatusKinds { get; }
        public SkillRuntimeKindCondition[] IncomingRuntimeKinds { get; }
        public SkillRuntimeKindCondition[] OutgoingRuntimeKinds { get; }
    }

    public readonly struct StatusConditionNodeOp
    {
        /* 상태 조건식, 검사 대상과 허용할 원본 스킬 목록을 기록한다. */
        public StatusConditionNodeOp(
            StatusConditionGroup[] conditions,
            SkillMultiEffectTargetSide targetSide,
            string[] sourceSkillIds)
        {
            Conditions = conditions ?? System.Array.Empty<StatusConditionGroup>();
            TargetSide = targetSide;
            SourceSkillIds = sourceSkillIds ?? System.Array.Empty<string>();
        }

        public StatusConditionGroup[] Conditions { get; }
        public SkillMultiEffectTargetSide TargetSide { get; }
        public string[] SourceSkillIds { get; }
    }

    public readonly struct SkillAttributeConditionNodeOp
    {
        /* 대상이 보유해야 할 액티브 스킬 속성을 기록한다. */
        public SkillAttributeConditionNodeOp(DamageAttribute attribute)
        {
            Attribute = attribute;
        }

        public DamageAttribute Attribute { get; }
    }

    public readonly struct HealthRatioConditionNodeOp
    {
        /* Node 실행을 허용할 대상의 최대 체력 비율을 기록한다. */
        public HealthRatioConditionNodeOp(float maximumRatio)
        {
            MaximumRatio = maximumRatio;
        }

        public float MaximumRatio { get; }
    }

    public readonly struct HitCountConditionNodeOp
    {
        /* Node 묶음 실행에 필요한 최소 명중 횟수를 기록한다. */
        public HitCountConditionNodeOp(int minimumHitCount)
        {
            MinimumHitCount = minimumHitCount;
        }

        public int MinimumHitCount { get; }
    }

    public readonly struct StatusPayloadNodeOp
    {
        /* 상태 적용 Node가 사용할 확률, 중첩, 지속시간과 갱신 규칙을 기록한다. */
        public StatusPayloadNodeOp(
            StatusEffectKind statusKind,
            float chance,
            int stacks,
            float durationSeconds,
            int maxStacks,
            bool refreshDuration)
        {
            StatusKind = statusKind;
            Chance = chance;
            Stacks = stacks;
            DurationSeconds = durationSeconds;
            MaxStacks = maxStacks;
            RefreshDuration = refreshDuration;
        }

        public StatusEffectKind StatusKind { get; }
        public float Chance { get; }
        public int Stacks { get; }
        public float DurationSeconds { get; }
        public int MaxStacks { get; }
        public bool RefreshDuration { get; }
    }

    public readonly struct ExtendStatusDurationNodeOp
    {
        /* 지속시간을 연장할 상태 종류를 기록한다. */
        public ExtendStatusDurationNodeOp(StatusEffectKind statusKind)
        {
            StatusKind = statusKind;
        }

        public StatusEffectKind StatusKind { get; }
    }

    public readonly struct SelectTargetsNodeOp
    {
        /* 뒤따르는 runtime 행동들이 공유할 대상 선택과 중심 규칙을 기록한다. */
        public SelectTargetsNodeOp(
            SkillMultiEffectTargetSide targetSide,
            SkillMultiEffectTargetSelection targetSelection,
            SkillMultiEffectTargetShape targetShape,
            SkillMultiEffectCenterMode centerMode,
            SkillMultiEffectVisualAnchorMode visualAnchorMode,
            bool applyOnce,
            bool coverAll,
            int maxTargets)
        {
            TargetSide = targetSide;
            TargetSelection = targetSelection;
            TargetShape = targetShape;
            CenterMode = centerMode;
            VisualAnchorMode = visualAnchorMode;
            ApplyOnce = applyOnce;
            CoverAll = coverAll;
            MaxTargets = maxTargets;
        }

        public SkillMultiEffectTargetSide TargetSide { get; }
        public SkillMultiEffectTargetSelection TargetSelection { get; }
        public SkillMultiEffectTargetShape TargetShape { get; }
        public SkillMultiEffectCenterMode CenterMode { get; }
        public SkillMultiEffectVisualAnchorMode VisualAnchorMode { get; }
        public bool ApplyOnce { get; }
        public bool CoverAll { get; }
        public int MaxTargets { get; }
    }

    public readonly struct SetDurationNodeOp
    {
        /* Visual·상태·연장 행동이 공유할 지속시간을 기록한다. */
        public SetDurationNodeOp(float durationSeconds)
        {
            DurationSeconds = durationSeconds;
        }

        public float DurationSeconds { get; }
    }

    public readonly struct RequireStatusNodeOp
    {
        /* 대상 또는 시전자에게 요구할 상태 종류와 최소 중첩을 기록한다. */
        public RequireStatusNodeOp(
            StatusEffectKind statusKind,
            SkillMultiEffectTargetSide targetSide,
            int minimumStacks)
        {
            StatusKind = statusKind;
            TargetSide = targetSide;
            MinimumStacks = minimumStacks;
        }

        public StatusEffectKind StatusKind { get; }
        public SkillMultiEffectTargetSide TargetSide { get; }
        public int MinimumStacks { get; }
    }

    public readonly struct ShowVisualNodeOp
    {
        /* Node 실행 시 표시할 프리팹 또는 runtime Visual 설정을 기록한다. */
        public ShowVisualNodeOp(GameObject prefab, RuntimeSkillVisualSpec runtimeVisual)
        {
            Prefab = prefab;
            RuntimeVisual = runtimeVisual;
        }

        public GameObject Prefab { get; }
        public RuntimeSkillVisualSpec RuntimeVisual { get; }
    }

    public readonly struct RecastZoneNodeOp
    {
        /* 장판 재시전의 지연, 지속시간, 피해 배율과 최대 세대를 기록한다. */
        public RecastZoneNodeOp(
            string sourceSkillId,
            float delaySeconds,
            float durationSeconds,
            float radiusMultiplier,
            bool inheritSnapshot,
            int maxGeneration)
        {
            SourceSkillId = sourceSkillId ?? string.Empty;
            DelaySeconds = delaySeconds;
            DurationSeconds = durationSeconds;
            RadiusMultiplier = radiusMultiplier;
            InheritSnapshot = inheritSnapshot;
            MaxGeneration = maxGeneration;
        }

        public string SourceSkillId { get; }
        public float DelaySeconds { get; }
        public float DurationSeconds { get; }
        public float RadiusMultiplier { get; }
        public bool InheritSnapshot { get; }
        public int MaxGeneration { get; }
    }

    public readonly struct ExecuteSkillNodeOp
    {
        /* Trigger가 실행할 다른 스킬 ID와 전달할 피해 배율을 기록한다. */
        public ExecuteSkillNodeOp(string skillId, float damageMultiplier)
        {
            SkillId = skillId ?? string.Empty;
            DamageMultiplier = damageMultiplier;
        }

        public string SkillId { get; }
        public float DamageMultiplier { get; }
    }

    public readonly struct RefundCooldownNodeOp
    {
        /* 지정 스킬에 반환할 쿨다운 비율을 기록한다. */
        public RefundCooldownNodeOp(string skillId, float ratio)
        {
            SkillId = skillId ?? string.Empty;
            Ratio = ratio;
        }

        public string SkillId { get; }
        public float Ratio { get; }
    }

    public readonly struct ReduceReloadNodeOp
    {
        /* 지정 스킬에서 감소시킬 재장전 비율을 기록한다. */
        public ReduceReloadNodeOp(string skillId, float ratio)
        {
            SkillId = skillId ?? string.Empty;
            Ratio = ratio;
        }

        public string SkillId { get; }
        public float Ratio { get; }
    }

    /*
     * 전투용 실행 값 하나를 보관한다.
     * GameDataCatalogBuilder가 만들고 SkillExecutionData와 스킬 규칙 코드가 필요한 형식으로 꺼낸다.
     */
    public class SkillNode
    {
        private readonly object operation;
        public string TargetSkillId { get; internal set; }

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

        /* 실행 값과 적용 대상 스킬을 SkillNode 하나로 만든다. */
        public static SkillNode FromOperation<T>(T op, string targetSkillId) where T : struct
        {
            return new SkillNode(op) { TargetSkillId = targetSkillId ?? string.Empty };
        }

    }
}
