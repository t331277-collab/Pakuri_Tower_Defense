using System;
using Pakuri.Data;

namespace Pakuri.InGame
{
    /*
     * 선택지 원본과 실행용 계산값을 함께 보관한다.
     */
    [Serializable]
    public sealed class SkillChoiceRuntimeData
    {
        public SkillChoiceDefinition Source;
        public SkillExecutionPlanNode[] PlanNodes = Array.Empty<SkillExecutionPlanNode>();
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

        public string ChoiceId => Source.ChoiceId;
    }
}
