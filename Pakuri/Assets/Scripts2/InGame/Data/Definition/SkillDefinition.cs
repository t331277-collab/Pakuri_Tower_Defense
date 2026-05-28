using System;
using Pakuri.Combat;
using UnityEngine;

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

    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }

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

    public enum SkillMultiEffectKind
    {
        Damage,
        Status,
        ExtendStatusDuration
    }

    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    public enum SkillMultiEffectTiming
    {
        OnCast,
        OnDeploymentCast,
        Delayed,
        OnHit,
        OnExpire,
        OnHitCount
    }

    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

    public enum SkillMultiEffectVisualAnchorMode
    {
        Center,
        AppliedTargets
    }

    public enum SkillTriggerEvent
    {
        OnMagazineLastProjectileHit,
        OnShieldExpire,
        OnShieldAbsorb,
        OnStatusExpire,
        OnOutgoingDamage,
        OnKill,
        OnSkillCast
    }

    public enum SkillTriggerDamageSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

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

    [Serializable]
    public class SkillTriggerDefinition
    {
        public string TriggerId;
        public string MonsterId;
        public string SourceSkillId;
        public SkillTriggerEvent TriggerEvent;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string ConditionStatusId;
        public string ConditionStatusSourceSkillId;
        public string TriggerAttribute;
        public SkillTriggerActionKind TriggerAction;
        public string EventSkillId;
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
        public DamageAttribute TrackedAttribute;
        public float Radius;
        public bool CoverAll;
        public string HitTargetCount;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public float CooldownRefundRatio;
        public float ReloadReduceRatio;
        public GameObject SkillEffectPrefab;
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }

    [Serializable]
    public class SkillEffectDefinition
    {
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
        public bool ApplyOnce;
        public string ConditionStatusId;
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
        public float TickIntervalSeconds;
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
        public string StatusAppliedStatusDurationBonusStatusId;
        public float StatusAppliedStatusDurationBonus;
        public float StatusOutgoingAdditionalDamageMultiplier;
        public DamageAttribute StatusOutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute StatusOutgoingAdditionalDamageAttribute;
        public GameObject SkillEffectPrefab;
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }

    [Serializable]
    public class SkillChoiceDefinition
    {
        public string ChoiceId;
        public string MonsterId;
        public string SkillId;
        public string TargetSkillId;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public bool HasDamageMultiplier;
        public float DamageMultiplier = 1f;
        public float BaseDamageBonus;
        public bool HasCooldownMultiplier;
        public float CooldownMultiplier = 1f;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        public int AdditionalProjectileBonus;
        public int PierceBonus;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasBurstDamageProjectileIndex;
        public int BurstDamageProjectileIndex;
        public bool HasBurstDamageMultiplier;
        public float BurstDamageMultiplier = 1f;
        public int FollowUpProjectileCount;
        public float FollowUpProjectileDelaySeconds;
        public float FollowUpProjectileDamageMultiplier = 1f;
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
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
        public string StatusTag;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
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
        public int ThresholdStatusMinStacks;
        public string ThresholdApplyStatusId;
        public bool HasConditionalDamageMultiplier;
        public float ConditionalDamageMultiplier = 1f;
        public string ConditionalTargetStatusId;
        public int ConditionalTargetStatusMinStacks;
        public string CountStatusId;
        public SkillMultiEffectTargetSide CountTargetSide;
        public float DamageMultiplierPerCount;
        public int CountMax;
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;
        public bool HasStatusConditionalDamageTakenBonus;
        public float StatusConditionalDamageTakenBonus;
        public string StatusConditionalSourceStatusId;
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
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }

    [Serializable]
    public class SkillDefinition
    {
        public string SkillId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float Radius;
        public float KnockbackDistance;
        public float DamageDelaySeconds;
        [Range(0f, 1f)] public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        [Range(0f, 1f)] public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;
        public string HitTargetCount;
        public string TargetSelection;
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
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] MasterSkillChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
    }

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
    }
}
