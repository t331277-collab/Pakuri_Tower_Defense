using System;
using Pakuri.Combat;
using UnityEngine;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
using CombatStatBlock = Pakuri.Combat.DamageCalculator.CombatStatBlock;

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
        public CombatStatBlock Stats = new CombatStatBlock();
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

        public EnemyDefinition CloneRuntimeCopy()
        {
            var clone = CreateInstance<EnemyDefinition>();
            clone.EnemyId = EnemyId;
            clone.DisplayName = DisplayName;
            clone.EncounterRole = EncounterRole;
            clone.AttackType = AttackType;
            clone.Attribute = Attribute;
            clone.Stats = new CombatStatBlock
            {
                MaxHealth = Stats != null ? Stats.MaxHealth : 100f,
                AttackPower = Stats != null ? Stats.AttackPower : 0f,
                SpellPower = Stats != null ? Stats.SpellPower : 0f,
                MoveSpeed = Stats != null ? Stats.MoveSpeed : 1f,
                CriticalChance = Stats != null ? Stats.CriticalChance : DamageCalculator.BaseCriticalChance,
                CriticalDamage = Stats != null ? Stats.CriticalDamage : DamageCalculator.BaseCriticalMultiplier,
                CriticalResistance = Stats != null ? Stats.CriticalResistance : 0f
            };
            clone.Defenses = Defenses != null ? Defenses.Clone() : new AttributeDefenseSet();
            clone.UnitSprite = UnitSprite;
            clone.ProjectileSprite = ProjectileSprite;
            clone.ActiveSkills = ActiveSkills ?? Array.Empty<SkillDefinition>();
            clone.SkillTriggers = SkillTriggers ?? Array.Empty<SkillTriggerDefinition>();
            clone.PassiveSkill = PassiveSkill != null ? PassiveSkill.Clone() : null;
            clone.NexusDamage = NexusDamage;
            return clone;
        }
    }
}
