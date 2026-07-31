/*
 * 역할: 스킬 Trigger 계약.
 * 책임: 이벤트 조건·지연·반복·후속 스킬·명령 실행 설정을 정의한다.
 */

using System;
using Pakuri.Combat;

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

    public enum SkillTriggerCommandKind
    {
        RecastZone,
        RefundCooldown,
        ReduceReload,
        ExtendStatusDuration
    }

    [Serializable]
    public class SkillTriggerCommand
    {
        public SkillTriggerCommandKind Kind;
        public string TargetId;
        public StatusEffectKind StatusKind;
        public float Ratio;
        public float DurationSeconds;
        public Pakuri.InGame.SkillTargetingSpec Targeting =
            new Pakuri.InGame.SkillTargetingSpec();
        public bool LockToEventTarget;
        public int MaxTargets;
        public float DelaySeconds;
        public float RadiusMultiplier = 1f;
        public bool InheritSnapshot = true;
        public int MaxGeneration = 1;
    }

    [Serializable]
    public class SkillTriggerDefinition
    {
        public string TriggerId;
        public string MonsterId;
        public string SourceSkillId;
        public SkillTriggerEvent TriggerEvent;
        public string[] RequiredActiveChoiceIds = Array.Empty<string>();
        public string[] ExcludedActiveChoiceIds = Array.Empty<string>();
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public DamageAttribute[] TriggerAttributes = Array.Empty<DamageAttribute>();
        public string[] EventSkillIds = Array.Empty<string>();
        public SkillRuntimeKindCondition[] EventSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();
        public float ProcChance = 1f;
        public float InternalCooldownSeconds;
        public float TriggerDelaySeconds;
        public int TriggerEveryCount;
        public SkillTriggerEventSourceScope EventSourceScopeValue;
        public int SortOrder;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public Pakuri.InGame.SkillDefinition TriggeredSkill;
        public Pakuri.InGame.SkillCastEffect Effect;
        public SkillTriggerCommand Command;
        public bool UsesExistingSkillRuntime;
        public float TriggeredDamageMultiplier = 1f;
        public SkillTriggerDamageValueSource DamageValueSource;
        public float DamageValueMultiplier = 1f;
        public DamageAttribute TrackedDamageAttribute;
        public bool LockToEventTarget;
        public SkillTriggerCenterMode CenterMode;
        public bool PublishSkillLifecycleEvents;
    }
}
