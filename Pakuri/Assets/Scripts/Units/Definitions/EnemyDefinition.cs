/*
 * 역할: 적 저작 정의.
 * 책임: 적 식별·표시·능력치·이동·Nexus 피해·스킬·패시브 참조를 보관한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{

    /// EnemyDefinition의 저작 데이터와 런타임 설정을 정의한다.
    public class EnemyDefinition : ScriptableObject
    {

        public string EnemyId;
        public string DisplayName;
        public Sprite Image;
        public DamageAttribute Attribute;
        public UnitCombatStats Stats = new UnitCombatStats
        {
            MaxHealth = 100f,
            AttackPower = 30f,
            SpellPower = 30f,
            MoveSpeed = 1f
        };
        public UnitDefenseStats Defenses = new UnitDefenseStats();

        [Header("Shared Runtime Skills")]

        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();

        [Header("Passive")]

        public PassiveSkillDefinition PassiveSkill;
        public float NexusDamage = 1f;

    }
}
