using System;
using Pakuri.Combat;
using UnityEngine;

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
        public int RequiredSourceStatusMinStacks;
        public string ConditionStatusId;
        public string ConditionStatusSourceSkillId;
        public string TriggerAttribute;
        // Trigger 실행 동작과 대상 선택
        public SkillTriggerActionKind TriggerAction;
        public string EventSkillId;
        public string EventSkillRuntimeKinds;
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
        public int RequiredSourceStatusMinStacks;
        // 선택지·패시브·상태 기반 실행 조건
        public bool ApplyOnce;
        public string ConditionStatusId;
        public string ConditionStatusSourceSkillId;
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
