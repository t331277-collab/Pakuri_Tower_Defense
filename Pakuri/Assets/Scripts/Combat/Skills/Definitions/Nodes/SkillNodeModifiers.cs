/*
 * 역할: 전투 결과를 바꾸는 보정 규칙을 정의한다.
 * 책임: 피해와 치명타가 대상 조건에 따라 달라질 기준값을 제공한다.
 */

using Pakuri.Data;

namespace Pakuri.InGame
{
    /// 피해 보정이 적용될 전투 조건을 구분한다.
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    /// 특정 전투 조건에 적용할 피해 배율을 나타낸다.
    public readonly struct DamageModifierOp
    {
        /// 특정 전투 조건에서 적용할 피해 배율을 고정한다.
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    /// 조건부로 더할 치명타 확률을 나타낸다.
    public readonly struct CritModifierOp
    {
        /// 조건을 만족했을 때 더할 치명타 확률을 고정한다.
        public CritModifierOp(float chanceBonus)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    /// 대상 상태가 피해 배율을 바꾸는 규칙을 나타낸다.
    public readonly struct ConditionalDamageActionOp
    {
        /// 대상 상태에 따라 적용할 피해 배율을 정의한다.
        public ConditionalDamageActionOp(
            float damageMultiplier,
            StatusEffectKind requiredStatus,
            int minimumStacks)
        {
            DamageMultiplier = damageMultiplier;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float DamageMultiplier { get; }
        public StatusStackCondition Condition { get; }
    }

    /// OR/AND 상태식을 만족한 대상에 적용할 피해 배율이다.
    public readonly struct ConditionalStatusGroupDamageActionOp
    {
        public ConditionalStatusGroupDamageActionOp(
            float damageMultiplier,
            StatusConditionGroup[] groups)
        {
            DamageMultiplier = damageMultiplier;
            Groups = groups ?? System.Array.Empty<StatusConditionGroup>();
        }

        public float DamageMultiplier { get; }
        public StatusConditionGroup[] Groups { get; }
    }

    /// 대상 상태가 치명타 확률을 바꾸는 규칙을 나타낸다.
    public readonly struct ConditionalCritChanceActionOp
    {
        /// 대상 상태에 따라 적용할 치명타 보정을 정의한다.
        public ConditionalCritChanceActionOp(
            float chanceBonus,
            StatusEffectKind requiredStatus,
            int minimumStacks)
        {
            ChanceBonus = chanceBonus;
            Condition = new StatusStackCondition(requiredStatus, minimumStacks);
        }

        public float ChanceBonus { get; }
        public StatusStackCondition Condition { get; }
    }

    /// 시전자 상태가 대상의 받는 피해를 바꾸는 규칙을 나타낸다.
    public readonly struct StatusConditionalDamageTakenActionOp
    {
        /// 시전자 상태가 대상의 받는 피해를 바꾸는 규칙을 정의한다.
        public StatusConditionalDamageTakenActionOp(
            float bonus,
            StatusEffectKind requiredSourceStatus)
        {
            Bonus = bonus;
            RequiredSourceStatus = requiredSourceStatus;
        }

        public float Bonus { get; }
        public StatusEffectKind RequiredSourceStatus { get; }
    }
}
