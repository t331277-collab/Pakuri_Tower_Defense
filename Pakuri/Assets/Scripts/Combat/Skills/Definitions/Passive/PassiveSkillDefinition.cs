/*
 * 역할: 지속적으로 적용되는 스킬의 설계값을 정의한다.
 * 책임: 활성 조건과 유닛 능력치 변화, 다른 스킬에 줄 기본 보정을 제공한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
    /// 패시브가 바꿀 유닛 능력치의 종류를 구분한다.
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
    /// 지속 효과의 활성 조건과 능력치 변화를 설계한다.
    public class PassiveSkillDefinition : SkillDefinition
    {
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;

        [Header("Unit Modifier")]
        public PassiveModifierKind ModifierKind;
        public bool HasModifierAttribute;
        public DamageAttribute ModifierAttribute;
        public float ModifierValue;
    }
}
