/*
 * 역할: 소환 유닛 저작 정의.
 * 책임: Run 파티·보상과 분리된 소환 유닛 능력치, 표시 에셋과 스킬을 보관한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{
    public class SummonDefinition : ScriptableObject
    {
        public string SummonId;
        public string DisplayName;
        [TextArea(2, 4)] public string RoleSummary;
        public string ElementLabel;
        public DamageAttribute PrimaryAttribute = DamageAttribute.Physical;
        public Sprite Icon;
        public Sprite Image;
        public UnitCombatStats BaseStats = new UnitCombatStats();
        public UnitDefenseStats Defenses = new UnitDefenseStats();
        public float PowerStat;
        public SkillDefinition[] ActiveSkills = Array.Empty<SkillDefinition>();
    }
}
