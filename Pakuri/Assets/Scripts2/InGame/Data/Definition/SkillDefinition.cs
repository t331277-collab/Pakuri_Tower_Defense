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
        PassiveEnhancement
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
        Delayed
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
        OnStatusExpire
    }

    public enum SkillTriggerDamageSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage
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
        public string TriggeredSkillId;
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
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float DamageMultiplier = 1f;
        public float Radius;
        public bool CoverAll;
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
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        public float StatusCriticalChanceBonus;
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
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
        public bool HasRadiusMultiplier;
        public float RadiusMultiplier = 1f;
        public float RadiusBonus;
        public float BeamWidthBonus;
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
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;
        public int HitTargetCountBonus;
        public float CritChanceBonus;
        public float CritDamageBonus;
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
        public string CountStatusId;
        public SkillMultiEffectTargetSide CountTargetSide;
        public float DamageMultiplierPerCount;
        public int CountMax;
        public bool HasStatusConditionalDamageTakenBonus;
        public float StatusConditionalDamageTakenBonus;
        public string StatusConditionalSourceStatusId;
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
        public string HitTargetCount;
        public string TargetSelection;
        public float CooldownSeconds;
        public float ActiveDurationSeconds;
        public int MagazineCapacity;
        public float ReloadSeconds;
        public float ShotIntervalSeconds;
        public int ProjectileBurstCount;
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
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] PassiveEffects = Array.Empty<SkillEffectDefinition>();
    }
}
