/*
 * 역할: 스킬 노드 조건 계약.
 * 책임: 시전·상태 요구 조건값을 정의한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.Data
{
    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

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

    public enum SkillTriggerEventSourceScope
    {
        Any,
        Owner,
        AllAllies
    }

    public enum SkillTriggerDamageValueSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    public enum SkillTriggerCenterMode
    {
        EventCenter,
        EventTarget,
        Caster
    }

    public enum SkillReactionCommandKind
    {
        RecastZone,
        RefundCooldown,
        ReduceReload,
        ExtendStatusDuration
    }
}

namespace Pakuri.InGame
{
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
        public float RadiusMultiplier = 1f;
        public bool InheritSnapshot = true;
        public int MaxGeneration = 1;
    }

    /// Skill/Choice/Passive Node가 소유하는 사건 조건과 공통 실행 보정값.
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
        public string TargetSkillId = string.Empty;
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

    public readonly struct SkillReactionOp
    {
        /// 반응 조건과 결과의 의미를 보관한다.
        public SkillReactionOp(SkillReaction reaction)
        {
            Reaction = reaction;
        }

        public SkillReaction Reaction { get; }
    }

    public readonly struct StatusStackCondition
    {
        /// 상태 중첩 조건의 의미를 보관한다.
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
        /// 시전 조건 보정의 의미를 보관한다.
        public CastConditionOp(float targetHealthRatioBonus)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    public readonly struct SourceStatusRequirementOp
    {
        /// 시전자 상태 조건의 의미를 보관한다.
        public SourceStatusRequirementOp(StatusEffectKind statusKind, int minimumStacks)
        {
            Condition = new StatusStackCondition(statusKind, minimumStacks);
        }

        public StatusStackCondition Condition { get; }
    }
}
