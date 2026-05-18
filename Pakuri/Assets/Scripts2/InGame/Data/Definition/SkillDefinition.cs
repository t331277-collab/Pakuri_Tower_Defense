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
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public bool HasDamageMultiplier;
        public float DamageMultiplier = 1f;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasReloadDurationMultiplier;
        public float ReloadDurationMultiplier = 1f;
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
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
        public float ProjectileSpeed;
        public int PierceCount;
        public bool CriticalAllowed = true;
        public string StatusEffectId;
        [Range(0f, 1f)] public float StatusChance;
        public string StatusEffectLabel;
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
