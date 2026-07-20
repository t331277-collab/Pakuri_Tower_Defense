using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 선택지 원본 정의에 런타임에서 계산된 보정값만 추가한다.
     */
    [Serializable]
    public sealed class SkillChoiceRuntimeData : SkillChoiceDefinition
    {
        public string Description;
        public Sprite Icon;
        public bool HasShieldAmountMultiplier;
        public float ShieldAmountMultiplier = 1f;
        public BuffModifierSpec AddedModifiers = new BuffModifierSpec();
        public bool HasStatusDamageBonusRate;
        public float StatusDamageBonusRate;
        public bool HasStatusShieldReceivedBonus;
        public float StatusShieldReceivedBonus;
        public bool HasStatusCriticalChanceBonus;
        public float StatusCriticalChanceBonus;
        public bool HasStatusDamageTakenBonus;
        public float StatusDamageTakenBonus;
        public bool HasStatusFlatElementResistReduction;
        public float StatusFlatElementResistReduction;
        public string StatusActionSpeedBonusStatusId;
        public new SkillExecutionPlanNode[] NormalizedPlanNodes = Array.Empty<SkillExecutionPlanNode>();
    }
}
