using Pakuri.Data;

/*
 * 스킬 강화 노드의 조건, 수치 변경, 행동, 효과, Trigger를 정의한다.
 * 각 노드가 어떤 실행 정보를 담는지 나타내는 설계도만 보관한다.
 * 효과와 Trigger 목록의 조합 및 실행은 담당 실행 스크립트가 처리한다.
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
        StatusCriticalDamageTakenBonus
    }

    /*
     * 노드가 사용하는 조건과 수치 변경 값을 보관한다.
     */
    public readonly struct CastConditionOp
    {
        /*
         * CastConditionOp에 필요한 값을 초기화한다.
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
         * DamageModifierOp에 필요한 값을 초기화한다.
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
         * CritModifierOp에 필요한 값을 초기화한다.
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
         * KillActionOp에 필요한 값을 초기화한다.
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

    public readonly struct SkillActionOp
    {
        /*
         * 소수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            float amount /* 적용할 수치 */)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = string.Empty;
        }

        /*
         * 정수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            int count /* 적용할 개수 */)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = string.Empty;
        }

        /*
         * 참조 대상과 소수 값을 사용하는 단순 행동을 초기화한다.
         */
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

        /*
         * 참조 대상과 정수 값을 사용하는 단순 행동을 초기화한다.
         */
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
            RequiredStatus = requiredStatus;
            MinimumStacks = minimumStacks;
        }

        public float DamageMultiplier { get; }
        public StatusEffectKind RequiredStatus { get; }
        public int MinimumStacks { get; }
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
     * 스킬 실행 계획에 포함될 조건, 보정, 행동 노드를 보관한다.
     */
    public class SkillNode
    {
        /*
         * 스킬 실행 계획 노드에 필요한 값을 초기화한다.
         */
        private SkillNode(
            CastConditionOp? castCondition = null /* 스킬 사용 조건 */,
            SkillActionOp? action = null /* 동작 */,
            DamageModifierOp? damageModifier = null /* 피해 보정 */,
            CritModifierOp? critModifier = null /* 치명타 보정 */,
            KillActionOp? killAction = null /* 처치 동작 */,
            SkillEffectDefinition effect = null /* 스킬 효과 정의 */,
            SkillTriggerDefinition trigger = null /* 스킬 Trigger 정의 */,
            ConsecutiveHitActionOp? consecutiveHitAction = null /* 연속 적중 피해 동작 */,
            BranchDamageActionOp? branchDamageAction = null /* 분기 피해 동작 */,
            ConditionalDamageActionOp? conditionalDamageAction = null /* 상태 조건 피해 동작 */,
            CountStatusDamageActionOp? countStatusDamageAction = null /* 상태 효과 개수 피해 동작 */,
            StatusConditionalDamageTakenActionOp? statusConditionalDamageTakenAction = null /* 공격자 상태 조건 받는 피해 동작 */)
        {
            CastCondition = castCondition;
            Action = action;
            DamageModifier = damageModifier;
            CritModifier = critModifier;
            KillAction = killAction;
            Effect = effect;
            Trigger = trigger;
            ConsecutiveHitAction = consecutiveHitAction;
            BranchDamageAction = branchDamageAction;
            ConditionalDamageAction = conditionalDamageAction;
            CountStatusDamageAction = countStatusDamageAction;
            StatusConditionalDamageTakenAction = statusConditionalDamageTakenAction;
        }

        public CastConditionOp? CastCondition { get; }
        public SkillActionOp? Action { get; }
        public DamageModifierOp? DamageModifier { get; }
        public CritModifierOp? CritModifier { get; }
        public KillActionOp? KillAction { get; }
        public SkillEffectDefinition Effect { get; }
        public SkillTriggerDefinition Trigger { get; }
        public ConsecutiveHitActionOp? ConsecutiveHitAction { get; }
        public BranchDamageActionOp? BranchDamageAction { get; }
        public ConditionalDamageActionOp? ConditionalDamageAction { get; }
        public CountStatusDamageActionOp? CountStatusDamageAction { get; }
        public StatusConditionalDamageTakenActionOp? StatusConditionalDamageTakenAction { get; }

        /*
         * 시전 조건을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCastCondition(
            CastConditionOp op /* 동작 */)
        {
            return new SkillNode(
                castCondition: op);
        }

        /*
         * 피해 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromDamageModifier(
            DamageModifierOp op /* 동작 */)
        {
            return new SkillNode(
                damageModifier: op);
        }

        /*
         * 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromAction(
            SkillActionOp op /* 동작 */)
        {
            return new SkillNode(
                action: op);
        }

        /*
         * 연속 적중 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConsecutiveHitAction(
            ConsecutiveHitActionOp op /* 연속 적중 피해 동작 */)
        {
            return new SkillNode(
                consecutiveHitAction: op);
        }

        /*
         * 분기 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromBranchDamageAction(
            BranchDamageActionOp op /* 분기 피해 동작 */)
        {
            return new SkillNode(
                branchDamageAction: op);
        }

        /*
         * 상태 조건 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConditionalDamageAction(
            ConditionalDamageActionOp op /* 상태 조건 피해 동작 */)
        {
            return new SkillNode(
                conditionalDamageAction: op);
        }

        /*
         * 상태 효과 개수 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCountStatusDamageAction(
            CountStatusDamageActionOp op /* 상태 효과 개수 피해 동작 */)
        {
            return new SkillNode(
                countStatusDamageAction: op);
        }

        /*
         * 공격자 상태 조건 받는 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromStatusConditionalDamageTakenAction(
            StatusConditionalDamageTakenActionOp op /* 공격자 상태 조건 받는 피해 동작 */)
        {
            return new SkillNode(
                statusConditionalDamageTakenAction: op);
        }

        /*
         * 치명타 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCritModifier(
            CritModifierOp op /* 동작 */)
        {
            return new SkillNode(
                critModifier: op);
        }

        /*
         * 처치 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromKillAction(
            KillActionOp op /* 동작 */)
        {
            return new SkillNode(
                killAction: op);
        }

        /*
         * 효과를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromEffect(
            SkillEffectDefinition effect /* 실행하거나 변환할 효과 */)
        {
            return new SkillNode(
                effect: effect);
        }

        /*
         * 트리거를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromTrigger(
            SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
        {
            return new SkillNode(
                trigger: trigger);
        }

    }
}
