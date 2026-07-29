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
        public UnitCombatStats BaseStats = new UnitCombatStats
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f,
            CriticalResistance = 0f
        };
        public UnitDefenseStats Defenses = new UnitDefenseStats();

        // 런 선택 화면에 표시할 전투력
        public float PowerStat = 30f;

        [Header("Initial Reward Loop")]
        // 전투 시작 시 제시할 보상 선택지
        public RewardChoiceDefinition[] InitialRewardChoices = Array.Empty<RewardChoiceDefinition>();

        [Header("Full Skill Data")]
        // 공용 스킬 런타임으로 전달할 전체 스킬과 Trigger
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public PassiveSkillDefinition[] PassiveSkills = Array.Empty<PassiveSkillDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
    }
}
