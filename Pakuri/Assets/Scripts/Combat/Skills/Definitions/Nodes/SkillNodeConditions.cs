/*
 * 역할: 스킬 규칙이 성립할 조건을 정의한다.
 * 책임: 시전, 사건, 상태, 발생원 조건이 후속 결과를 허용할 기준을 제공한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.Data
{
    /// 다중 효과가 어느 진영을 셀지 구분한다.
    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    /// 다중 효과가 기준 대상을 고르는 방식을 구분한다.
    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    /// 다중 효과가 퍼질 공간 형태를 구분한다.
    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    /// 다중 효과가 시작될 중심 기준을 구분한다.
    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

    /// 스킬 반응을 검사할 전투 시점을 구분한다.
    public enum SkillTriggerEvent
    {
        BuildExecutionData,
        OnCast,
        OnDeploymentCast,
        OnHit,
        OnExpire,
        OnHitCount,
        OnMagazineLastProjectileHit,
        OnShieldExpire,
        OnShieldAbsorb,
        OnStatusExpire,
        OnOutgoingDamage,
        OnKill,
        OnSkillCast,
        CombatStart
    }

    /// 반응이 허용할 사건 발생원의 관계를 구분한다.
    public enum SkillTriggerEventSourceScope
    {
        Any,
        Owner,
        AllAllies
    }

    /// 사건에서 후속 피해값을 가져올 기준을 구분한다.
    public enum SkillTriggerDamageValueSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    /// 후속 결과가 발생할 위치 기준을 구분한다.
    public enum SkillTriggerCenterMode
    {
        EventCenter,
        EventTarget,
        Caster
    }

    /// 물리 효과 없이 바꿀 런타임 상태를 구분한다.
    public enum SkillReactionCommandKind
    {
        RefundCooldown,
        ReduceReload,
        ExtendStatusDuration
    }
}

namespace Pakuri.InGame
{
    /// 반응 결과로 수행할 상태 변화를 대상 규칙과 연결한다.
    [Serializable]
    public sealed class SkillReactionCommand
    {
        public SkillReactionCommandKind Kind;
        public string TargetId;
        public StatusEffectKind StatusKind;
        public float Ratio;
        public float DurationSeconds;
        public SkillTargetingSpec Targeting = new SkillTargetingSpec();
        public bool LockToEventTarget;
        public int MaxTargets;
    }

    /// 전투 사건이 언제 어떤 결과로 이어지는지 정의한다.
    [Serializable]
    public sealed class SkillReaction
    {
        public string ReactionId;
        public string SourceSkillId;
        public SkillTriggerEvent Event;
        public string[] RequiredActiveChoiceIds = Array.Empty<string>();
        public string[] ExcludedActiveChoiceIds = Array.Empty<string>();
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public DamageAttribute[] TriggerAttributes = Array.Empty<DamageAttribute>();
        public string[] EventSkillIds = Array.Empty<string>();
        public SkillRuntimeKindCondition[] EventSkillRuntimeKindValues =
            Array.Empty<SkillRuntimeKindCondition>();
        public float ProcChance = 1f;
        public float InternalCooldownSeconds;
        public float DelaySeconds;
        public int EveryCount;
        public SkillTriggerEventSourceScope EventSourceScope;
        public int SortOrder;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public SkillCastEffect Effect;
        public SkillReactionCommand Command;
        public float DamageMultiplier = 1f;
        public SkillTriggerDamageValueSource DamageValueSource;
        public float DamageValueMultiplier = 1f;
        public DamageAttribute TrackedDamageAttribute;
        public bool LockToEventTarget;
        public SkillTriggerCenterMode CenterMode;
        public bool PublishSkillLifecycleEvents;
    }

    /// 사건 반응을 노드에서 해석할 값으로 전달한다.
    public readonly struct SkillReactionOp
    {
        /// 사건 반응을 노드에서 해석할 규칙으로 고정한다.
        public SkillReactionOp(SkillReaction reaction)
        {
            Reaction = reaction;
        }

        public SkillReaction Reaction { get; }
    }

    /// 효과가 요구하는 상태와 최소 중첩을 나타낸다.
    public readonly struct StatusStackCondition
    {
        /// 효과가 요구하는 상태와 최소 중첩을 고정한다.
        public StatusStackCondition(StatusEffectKind statusKind, int minimumStacks)
        {
            StatusKind = statusKind;
            MinimumStacks = minimumStacks;
        }

        public StatusEffectKind StatusKind { get; }
        public int MinimumStacks { get; }
    }

    /// 대상 생명력에 따라 넓어질 시전 기준을 나타낸다.
    public readonly struct CastConditionOp
    {
        /// 시전 허용 범위를 바꿀 기준값을 고정한다.
        public CastConditionOp(float targetHealthRatioBonus)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

}
