using System;
using Pakuri.Combat;
using UnityEngine;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
using CombatStatBlock = Pakuri.Combat.DamageCalculator.CombatStatBlock;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/Monster Definition", fileName = "MonsterDefinition")]
    public class MonsterDefinition : ScriptableObject
    {
        [Serializable]
        public class RewardChoiceDefinition
        {
            public string RewardId = "reward-id";
            public string ActiveSkillId = string.Empty;
            public string PassiveSkillId = string.Empty;
            public string LinkedChoiceId = string.Empty;
            public string Title = "Reward";
            [TextArea(2, 4)] public string Description = "Reward description.";
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
        }

        public string MonsterId = "monster-id";
        public string DisplayName = "Monster";
        [TextArea(2, 4)] public string RoleSummary = "Role summary.";
        public string ElementLabel = "Element";
        public DamageAttribute PrimaryAttribute = DamageAttribute.Physical;
        public string ActiveSkillName = "Skill A";
        public string PassiveSkillName = "Passive F";
        public CombatStatBlock BaseStats = new CombatStatBlock();
        public AttributeDefenseSet Defenses = new AttributeDefenseSet();

        [Header("Prototype Combat Tuning")]
        public Sprite UnitSprite;
        public Sprite ProjectileSprite;
        public Color UnitColor = Color.white;
        public Color ProjectileColor = Color.white;
        public float MaxHealth = 220f;
        public float PowerStat = 30f;
        public float BaseDamage = 24f;
        public float PowerCoefficient = 0.95f;
        public float ProjectileSpeed = 15f;
        public float ProjectileLifetime = 5f;
        public float ProjectileHitRadius = 0.42f;
        public int MagazineCapacity = 6;
        public float ReloadDuration = 4f;
        public float ShotInterval = 0.35f;
        [Range(0f, 1f)] public float StatusChance;
        public string StatusEffectLabel = string.Empty;

        [Header("Initial Reward Loop")]
        public RewardChoiceDefinition[] InitialRewardChoices = Array.Empty<RewardChoiceDefinition>();

        [Header("Full Skill Data")]
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public PassiveDefinition[] PassiveSkills = Array.Empty<PassiveDefinition>();
    }
}
