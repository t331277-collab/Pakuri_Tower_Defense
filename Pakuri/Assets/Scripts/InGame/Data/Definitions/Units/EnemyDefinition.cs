using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

/*
 * 스테이지에서 적이 맡는 전투 위치를 구분한다.
 */
namespace Pakuri.Data
{
    public enum EnemyEncounterRole
    {
        Normal,
        Day5Midboss,
        Day10Midboss,
        StageBoss
    }

    /*
     * 적의 기본 공격 방식을 구분한다.
     */
    public enum EnemyAttackType
    {
        Melee,
        Ranged,
        MeleeAndRanged,
        Buffer
    }

    /*
     * 적 패시브가 변경하는 전투 능력치를 구분한다.
     */
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

    /*
     * 적 패시브 효과를 받을 대상을 구분한다.
     */
    public enum EnemyPassiveTarget
    {
        Self
    }

    [Serializable]
    /*
     * 적에게 적용할 고정 패시브의 식별자와 능력치 변경값을 보관한다.
     */
    public sealed class EnemyPassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public EnemyPassiveTarget ApplyTarget = EnemyPassiveTarget.Self;
        public EnemyPassiveModifierKind ModifierKind;
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
                ApplyTarget = ApplyTarget,
                ModifierKind = ModifierKind,
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
        public DamageCalculator.AttributeDefenseSet Defenses = new DamageCalculator.AttributeDefenseSet();

        [Header("Combat Visuals")]
        // 전투에서 사용하는 기본 스프라이트
        public Sprite UnitSprite;
        public Sprite ProjectileSprite;

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
