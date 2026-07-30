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
        public CritModifierOp(float chanceBonus)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    public readonly struct ConditionalDamageActionOp
    {
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
