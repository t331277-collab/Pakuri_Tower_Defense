using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

/*
 * CSV에서 구성된 적의 능력치, 공격 속성, 스킬과 패시브 정의 형식을 제공한다.
 */
namespace Pakuri.Data
{
    /*
     * 적 패시브가 변경하는 전투 능력치를 구분한다.
     */
    public enum EnemyPassiveModifierKind
    {
        None,
        DamageUp,
        DefenseUp,
        CritChanceUp,
        CritDamageUp,
        HealingUp,
        IncomingDamageDown
    }

    [Serializable]
    /*
     * 적에게 적용할 고정 패시브의 식별자와 능력치 변경값을 보관한다.
     */
    public class EnemyPassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public EnemyPassiveModifierKind ModifierKind;
        public bool HasAttribute;
        public DamageAttribute Attribute;
        public float ModifierValue;

        /*
         * 원본과 분리된 패시브 정의 복사본을 만든다.
         */
        public EnemyPassiveDefinition Clone()
        {
            return new EnemyPassiveDefinition
            {
                PassiveId = PassiveId,
                DisplayName = DisplayName,
                ModifierKind = ModifierKind,
                HasAttribute = HasAttribute,
                Attribute = Attribute,
                ModifierValue = ModifierValue
            };
        }
    }

    /*
     * CSV에서 구성되는 적의 기본 능력치, 외형, 스킬 정보를 보관한다.
     */
    public class EnemyDefinition : ScriptableObject
    {
        // 적 식별과 전투 분류
        public string EnemyId;
        public string DisplayName;
        public DamageAttribute Attribute;
        public UnitCombatStats Stats = new UnitCombatStats
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f,
            CriticalResistance = 0f
        };
        public UnitDefenseStats Defenses = new UnitDefenseStats();

        [Header("Shared Runtime Skills")]
        // 공용 스킬 런타임으로 전달할 액티브 스킬과 Trigger
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();

        [Header("Passive")]
        // 생성 시 적용할 패시브와 넥서스 접촉 피해
        public EnemyPassiveDefinition PassiveSkill;
        public float NexusDamage = 1f;

    }
}
