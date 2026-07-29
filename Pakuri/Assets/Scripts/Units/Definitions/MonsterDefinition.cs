/*
 * 역할: 몬스터 및 보상 저작 정의.
 * 책임: 플레이 가능 몬스터의 표시·능력치·에셋·스킬·보상 선택지 데이터를 보관한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{

    /// <summary><c>MonsterDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class MonsterDefinition : ScriptableObject
    {

        /// <summary><c>RewardChoiceDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
        [Serializable]

        public class RewardChoiceDefinition
        {
            public string RewardId = "reward-id";
            public string ActiveSkillId = string.Empty;
            public string PassiveSkillId = string.Empty;
        }

        public string MonsterId = "monster-id";
        public string DisplayName = "Monster";
        [TextArea(2, 4)] public string RoleSummary = "Role summary.";
        public string ElementLabel = "Element";
        public DamageAttribute PrimaryAttribute = DamageAttribute.Physical;
        public string ActiveSkillName = "Skill A";
        public string PassiveSkillName = "Passive F";
        public Sprite MonsterIconImage;
        public UnitCombatStats BaseStats = new UnitCombatStats
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f,
            CriticalResistance = 0f
        };
        public UnitDefenseStats Defenses = new UnitDefenseStats();

        public float PowerStat = 30f;

        [Header("Initial Reward Loop")]

        public RewardChoiceDefinition[] InitialRewardChoices = Array.Empty<RewardChoiceDefinition>();

        [Header("Full Skill Data")]

        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public PassiveSkillDefinition[] PassiveSkills = Array.Empty<PassiveSkillDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
    }
}
