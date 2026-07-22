using Pakuri.Combat;
using Pakuri.Data;

/*
 * 작성 데이터의 문자열 Handler와 Params를 전투 코드가 읽을 수 있는 강타입 실행 계획으로 바꾼다.
 *
 * CSV에서 만들어진 SkillNodeDefinition은 값이 모두 문자열이므로 그대로 전투 계산에 사용하지 않는다.
 * SkillNodeMapper가 Handler 종류에 맞는 아래 readonly struct를 만들고, SkillNode가 그중 정확히 하나를
 * 보관한다. 각 struct가 생성자에서 모든 값을 초기화하는 이유는 변환이 끝난 실행 계획을 불변 값으로
 * 유지하고, 시전 중 원본 작성 데이터가 바뀌어도 이미 만들어진 계산 규칙이 흔들리지 않게 하기 위함이다.
 *
 * SkillNode는 C#에서 구분 공용체(discriminated union)를 흉내 낸 형태다. 한 인스턴스에는 Action,
 * DamageModifier 같은 payload 하나만 들어가며 나머지 nullable payload는 비어 있다. 실제 수치 누적과
 * 조건 평가는 SkillExecutionData와 각 스킬 규칙이 담당하고, 이 파일은 실행 가능한 값의 형태만 정의한다.
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

    /*
     * 상태 종류와 최소 중첩 수를 함께 전달하는 공통 조건 값이다.
     * 조건을 사용하는 각 Op가 같은 두 필드를 반복 정의하지 않도록 한다. 최소 중첩의 유효성은 조립 단계에서
     * 검사한다. 일부 기존 Handler는 값이 없을 때 0으로 두어 해당 규칙을 비활성화하므로 여기서 1로 보정하지 않는다.
     */
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

    /*
     * 대상 체력 비율을 사용하는 시전 조건의 추가값을 보관한다.
     * SingleSkillRules가 기본 처형 기준과 합산해 실제 시전 가능 여부를 판정한다.
     */
    public readonly struct CastConditionOp
    {
        /*
         * 작성 데이터에서 파싱한 체력 비율 추가값을 불변 실행 값으로 확정한다.
         */
        public CastConditionOp(float targetHealthRatioBonus /* 대상 체력 비율 추가값 */)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    public readonly struct DamageModifierOp
    {
        /*
         * 피해 보정이 적용될 조건과 곱연산 배율을 함께 확정한다.
         * Kind가 BossMultiplier면 보스 대상, ExecuteMultiplier면 처형 조건 대상에만 적용된다.
         */
        public DamageModifierOp(DamageModifierOpKind kind /* 처리할 종류 */, float multiplier /* 값에 곱할 배율 */)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    public readonly struct CritModifierOp
    {
        /*
         * 처형 조건 등 별도 규칙이 만족됐을 때 더할 치명타 확률을 확정한다.
         */
        public CritModifierOp(float chanceBonus /* 확률 추가값 */)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    public readonly struct KillActionOp
    {
        /*
         * 처치 후 실행할 회복 종류와 회복량, 처형 요구 여부를 하나의 불변 규칙으로 확정한다.
         * CooldownReset은 RequiresExecute를 사용하고 CooldownRefundBonus는 RatioBonus를 사용한다.
         */
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
     * 대부분의 단순 강화 수치를 공통 형식으로 보관한다.
     *
     * Kind에 따라 Amount(float), Count(int), ReferenceId 중 필요한 값만 소비한다. 구조체는 모든 필드를
     * 항상 가져야 하므로 각 생성자는 사용하지 않는 필드를 0 또는 string.Empty로 명시 초기화한다.
     * 이 초기값은 실제 강화값이 아니라 "이 Kind에서는 사용하지 않는 payload"라는 sentinel이다.
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

        /* 특정 상태나 Trigger ID에 귀속되는 소수 보너스 행동을 만든다. */
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
        /*
         * 적중당 피해 증가율과 최대 증가율을 초기화한다.
         */
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
        /*
         * 분기 확률, 횟수, 피해 배율, 탐색 반경을 초기화한다.
         */
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
        /*
         * 피해 배율과 필요한 상태 효과 중첩 수를 초기화한다.
         */
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
        public StatusEffectKind RequiredStatus => Condition.StatusKind;
        public int MinimumStacks => Condition.MinimumStacks;
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

    /* 지정한 핵심 Hitbox에 적용할 피해 배율을 보관한다. */
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

    /* 지정한 핵심 Hitbox 적중 시 발생할 추가 피해 규칙을 보관한다. */
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
        /*
         * 셀 대상, 상태 효과, 개수당 증가량, 최대 계산 개수를 초기화한다.
         */
        public CountStatusDamageActionOp(
            SkillMultiEffectTargetSide targetSide /* 상태 효과를 셀 대상 진영 */,
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
        /*
         * 받는 피해 증가값과 공격자에게 필요한 상태 효과를 초기화한다.
         */
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
     * 실행 계획 payload 하나를 공통 목록에 넣기 위한 컨테이너다.
     * 단일 object 슬롯에는 readonly struct 하나만 저장한다. 실행 계획은 카탈로그 컴파일 때 한 번 생성되고
     * Choice에 캐시되므로 이때 발생하는 boxing은 시전 반복 비용이 아니다. nullable 필드를 Op 종류마다
     * 계속 추가하던 수동 union보다 새 Op 확장 시 중복 상태와 생성자 초기화를 만들지 않는다.
     */
    public class SkillNode
    {
        private readonly object operation;

        private SkillNode(object operation)
        {
            this.operation = operation;
        }

        public CastConditionOp? CastCondition => GetOperation<CastConditionOp>();
        public SkillActionOp? Action => GetOperation<SkillActionOp>();
        public DamageModifierOp? DamageModifier => GetOperation<DamageModifierOp>();
        public CritModifierOp? CritModifier => GetOperation<CritModifierOp>();
        public KillActionOp? KillAction => GetOperation<KillActionOp>();
        public ConsecutiveHitActionOp? ConsecutiveHitAction => GetOperation<ConsecutiveHitActionOp>();
        public BranchDamageActionOp? BranchDamageAction => GetOperation<BranchDamageActionOp>();
        public ConditionalDamageActionOp? ConditionalDamageAction => GetOperation<ConditionalDamageActionOp>();
        public ConditionalCritChanceActionOp? ConditionalCritChanceAction => GetOperation<ConditionalCritChanceActionOp>();
        public BurstDamageActionOp? BurstDamageAction => GetOperation<BurstDamageActionOp>();
        public BurstStatusActionOp? BurstStatusAction => GetOperation<BurstStatusActionOp>();
        public CountStatusDamageActionOp? CountStatusDamageAction => GetOperation<CountStatusDamageActionOp>();
        public StatusConditionalDamageTakenActionOp? StatusConditionalDamageTakenAction => GetOperation<StatusConditionalDamageTakenActionOp>();
        public FollowUpProjectileActionOp? FollowUpProjectileAction => GetOperation<FollowUpProjectileActionOp>();
        public ThresholdStatusActionOp? ThresholdStatusAction => GetOperation<ThresholdStatusActionOp>();
        public RepeatPerTargetActionOp? RepeatPerTargetAction => GetOperation<RepeatPerTargetActionOp>();
        public RedistributeConsumedStatusActionOp? RedistributeConsumedStatusAction => GetOperation<RedistributeConsumedStatusActionOp>();
        public AdditionalDamageActionOp? AdditionalDamageAction => GetOperation<AdditionalDamageActionOp>();
        public CoreDamageActionOp? CoreDamageAction => GetOperation<CoreDamageActionOp>();
        public CoreAdditionalDamageActionOp? CoreAdditionalDamageAction => GetOperation<CoreAdditionalDamageActionOp>();
        public HitChainDamageActionOp? HitChainDamageAction => GetOperation<HitChainDamageActionOp>();
        public HitCountCooldownRefundActionOp? HitCountCooldownRefundAction => GetOperation<HitCountCooldownRefundActionOp>();
        public ReloadReducePerHitActionOp? ReloadReducePerHitAction => GetOperation<ReloadReducePerHitActionOp>();
        public SourceStatusRequirementOp? SourceStatusRequirement => GetOperation<SourceStatusRequirementOp>();

        private T? GetOperation<T>() where T : struct
        {
            if (operation is T value)
            {
                return value;
            }

            return null;
        }

        /*
         * 시전 조건을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCastCondition(
            CastConditionOp op /* 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 피해 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromDamageModifier(
            DamageModifierOp op /* 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromAction(
            SkillActionOp op /* 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 연속 적중 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConsecutiveHitAction(
            ConsecutiveHitActionOp op /* 연속 적중 피해 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 분기 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromBranchDamageAction(
            BranchDamageActionOp op /* 분기 피해 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 상태 조건 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConditionalDamageAction(
            ConditionalDamageActionOp op /* 상태 조건 피해 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 상태 효과 개수 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCountStatusDamageAction(
            CountStatusDamageActionOp op /* 상태 효과 개수 피해 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 공격자 상태 조건 받는 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromStatusConditionalDamageTakenAction(
            StatusConditionalDamageTakenActionOp op /* 공격자 상태 조건 받는 피해 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 치명타 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCritModifier(
            CritModifierOp op /* 동작 */)
        {
            return new SkillNode(op);
        }

        /*
         * 처치 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromKillAction(
            KillActionOp op /* 동작 */)
        {
            return new SkillNode(op);
        }

        public static SkillNode FromOperation<T>(T op) where T : struct => new SkillNode(op);

    }
}
