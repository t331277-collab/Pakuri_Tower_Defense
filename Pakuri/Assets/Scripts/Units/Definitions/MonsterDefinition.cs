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

    /// MonsterDefinition의 저작 데이터와 런타임 설정을 정의한다.
    public class MonsterDefinition : ScriptableObject
    {

        /// RewardChoiceDefinition의 저작 데이터와 런타임 설정을 정의한다.
        [Serializable]

        public class RewardChoiceDefinition
        {
            public string RewardName = "reward-Name";
            public string ActiveSkillName = string.Empty;
            public string PassiveSkillName = string.Empty;
        }

        public string MonsterName = "monster-Name";
        public string DisplayName = "Monster";
        [TextArea(2, 4)] public string RoleSummary = "Role summary.";
        public string ElementLabel = "Element";
        public DamageAttribute PrimaryAttribute = DamageAttribute.Physical;
        public string ActiveSkillName = "Skill A";
        public string PassiveSkillName = "Passive F";
        public Sprite MonsterIconImage;
        public Sprite Image;
        public Sprite MainTypeIcon;
        public Sprite SubTypeIcon;
        public AnimationClip StandingAnimation;
        public UnitCombatStats BaseStats = new UnitCombatStats
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f
        };
        public UnitDefenseStats Defenses = new UnitDefenseStats();

        public float PowerStat = 30f;

        [Header("Initial Reward Loop")]

        public RewardChoiceDefinition[] InitialRewardChoices = Array.Empty<RewardChoiceDefinition>();

        [Header("Full Skill Data")]

        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public PassiveSkillDefinition[] PassiveSkills = Array.Empty<PassiveSkillDefinition>();
    }
}
