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
        public string StatusTag;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
        public int StatusStacksBonus;
        public bool HasStatusStacksSet;
        public int StatusStacksSet;
        public bool HasStatusElementDamageTakenBonus;
        public float StatusElementDamageTakenBonus;
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
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusElementDamageTakenBonus;
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] MasterSkillChoices = Array.Empty<SkillChoiceDefinition>();
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
    }
}
