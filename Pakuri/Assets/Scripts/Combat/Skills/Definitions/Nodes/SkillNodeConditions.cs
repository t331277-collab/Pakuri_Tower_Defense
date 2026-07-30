/*
 * 역할: 스킬 노드 조건 계약.
 * 책임: 시전·상태 요구 조건값을 정의한다.
 */

using Pakuri.Data;

namespace Pakuri.InGame
{
    public readonly struct StatusStackCondition
    {
        public StatusStackCondition(StatusEffectKind statusKind, int minimumStacks)
        {
            StatusKind = statusKind;
            MinimumStacks = minimumStacks;
        }

        public StatusEffectKind StatusKind { get; }
        public int MinimumStacks { get; }
    }

    public readonly struct CastConditionOp
    {
        public CastConditionOp(float targetHealthRatioBonus)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    public readonly struct SourceStatusRequirementOp
    {
        public SourceStatusRequirementOp(StatusEffectKind statusKind, int minimumStacks)
        {
            Condition = new StatusStackCondition(statusKind, minimumStacks);
        }

        public StatusStackCondition Condition { get; }
    }
}
