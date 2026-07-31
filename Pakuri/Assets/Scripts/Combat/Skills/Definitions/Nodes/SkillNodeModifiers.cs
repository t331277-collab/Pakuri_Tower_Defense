/*
 * 역할: 스킬 노드 보정 계약.
 * 책임: 피해·치명타·조건부 피해 보정값을 정의한다.
 */

using Pakuri.Data;

namespace Pakuri.InGame
{
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    public readonly struct DamageModifierOp
    {
        /// 피해 배율 보정의 의미를 보관한다.
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    public readonly struct CritModifierOp
    {
        /// 치명타 보정의 의미를 보관한다.
        public CritModifierOp(float chanceBonus)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    public readonly struct ConditionalDamageActionOp
    {
        /// 조건부 피해 행동의 의미를 보관한다.
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

    public readonly struct ConditionalCritChanceActionOp
    {
        /// 조건부 치명타 행동의 의미를 보관한다.
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

    public readonly struct StatusConditionalDamageTakenActionOp
    {
        /// 상태 조건부 피해 행동의 의미를 보관한다.
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
