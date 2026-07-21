using System;
using Pakuri.Combat;
using UnityEngine;

/*
 * 몬스터가 보유할 수 있는 스킬 슬롯을 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillSlot
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    /*
     * 스킬 데이터가 런타임에서 지원되는 단계를 구분한다.
     */
    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    /*
     * 스킬을 실행할 공용 런타임 종류를 구분한다.
     */
    public enum SkillRuntimeKind
    {
        MagazineProjectile,
        CooldownProjectile,
        LineAttack,
        AreaAttack,
        SingleAttack,
        Field,
        Buff,
        Shield,
        Heal,
        Mark,
        Execute,
        Passive
    }

    /*
     * 액티브 스킬의 실행 종류, 피해, 투사체, 상태, 성장 선택지를 보관한다.
     */
    [Serializable]
    public class SkillDefinition
    {
        // 스킬 식별과 표시 정보
        public string SkillId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        [TextArea(2, 5)] public string DescriptionText;
        // 기본 피해와 대상 범위
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool UseCombinedStatCoefficients;
        public float Radius;
        public float CastRange;
        public float EffectRadius;
        public string TargetScope;
        // 특수 실행 프로필과 전용 수치
        public string ExecutionProfile;
        public float FlatValue;
        public float ProjectileLifetimeSeconds;
        public float IncomingDamageMultiplier = 1f;
        public float MoveSpeedMultiplier = 1f;
        public float OutgoingDamageMultiplier = 1f;
        public float ChainDamageMultiplier;
        public float ChainDelaySeconds;
        public float ChainRadius;
        public bool ExcludePrimaryTarget;
        public float TargetMaxHealthRatio;
        public float ChargeRampSeconds = 3f;
        public float ChargeMoveSpeedMultiplier = 2.5f;
        public float KnockbackDistance;
        public float DamageDelaySeconds;
        [Range(0f, 1f)] public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        [Range(0f, 1f)] public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;
        // 명중 대상 수와 대상 선택
        public string HitTargetCount;
        public bool UsePrefabHitbox;
        public string TargetSelection;
        public string TargetSelectionStatusId;
        public int TargetSelectionStatusMinStacks;
        // 재사용 대기시간과 투사체 동작
        public float CooldownSeconds;
        public float ActiveDurationSeconds;
        public int MagazineCapacity;
        public float ReloadSeconds;
        public float ShotIntervalSeconds;
        public float BurstIntervalSeconds;
        public int ProjectileBurstCount;
        public int BurstDamageProjectileIndex;
        public float BurstDamageMultiplier = 1f;
        public float ProjectileSpeed;
        public int PierceCount;
        public bool CriticalAllowed = true;
        // 대상 상태 중첩을 이용하는 공격 설정
        public string DeploymentRequiredTargetStatusId;
        public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        public int TargetStatusStackMaxStacks;
        public float TargetStatusStackBaseDamage;
        public float TargetStatusStackAttackPowerCoefficient;
        public float TargetStatusStackSpellPowerCoefficient;
        public string ConsumeTargetStatusId;
        [Range(0f, 1f)] public float ConsumeTargetStatusRatio;
        public int ConsumeTargetStatusStacks;
        // 스킬이 적용할 상태와 능력치 변경값
        public string StatusEffectId;
        [Range(0f, 1f)] public float StatusChance;
        public string StatusEffectLabel;
        public GameObject StatusEffectPrefab;
        public float StatusDurationSeconds;
        public int StatusMaxStacks;
        public int StatusStackAmount;
        public string StatusTargetScope;
        public string StatusMergePolicy;
        public string ShieldAmountRefreshPolicy;
        public float StatusActionSpeedBonus;
        public float StatusMoveSpeedBonus;
        public float StatusAttackPowerBonus;
        public float StatusSpellPowerBonus;
        public float StatusDamageBonusRate;
        public bool StatusPermanent;
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        // 성장 선택지와 추가 효과 그래프
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] MasterSkillChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }

    /*
     * 패시브 스킬의 요구 슬롯, 성장 선택지, 효과 그래프를 보관한다.
     */
    [Serializable]
    public class PassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] BaseModifierChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] PassiveEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }
}


/*
 * 선택지가 액티브·패시브의 어느 성장 단계인지 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }

    /*
     * 스킬 성장 선택지가 변경할 전투 수치와 조건을 보관한다.
     */
    [Serializable]
    public class SkillChoiceDefinition
    {
        // 선택지 식별과 표시 정보
        public string ChoiceId;
        public string MonsterId;
        public string SkillId;
        public string TargetSkillId;
        public string RuntimeTargetSkillIds;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        // 기본 피해, 재사용 대기시간, 탄창 변경
        public bool HasDamageMultiplier;
        public float DamageMultiplier = 1f;
        public float BaseDamageBonus;
        public bool HasCooldownMultiplier;
        public float CooldownMultiplier = 1f;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        // 투사체와 연속 발사 변경
        public int AdditionalProjectileBonus;
        public int PierceBonus;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasBurstDamageProjectileIndex;
        public int BurstDamageProjectileIndex;
        public bool HasBurstDamageMultiplier;
        public float BurstDamageMultiplier = 1f;
        public bool HasBurstStatusProjectileIndex;
        public int BurstStatusProjectileIndex;
        public int BurstStatusStacksBonus;
        public int FollowUpProjectileCount;
        public float FollowUpProjectileDelaySeconds;
        public float FollowUpProjectileDamageMultiplier = 1f;
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
        // 범위, 지속시간, 분기 공격 변경
        public bool HasRadiusMultiplier;
        public float RadiusMultiplier = 1f;
        public float RadiusBonus;
        public float BeamWidthBonus;
        public bool HasKnockbackDistanceMultiplier;
        public float KnockbackDistanceMultiplier = 1f;
        public bool HasDamageDelayMultiplier;
        public float DamageDelayMultiplier = 1f;
        public bool HasExecuteHealthRatioBonus;
        public float ExecuteHealthRatioBonus;
        public bool HasDurationMultiplier;
        public float DurationMultiplier = 1f;
        public float DurationBonus;
        public float BranchChanceBonus;
        public bool HasBranchChanceSet;
        public float BranchChanceSet;
        public bool HasBranchCount;
        public int BranchCount;
        public bool HasBranchDamageMultiplier;
        public float BranchDamageMultiplier = 1f;
        public bool HasBranchSearchRadius;
        public float BranchSearchRadius;
        public int BranchLaunchPeriod;
        public bool HasBranchLaunchChanceSet;
        public float BranchLaunchChanceSet;
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;
        public int HitTargetCountBonus;
        public float CritChanceBonus;
        public float CritDamageBonus;
        public float ExecuteCritChanceBonus;
        public bool HasBossDamageMultiplier;
        public float BossDamageMultiplier = 1f;
        public bool HasKillCooldownRefundRatioBonus;
        public float KillCooldownRefundRatioBonus;
        public bool KillResetsCooldown;
        public bool KillResetsCooldownRequiresExecute;
        // 상태 적용과 조건부 피해 변경
        public string StatusTag;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
        public bool HasStatusActionSpeedBonus;
        public float StatusActionSpeedBonus;
        public bool HasStatusAttackPowerBonus;
        public float StatusAttackPowerBonus;
        public int StatusStacksBonus;
        public bool HasStatusStacksSet;
        public int StatusStacksSet;
        public bool HasStatusElementDamageTakenBonus;
        public float StatusElementDamageTakenBonus;
        public bool HasStatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public bool HasStatusAilmentResistanceBonus;
        public float StatusAilmentResistanceBonus;
        public string StatusMaxStacksBonusStatusId;
        public int StatusMaxStacksBonus;
        public string StatusDurationBonusStatusId;
        public float StatusDurationBonus;
        public string ThresholdStatusId;
        public StatusEffectKind ThresholdStatusKind;
        public int ThresholdStatusMinStacks;
        public string ThresholdApplyStatusId;
        public StatusEffectKind ThresholdApplyStatusKind;
        public bool HasConditionalDamageMultiplier;
        public float ConditionalDamageMultiplier = 1f;
        public string ConditionalTargetStatusId;
        public int ConditionalTargetStatusMinStacks;
        public bool HasTargetStatusStackDamageMultiplier;
        public float TargetStatusStackDamageMultiplier = 1f;
        public bool HasConsumeTargetStatusRatioOverride;
        public float ConsumeTargetStatusRatioOverride;
        public bool HasConsumeTargetStatusStacksOverride;
        public int ConsumeTargetStatusStacksOverride;
        public float ConditionalCritChanceBonus;
        public string ConditionalCritTargetStatusId;
        public StatusEffectKind ConditionalCritTargetStatusKind;
        public int ConditionalCritTargetStatusMinStacks;
        public float RedistributeConsumedStatusRatioOnKill;
        public string RedistributeConsumedStatusId;
        public StatusEffectKind RedistributeConsumedStatusKind;
        public float RedistributeConsumedStatusSearchRadius;
        public int RedistributeConsumedStatusTargetCount;
        public string CountStatusId;
        public StatusEffectKind CountStatusKind;
        public SkillMultiEffectTargetSide CountTargetSide;
        public float DamageMultiplierPerCount;
        public int CountMax;
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;
        public bool HasStatusConditionalDamageTakenBonus;
        public float StatusConditionalDamageTakenBonus;
        public string StatusConditionalSourceStatusId;
        public StatusEffectKind StatusConditionalSourceStatusKind;
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        // 추가 타격, 연쇄 공격, 핵심 충돌 영역 변경
        public bool HasOnHitAdditionalDamage;
        public float OnHitAdditionalDamageChance;
        public float OnHitAdditionalDamageMultiplier = 1f;
        public DamageAttribute OnHitAdditionalDamageAttribute;
        public string OnHitAdditionalDamageTarget;
        public int OnHitChainHitPeriod;
        public int OnHitChainTargetCount;
        public float OnHitChainSearchRadius;
        public float OnHitChainDamageMultiplier = 1f;
        public DamageAttribute OnHitChainDamageAttribute;
        public string ReloadReduceTargetSkillId;
        public float ReloadReduceSecondsPerHit;
        public string CoreHitboxName;
        public bool HasCoreDamageMultiplier;
        public float CoreDamageMultiplier = 1f;
        public bool HasCoreOnHitAdditionalDamage;
        public float CoreOnHitAdditionalDamageChance;
        public float CoreOnHitAdditionalDamageMultiplier = 1f;
        public DamageAttribute CoreOnHitAdditionalDamageAttribute;
        public string HitCountCooldownRefundTargetSkillId;
        public int HitCountCooldownRefundMinTargets;
        public float HitCountCooldownRefundRatio;
        public int RepeatCountPerTarget;
        public float RepeatIntervalSeconds;
        public float RepeatDamageMultiplier = 1f;
        // 정규화 그래프와 런타임 지원 상태
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }
}


/*
 * 한 스킬에 연결되는 추가 효과의 동작을 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillMultiEffectKind
    {
        Damage,
        Status,
        ExtendStatusDuration,
        RecastZone
    }

    /*
     * 추가 효과가 적용될 진영을 구분한다.
     */
    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    /*
     * 추가 효과가 대상을 고르는 방식을 구분한다.
     */
    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    /*
     * 추가 효과의 대상 범위를 구분한다.
     */
    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    /*
     * 추가 효과가 실행되는 시점을 구분한다.
     */
    public enum SkillMultiEffectTiming
    {
        OnCast,
        OnDeploymentCast,
        Delayed,
        OnHit,
        OnExpire,
        OnHitCount
    }

    /*
     * 범위 효과의 중심 위치를 구분한다.
     */
    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

    /*
     * 스킬 Trigger를 발생시키는 전투 사건을 구분한다.
     */
    public enum SkillTriggerEvent
    {
        OnMagazineLastProjectileHit,
        OnShieldExpire,
        OnShieldAbsorb,
        OnStatusExpire,
        OnOutgoingDamage,
        OnKill,
        OnSkillCast,
        CombatStart
    }

    /*
     * Trigger 피해가 기준으로 삼을 값을 구분한다.
     */
    public enum SkillTriggerDamageSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    /*
     * Trigger가 실행할 결과 동작을 구분한다.
     */
    public enum SkillTriggerActionKind
    {
        Auto,
        SingleAttack,
        LineAttack,
        TriggeredSkill,
        Effect,
        CooldownRefund,
        ReloadReduce
    }

    /*
     * 전투 사건의 조건과 그때 실행할 스킬·효과 정보를 보관한다.
     */
    [Serializable]
    public class SkillTriggerDefinition
    {
        // Trigger 식별과 발생 조건
        public string TriggerId;
        public string MonsterId;
        public string SourceSkillId;
        public SkillTriggerEvent TriggerEvent;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        public string ConditionStatusId;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string ConditionStatusSourceSkillId;
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public string TriggerAttribute;
        // Trigger 실행 동작과 대상 선택
        public SkillTriggerActionKind TriggerAction;
        public string EventSkillId;
        public string EventSkillRuntimeKinds;
        public SkillRuntimeKindCondition[] EventSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();
        public float ProcChance = 1f;
        public float InternalCooldownSeconds;
        public float TriggerDelaySeconds;
        public int TriggerEveryCount;
        public string EventSourceScope;
        public string TriggeredSkillId;
        public string TargetSkillId;
        public string TriggeredEffectId;
        public SkillRuntimeKind RuntimeKind;
        public int SortOrder;
        public SkillMultiEffectTargetSide TargetSide;
        public SkillMultiEffectTargetSelection TargetSelection;
        public SkillMultiEffectTargetShape TargetShape;
        public SkillMultiEffectCenterMode CenterMode;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float DamageMultiplier = 1f;
        public SkillTriggerDamageSource DamageSource;
        public float DamageSourceMultiplier;
        // Trigger 피해 계산
        public DamageAttribute TrackedAttribute;
        public float Radius;
        public bool CoverAll;
        // 반복 실행과 재사용 대기시간 변경
        public string HitTargetCount;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public float CooldownRefundRatio;
        public float ReloadReduceRatio;
        // Trigger 표시와 런타임 지원 상태
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }

    /*
     * 기본 스킬에 덧붙일 피해, 상태, 재시전 효과와 실행 조건을 보관한다.
     */
    [Serializable]
    public class SkillEffectDefinition
    {
        // 효과 식별과 실행 시점
        public string EffectId;
        public string SkillId;
        public int SortOrder;
        public SkillMultiEffectKind EffectKind;
        public SkillMultiEffectTargetSide TargetSide;
        public SkillMultiEffectTargetSelection TargetSelection;
        public SkillMultiEffectTargetShape TargetShape;
        public SkillMultiEffectCenterMode CenterMode;
        public SkillMultiEffectVisualAnchorMode VisualAnchorMode;
        public SkillMultiEffectTiming EffectTiming;
        public float DelaySeconds;
        public bool EnabledByDefault;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string RequiresPassiveSkillId;
        public string ExcludesPassiveSkillId;
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        // 선택지·패시브·상태 기반 실행 조건
        public bool ApplyOnce;
        public string ConditionStatusId;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string ConditionStatusSourceSkillId;
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public SkillMultiEffectTargetSide ConditionTargetSide;
        public string ConditionSkillAttribute;
        public float ConditionHealthRatioMax;
        public int ConditionHitCountMin;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float DamageMultiplier = 1f;
        public float Radius;
        public bool CoverAll;
        public float ActiveDurationSeconds;
        // 피해와 범위 설정
        public float TickIntervalSeconds;
        // 기존 장판을 다시 생성할 때 사용할 설정
        public string RecastSourceSkillId;
        public float RecastDurationSeconds;
        public float RecastRadiusMultiplier = 1f;
        public bool RecastInheritSnapshot = true;
        public int RecastMaxGeneration = 1;
        public string StatusEffectId;
        public StatusEffectKind StatusKind;
        public StatusRuntimeData CompiledStatusData;
        [Range(0f, 1f)] public float StatusChance;
        public string StatusEffectLabel;
        public GameObject StatusEffectPrefab;
        public float StatusDurationSeconds;
        public int StatusMaxStacks;
        public int StatusStackAmount;
        public string StatusTargetScope;
        public string StatusMergePolicy;
        public string ShieldAmountRefreshPolicy;
        public float StatusActionSpeedBonus;
        public float StatusMoveSpeedBonus;
        public float StatusAttackPowerBonus;
        public float StatusSpellPowerBonus;
        public float StatusDamageBonusRate;
        // 상태 적용과 상태 능력치 변경
        public float StatusShieldReceivedBonus;
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        public float StatusCriticalChanceBonus;
        public string StatusConditionalTargetStatusId;
        public float StatusConditionalStatusChanceBonus;
        public string StatusConditionalIncomingSkillRuntimeKinds;
        public string StatusConditionalOutgoingSkillRuntimeKinds;
        public string StatusAppliedStatusDurationBonusStatusId;
        public float StatusAppliedStatusDurationBonus;
        public float StatusOutgoingAdditionalDamageMultiplier;
        public DamageAttribute StatusOutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute StatusOutgoingAdditionalDamageAttribute;
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        // 런타임 지원 여부와 진단 설명
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }

    /*
     * 그래프 노드에 전달할 매개변수 하나를 보관한다.
     */
    [Serializable]
    public class SkillNodeParamDefinition
    {
        public string NodeId;
        public string ParamKey;
        public string ValueType;
        public string Value;
    }

    /*
     * 스킬·선택지·Trigger가 소유하는 실행 그래프 노드를 보관한다.
     */
    [Serializable]
    public class SkillNodeDefinition
    {
        public string NodeId;
        public string OwnerKind;
        public string OwnerId;
        public string TargetSkillId;
        public string NodeKind;
        public string HandlerId;
        public int SortOrder;
        public bool EnabledByDefault;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string RequiresPassiveSkillId;
        public string ExcludesPassiveSkillId;
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
        public SkillNodeParamDefinition[] Params = Array.Empty<SkillNodeParamDefinition>();
    }
}


/*
 * 추가 효과의 시각 오브젝트가 붙을 위치를 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillMultiEffectVisualAnchorMode
    {
        Center,
        AppliedTargets
    }

    /*
     * 스킬 시각 오브젝트가 스킬과 상태 중 어디에 속하는지 구분한다.
     */
    public enum RuntimeSkillVisualAnchor
    {
        Skill,
        StatusTarget
    }

    /*
     * 런타임 스킬 충돌 영역의 크기와 중심 보정값을 보관한다.
     */
    [Serializable]
    public class RuntimeSkillHitboxSpec
    {
        public Vector2 Size;
        public Vector2 Offset;

        /*
         * 너비와 높이가 모두 설정됐는지 확인한다.
         */
        public bool HasHitbox()
        {
            return Size.x > 0f && Size.y > 0f;
        }
    }

    /*
     * 런타임에서 조합할 스프라이트, 애니메이터, 크기, 충돌 영역을 보관한다.
     */
    [Serializable]
    public class RuntimeSkillVisualSpec
    {
        public Sprite Sprite;
        public RuntimeAnimatorController AnimatorController;
        public float Scale = 1f;
        public bool UseLocalScale;
        public Vector3 LocalScale = Vector3.one;
        public int SortingOrder;
        public RuntimeSkillVisualAnchor Anchor = RuntimeSkillVisualAnchor.Skill;
        public RuntimeSkillHitboxSpec Hitbox = new RuntimeSkillHitboxSpec();

        /*
         * 화면 표시나 충돌 영역으로 사용할 데이터가 있는지 확인한다.
         */
        public bool HasVisual()
        {
            return Sprite != null
                || AnimatorController != null
                || (Hitbox != null && Hitbox.HasHitbox());
        }
    }
}
