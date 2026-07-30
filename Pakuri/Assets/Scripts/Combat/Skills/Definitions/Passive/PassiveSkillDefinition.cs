/*
 * 역할: 패시브 스킬 Definition.
 * 책임: 활성 스킬 요구 조건과 유닛 수치 보정값을 정의한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
    public enum PassiveModifierKind
    {
        None,
        DamageUp,
        DefenseUp,
        CritChanceUp,
        CritDamageUp,
        HealingUp,
        IncomingDamageDown
    }
}

namespace Pakuri.InGame
{
    public class PassiveSkillDefinition : SkillDefinition
    {
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;

        [Header("Unit Modifier")]
        public PassiveModifierKind ModifierKind;
        public bool HasModifierAttribute;
        public DamageAttribute ModifierAttribute;
        public float ModifierValue;

        [Header("Choices")]
        public SkillChoice[] BaseModifierChoices = Array.Empty<SkillChoice>();
    }
}
