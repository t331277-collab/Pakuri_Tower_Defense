using System;
using Pakuri.Combat;
using UnityEngine;

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

    public enum StageOneEnemySkillKind
    {
        Slash,
        ShieldUp,
        AimedShot,
        ShurikenThrow,
        Heal,
        GuardianFlag,
        ChargeCommand,
        SacredSwordWave
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

        [Header("Stage 1 Skill")]
        public bool HasBasicSkill;
        public StageOneEnemySkillKind BasicSkill;
        public string BasicSkillName;
        public float BasicSkillCoefficient = 1f;
        public float BasicSkillCooldown = 2f;
        public float BasicSkillDuration;
        public float BasicSkillRadius;
        public float BasicSkillFlatValue;
        public float BasicSkillProjectileSpeed;
        public float BasicSkillProjectileLifetime;
        public float BasicSkillMoveSpeedMultiplier = 1f;
        public float BasicSkillOutgoingDamageMultiplier = 1f;
        public StageOneEnemySkillKind StageOneSkill;
        public string ActiveSkillName;
        public float ActiveSkillCoefficient = 1f;
        public float ActiveSkillCooldown = 2f;
        public float ActiveSkillDuration;
        public float ActiveSkillRadius;
        public float ActiveSkillFlatValue;
        public float ActiveSkillProjectileSpeed;
        public float ActiveSkillProjectileLifetime;
        public float ActiveSkillMoveSpeedMultiplier = 1f;
        public float ActiveSkillOutgoingDamageMultiplier = 1f;
        public string PassiveSkillName;
        public string PassiveSkillId;
        public float PassiveSkillValue;
        [TextArea(2, 4)] public string PassiveSummary;

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
            clone.HasBasicSkill = HasBasicSkill;
            clone.BasicSkill = BasicSkill;
            clone.BasicSkillName = BasicSkillName;
            clone.BasicSkillCoefficient = BasicSkillCoefficient;
            clone.BasicSkillCooldown = BasicSkillCooldown;
            clone.BasicSkillDuration = BasicSkillDuration;
            clone.BasicSkillRadius = BasicSkillRadius;
            clone.BasicSkillFlatValue = BasicSkillFlatValue;
            clone.BasicSkillProjectileSpeed = BasicSkillProjectileSpeed;
            clone.BasicSkillProjectileLifetime = BasicSkillProjectileLifetime;
            clone.BasicSkillMoveSpeedMultiplier = BasicSkillMoveSpeedMultiplier;
            clone.BasicSkillOutgoingDamageMultiplier = BasicSkillOutgoingDamageMultiplier;
            clone.StageOneSkill = StageOneSkill;
            clone.ActiveSkillName = ActiveSkillName;
            clone.ActiveSkillCoefficient = ActiveSkillCoefficient;
            clone.ActiveSkillCooldown = ActiveSkillCooldown;
            clone.ActiveSkillDuration = ActiveSkillDuration;
            clone.ActiveSkillRadius = ActiveSkillRadius;
            clone.ActiveSkillFlatValue = ActiveSkillFlatValue;
            clone.ActiveSkillProjectileSpeed = ActiveSkillProjectileSpeed;
            clone.ActiveSkillProjectileLifetime = ActiveSkillProjectileLifetime;
            clone.ActiveSkillMoveSpeedMultiplier = ActiveSkillMoveSpeedMultiplier;
            clone.ActiveSkillOutgoingDamageMultiplier = ActiveSkillOutgoingDamageMultiplier;
            clone.PassiveSkillName = PassiveSkillName;
            clone.PassiveSkillId = PassiveSkillId;
            clone.PassiveSkillValue = PassiveSkillValue;
            clone.PassiveSummary = PassiveSummary;
            return clone;
        }
    }
}
