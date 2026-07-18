using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;

namespace Pakuri.Data
{
    public enum EnemyEncounterRole
    {
        Normal,
        Day5Midboss,
        Day10Midboss,
        StageBoss
    }

    public enum EnemyAttackType
    {
        Melee,
        Ranged,
        MeleeAndRanged,
        Buffer
    }

    public enum EnemyPassiveModifierKind
    {
        None,
        PhysicalDamageUp,
        FireDamageUp,
        LightningDamageUp,
        IceDamageUp,
        DarknessDamageUp,
        HolyDamageUp,
        DefenseUp,
        PhysicalDefenseUp,
        FireDefenseUp,
        LightningDefenseUp,
        IceDefenseUp,
        DarknessDefenseUp,
        HolyDefenseUp,
        CritChanceUp,
        CritDamageUp,
        HealingUp,
        IncomingDamageDown
    }

    public enum EnemyPassiveTarget
    {
        Self
    }

    [Serializable]
    public sealed class EnemyPassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public EnemyPassiveTarget ApplyTarget = EnemyPassiveTarget.Self;
        public EnemyPassiveModifierKind ModifierKind;
        public float ModifierValue;

        public EnemyPassiveDefinition Clone()
        {
            return new EnemyPassiveDefinition
            {
                PassiveId = PassiveId,
                DisplayName = DisplayName,
                ApplyTarget = ApplyTarget,
                ModifierKind = ModifierKind,
                ModifierValue = ModifierValue
            };
        }
    }

    [CreateAssetMenu(menuName = "Pakuri/Enemy Definition", fileName = "EnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string EnemyId;
        public string DisplayName;
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
        public UnitStatsRuntime Stats = new UnitStatsRuntime
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f,
            CriticalChance = DamageCalculator.BaseCriticalChance,
            CriticalDamage = DamageCalculator.BaseCriticalMultiplier,
            CriticalResistance = 0f
        };
        public AttributeDefenseSet Defenses = new AttributeDefenseSet();

        [Header("Combat Visuals")]
        public Sprite UnitSprite;
        public Sprite ProjectileSprite;

        [Header("Shared Runtime Skills")]
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();

        [Header("Passive")]
        public EnemyPassiveDefinition PassiveSkill;
        public float NexusDamage = 1f;

    }
}
