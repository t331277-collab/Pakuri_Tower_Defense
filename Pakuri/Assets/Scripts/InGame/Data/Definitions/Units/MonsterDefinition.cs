using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

/*
 * CSV에서 구성되는 플레이어 몬스터의 능력치, 외형, 스킬 정보를 보관한다.
 */
namespace Pakuri.Data
{
    public class MonsterDefinition : ScriptableObject
    {
        [Serializable]
        /*
         * 초기 보상에서 연결할 액티브·패시브 스킬 ID를 보관한다.
         */
        public class RewardChoiceDefinition
        {
            public string RewardId = "reward-id";
            public string ActiveSkillId = string.Empty;
            public string PassiveSkillId = string.Empty;
        }

        // 몬스터 식별과 표시 정보
        public string MonsterId = "monster-id";
        public string DisplayName = "Monster";
        [TextArea(2, 4)] public string RoleSummary = "Role summary.";
        public string ElementLabel = "Element";
        public DamageAttribute PrimaryAttribute = DamageAttribute.Physical;
        public string ActiveSkillName = "Skill A";
        public string PassiveSkillName = "Passive F";
        public Sprite MonsterIconImage;
        public UnitStatsRuntime BaseStats = new UnitStatsRuntime
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f,
            CriticalChance = DamageCalculator.BaseCriticalChance,
            CriticalDamage = DamageCalculator.BaseCriticalMultiplier,
            CriticalResistance = 0f
        };
        public DamageCalculator.AttributeDefenseSet Defenses = new DamageCalculator.AttributeDefenseSet();

        [Header("Prototype Combat Tuning")]
        // 초기 전투 프로토타입에서 사용하는 표시와 공격 설정
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
        // 전투 시작 시 제시할 보상 선택지
        public RewardChoiceDefinition[] InitialRewardChoices = Array.Empty<RewardChoiceDefinition>();

        [Header("Full Skill Data")]
        // 공용 스킬 런타임으로 전달할 전체 스킬과 Trigger
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public PassiveDefinition[] PassiveSkills = Array.Empty<PassiveDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
    }
}
