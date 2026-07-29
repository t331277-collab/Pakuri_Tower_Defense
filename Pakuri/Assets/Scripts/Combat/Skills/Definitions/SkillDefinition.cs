/*
 * 역할: 스킬 저작 및 런타임 데이터 계약.
 * 책임: 스킬 종류·대상·피해·상태·전달·패시브·Trigger·비주얼·선택지 설정을 정의한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{

    /// <summary><c>SkillSlot</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillSlot
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    /// <summary><c>SkillImplementationState</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    /// <summary><c>SkillRuntimeKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillRuntimeKind
    {
        MagazineProjectile,
        CooldownProjectile,
        LineAttack,
        AreaAttack,
        SingleAttack,
        Field,
        Buff,
        Shield,
        Heal,
        Mark,
        Execute,
        Passive
    }

    /// <summary><c>PassiveModifierKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
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

    /// <summary><c>StatSource</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum StatSource
    {
        Attack,
        Intelligence
    }

    /// <summary><c>SkillTargetSide</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTargetSide
    {
        Enemy,
        Self,
        Ally,
        AllAllies
    }

    /// <summary><c>SkillTargetSelection</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTargetSelection
    {
        Nearest,
        LowestHealth,
        HighestHealth,
        HighestStacks,
        ManualPosition,
        Owner,
        Farthest,
        Random
    }

    /// <summary><c>SkillTargetShape</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTargetShape
    {
        Single,
        Line,
        Circle,
        Battlefield
    }

    /// <summary><c>StatusTargetScope</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum StatusTargetScope
    {
        Unspecified,
        AllAllies,
        Self
    }

    /// <summary><c>StatusMergePolicy</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum StatusMergePolicy
    {
        Unspecified,
        SameSourceTakeHighest,
        SameSourceRefresh,
        AlwaysStack
    }

    /// <summary><c>ShieldRefreshRule</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum ShieldRefreshRule
    {
        Replace,
        TakeHighest,
        Stack
    }

    /// <summary><c>SkillTimingSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class SkillTimingSpec
    {
        public float Cooldown;
        public float ActiveDuration;
        public float TickInterval;
    }

    /// <summary><c>SkillTargetingSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy;
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusId;
        public StatusEffectKind SelectionStatusKind;
        public int SelectionStatusMinStacks;
        public bool HasSelectionSkillAttribute;
        public DamageAttribute SelectionSkillAttribute;
        public SkillTargetShape Shape = SkillTargetShape.Single;
        [Tooltip("Deprecated. InGame skills target the whole battlefield; runtime ignores this value.")]
        public float Range;
        public float Radius;
        public bool CoverAll;
    }

    /// <summary><c>SkillDamageSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class SkillDamageSpec
    {
        public string SkillId;
        public DamageAttribute Element;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool CriticalAllowed = true;
    }

    /// <summary><c>StatusApplicationSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class StatusApplicationSpec
    {
        public StatusRuntimeData Status;
        public float Chance = 1f;
        public int Stacks = 1;
        public bool RefreshDuration = true;
    }

    /// <summary><c>ProjectileBlueprintSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class ProjectileBlueprintSpec
    {
        public int MagazineSize;
        public float ReloadTime;
        public int BurstProjectileCount = 1;
        public float BurstIntervalSeconds;
        public int BurstDamageProjectileIndex;
        public float BurstDamageMultiplier = 1f;
        public int ProjectilesPerShot = 1;
        public int PierceCount;
        public float ProjectileSpeed;
        public float LifetimeSeconds;
    }

    /// <summary><c>AreaBlueprintSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class AreaBlueprintSpec
    {
        public float Radius;
        public float Duration;
        public float TickInterval;
        public bool CoverAll;
    }

    /// <summary><c>BuffModifierSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class BuffModifierSpec
    {
        public float ActionSpeedBonus;
        public float AttackPowerBonus;
        public float SpellPowerBonus;
        public float DamageBonusRate;
        public float ShieldReceivedBonus;
        public float CritChanceBonusRate;
        public float CritDamageBonusRate;
        public float ResistReduction;
        public DamageAttribute ResistReductionElement;
    }

    /// <summary><c>SkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class SkillDefinition
    {

        [Header("Identity")]
        public string SkillId;
        public string SkillName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public bool IsActive = true;
        public DamageAttribute Element;
        [TextArea(2, 5)] public string Description;
        [TextArea(2, 4)] public string Summary;
        public Sprite Icon;

        [Header("Runtime Blueprint")]
        public SkillTimingSpec Timing = new SkillTimingSpec();
        public SkillTargetingSpec Targeting = new SkillTargetingSpec();
        public int MagazineCapacity;
        public float ReloadSeconds;

        [Header("Presentation")]
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();

        [Header("Choices")]
        public SkillChoice[] EnhancementChoices = Array.Empty<SkillChoice>();
        public SkillChoice[] MasterChoices = Array.Empty<SkillChoice>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
    }

    /// <summary><c>BuffSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class BuffSkillDefinition : SkillDefinition
    {
        [Header("Buff")]
        public float BuffDuration;
        public SkillTargetSide Target = SkillTargetSide.AllAllies;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Attached Damage")]
        public bool HasAttachedDamage;
        public SkillDamageSpec AttachedDamage = new SkillDamageSpec();
        public float AttachedDamageRadius;
        public StatusApplicationSpec AttachedStatus = new StatusApplicationSpec();
    }

    /// <summary><c>BuffHealSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class BuffHealSkillDefinition : SkillDefinition
    {
        public SkillDamageSpec Healing = new SkillDamageSpec();
    }

    /// <summary><c>SingleChainSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class SingleChainSkillDefinition : SkillDefinition
    {
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public float ChainDamageMultiplier = 0.5f;
        public float ChainDelaySeconds = 0.5f;
        public float ChainRadius;
        public bool ExcludePrimaryTarget = true;
    }

    /// <summary><c>SingleChargeSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class SingleChargeSkillDefinition : SkillDefinition
    {
        public float TargetMaxHealthRatio = 1f;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /// <summary><c>LineSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class LineSkillDefinition : SkillDefinition
    {
        [Header("Line")]
        public float LineWidth;
        public float LineLength;
        public int CastRepeatCount = 1;
        public float CastRepeatIntervalSeconds;
        public float KnockbackDistance;

        [Header("Tick Damage")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /// <summary><c>ProjectileSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class ProjectileSkillDefinition : SkillDefinition
    {
        [Header("Projectile")]
        public ProjectileBlueprintSpec Projectile = new ProjectileBlueprintSpec();

        [Header("Damage")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();

        [Header("Consecutive Hit")]
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;

        [Header("Impact Area")]
        public bool ContactDamageEnabled = true;
        public bool StopOnFirstHit;
        public float ImpactDelaySeconds;
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        public bool HasImpactArea;
        public AreaBlueprintSpec ImpactArea = new AreaBlueprintSpec();
        public SkillDamageSpec ImpactDamage = new SkillDamageSpec();
        public StatusApplicationSpec ImpactStatus = new StatusApplicationSpec();
    }

    /// <summary><c>SingleSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class SingleSkillDefinition : SkillDefinition
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool UsePrefabHitbox;
        public bool UseMultiDeployment;
        public bool HitAllTargets;
        public int HitTargetCount = 1;
        public int DeploymentCount = 1;
        public string DeploymentRequiredTargetStatusId;
        public StatusEffectKind DeploymentRequiredTargetStatusKind;
        public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        public StatusEffectKind TargetStatusStackStatusKind;
        public int TargetStatusStackMaxStacks;
        public string ConsumeTargetStatusId;
        public StatusEffectKind ConsumeTargetStatusKind;
        public float ConsumeTargetStatusRatio;
        public int ConsumeTargetStatusStacks;
        public float DamageDelaySeconds;
        public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;

        [Header("Enemy Effect")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public SkillDamageSpec TargetStatusStackDamage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /// <summary><c>ZoneSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class ZoneSkillDefinition : SkillDefinition
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool HitAllTargets;
        public int HitTargetCount = 1;

        [Header("Enemy Effect")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnTickStatus = new StatusApplicationSpec();
    }

    /// <summary><c>BuffShieldSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class BuffShieldSkillDefinition : SkillDefinition
    {
        [Header("Shield")]
        public SkillTargetSide Target = SkillTargetSide.AllAllies;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public StatusRuntimeData ShieldStatus;
    }

    /// <summary><c>PassiveSkillDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
    public class PassiveSkillDefinition : SkillDefinition
    {
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;

        [Header("Unit Modifier")]
        public PassiveModifierKind ModifierKind;
        public bool HasModifierAttribute;
        public DamageAttribute ModifierAttribute;
        public float ModifierValue;

        [Header("Choices")]
        public SkillChoice[] BaseModifierChoices = Array.Empty<SkillChoice>();
    }
}

namespace Pakuri.InGame
{

    /// <summary><c>SkillChoice</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    [Serializable]
    public class SkillChoice
    {
        public string ChoiceId;
        public string MonsterId;
        public string SkillId;
        public string TargetSkillId;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
    }
}

namespace Pakuri.Data
{

    /// <summary><c>SkillChoiceGroup</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }

}

namespace Pakuri.Data
{

    /// <summary><c>SkillMultiEffectTargetSide</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    /// <summary><c>SkillMultiEffectTargetSelection</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    /// <summary><c>SkillMultiEffectTargetShape</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    /// <summary><c>SkillMultiEffectCenterMode</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

    /// <summary><c>SkillTriggerEvent</c>에서 지원하는 값의 종류를 정의한다.</summary>
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

    /// <summary><c>SkillTriggerEventSourceScope</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTriggerEventSourceScope
    {
        Any,
        Owner,
        AllAllies
    }

    /// <summary><c>SkillTriggerDamageValueSource</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTriggerDamageValueSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    /// <summary><c>SkillTriggerCenterMode</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTriggerCenterMode
    {
        EventCenter,
        EventTarget,
        Caster
    }

    /// <summary><c>SkillTriggerCommandKind</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillTriggerCommandKind
    {
        RecastZone,
        RefundCooldown,
        ReduceReload,
        ExtendStatusDuration
    }

    /// <summary><c>SkillTriggerCommand</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
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

    /// <summary><c>SkillTriggerDefinition</c>의 저작 데이터와 런타임 설정을 정의한다.</summary>
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

namespace Pakuri.Data
{

    /// <summary><c>SkillMultiEffectVisualAnchorMode</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum SkillMultiEffectVisualAnchorMode
    {
        Center,
        AppliedTargets
    }

    /// <summary><c>RuntimeSkillVisualAnchor</c>에서 지원하는 값의 종류를 정의한다.</summary>
    public enum RuntimeSkillVisualAnchor
    {
        Skill,
        StatusTarget
    }

    /// <summary><c>RuntimeSkillHitboxSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class RuntimeSkillHitboxSpec
    {
        public Vector2 Size;

        /// <summary>소유한 런타임 상태에 <c>Hitbox</c>가 있는지 반환한다.</summary>
        public bool HasHitbox()
        {
            return Size.x > 0f && Size.y > 0f;
        }
    }

    /// <summary><c>RuntimeSkillVisualSpec</c>을 설명하는 설정값을 묶는다.</summary>
    [Serializable]
    public class RuntimeSkillVisualSpec
    {
        public Sprite Sprite;
        public RuntimeAnimatorController AnimatorController;
        public Vector3 LocalScale = Vector3.one;
        public int SortingOrder;
        public RuntimeSkillVisualAnchor Anchor = RuntimeSkillVisualAnchor.Skill;
        public RuntimeSkillHitboxSpec Hitbox = new RuntimeSkillHitboxSpec();

        /// <summary>소유한 런타임 상태에 <c>Visual</c>가 있는지 반환한다.</summary>
        public bool HasVisual()
        {
            return Sprite != null
                || AnimatorController != null
                || (Hitbox != null && Hitbox.HasHitbox());
        }
    }
}
