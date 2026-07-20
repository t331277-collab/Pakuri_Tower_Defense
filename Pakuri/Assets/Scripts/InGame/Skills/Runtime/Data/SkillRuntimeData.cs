using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 능력치 출처에서 사용하는 선택 값을 정의한다.
     */
    public enum StatSource
    {
        Attack,
        Intelligence
    }

    /*
     * 스킬 대상 진영에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillTargetSide
    {
        Enemy,
        Self,
        Ally,
        AllAllies
    }

    /*
     * 스킬 대상 선택 방식에서 사용하는 선택 값을 정의한다.
     */
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

    /*
     * 스킬 대상 형태에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillTargetShape
    {
        Single,
        Line,
        Circle,
        Battlefield
    }

    /*
     * 투사체 이동 방식에서 사용하는 선택 값을 정의한다.
     */
    public enum ProjectileTravelMode
    {
        Straight,
        Homing,
        Arc,
        Instant
    }

    /*
     * 지속 범위 기준점 방식에서 사용하는 선택 값을 정의한다.
     */
    public enum ZoneAnchorMode
    {
        GroundPosition,
        Target,
        Owner,
        Battlefield
    }

    /*
     * 지속 범위 주기 방식에서 사용하는 선택 값을 정의한다.
     */
    public enum ZoneTickMode
    {
        Once,
        OnInterval,
        OnEnter,
        OnExit
    }

    /*
     * 버프 대상에서 사용하는 선택 값을 정의한다.
     */
    public enum BuffTarget
    {
        AllAllies,
        Self
    }

    /*
     * 상태 대상 범위에서 사용하는 선택 값을 정의한다.
     */
    public enum StatusTargetScope
    {
        Unspecified,
        AllAllies,
        Self
    }

    /*
     * 상태 병합 규칙에서 사용하는 선택 값을 정의한다.
     */
    public enum StatusMergePolicy
    {
        Unspecified,
        SameSourceTakeHighest,
        SameSourceRefresh,
        AlwaysStack
    }

    /*
     * 보호막 갱신 규칙에서 사용하는 선택 값을 정의한다.
     */
    public enum ShieldRefreshRule
    {
        Replace,
        TakeHighest,
        Stack
    }

    /*
     * 패시브 트리거에서 사용하는 선택 값을 정의한다.
     */
    public enum PassiveTrigger
    {
        Always,
        DuringBuff,
        AfterSkill,
        OnTargetStatus,
        OnEvent,
        OnHitCount
    }

    /*
     * 패시브 대상에서 사용하는 선택 값을 정의한다.
     */
    public enum PassiveTarget
    {
        AllAllies,
        ElementUsers,
        Self
    }

    /*
     * 스킬 실행 시간 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class SkillTimingSpec
    {
        [Min(0f)] public float Cooldown;
        [Min(0f)] public float CastTime;
        [Min(0f)] public float ActiveDuration;
        [Min(0f)] public float TickInterval;
    }

    /*
     * 스킬 대상 지정 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy;
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusId;
        [Min(0)] public int SelectionStatusMinStacks;
        public SkillTargetShape Shape = SkillTargetShape.Single;
        [Tooltip("Deprecated. InGame skills target the whole battlefield; runtime ignores this value.")]
        [Min(0f)] public float Range;
        [Min(0f)] public float Radius;
        public bool CoverAll;
    }

    /*
     * 스킬 피해 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class SkillDamageSpec
    {
        public string SkillId;
        public DamageAttribute Element;
        [Min(0f)] public float BaseDamage;
        public float StatCoefficient;
        public StatSource StatSource;
        public bool UseCombinedStatCoefficients;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool CriticalAllowed = true;
    }

    /*
     * 상태 적용 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class StatusApplicationSpec
    {
        public RuntimeStatusData Status;
        [Range(0f, 1f)] public float Chance = 1f;
        [Min(0)] public int Stacks = 1;
        public bool RefreshDuration = true;
    }

    /*
     * 투사체 설계값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class ProjectileBlueprintSpec
    {
        [Min(0)] public int MagazineSize;
        [Min(0f)] public float ReloadTime;
        [Min(1)] public int BurstProjectileCount = 1;
        [Min(0f)] public float BurstIntervalSeconds;
        [Min(0)] public int BurstDamageProjectileIndex;
        [Min(0f)] public float BurstDamageMultiplier = 1f;
        [Min(1)] public int ProjectilesPerShot = 1;
        [Min(0)] public int PierceCount;
        [Min(0f)] public float ProjectileSpeed;
        [Min(0f)] public float LifetimeSeconds;
        public ProjectileTravelMode TravelMode = ProjectileTravelMode.Straight;
        public GameObject ProjectilePrefab;
    }

    /*
     * 범위 설계값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class AreaBlueprintSpec
    {
        public ZoneAnchorMode AnchorMode = ZoneAnchorMode.GroundPosition;
        public ZoneTickMode TickMode = ZoneTickMode.OnInterval;
        [Min(0f)] public float DeployDelay;
        [Min(0f)] public float Radius;
        [Min(0f)] public float Duration;
        [Min(0f)] public float TickInterval;
        public bool CoverAll;
    }

    /*
     * 아군 효과 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class AllyEffectSpec
    {
        public bool Enabled;
        [Min(0f)] public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public string BuffTag;
        [Min(0f)] public float BuffDuration;
    }

    /*
     * 버프 보정값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public sealed class BuffModifierSpec
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

    /*
     * 스킬 데이터에 필요한 값을 보관한다.
     */
    public abstract class SkillRuntimeData
    {
        [Header("Identity")]
        public string SkillId;
        public string SkillName;
        public SkillSlot Slot;
        public bool IsActive = true;
        public DamageAttribute Element;
        [TextArea(2, 5)] public string Description;
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
        public SkillChoiceRuntimeData[] EnhancementChoices = Array.Empty<SkillChoiceRuntimeData>();
        public SkillChoiceRuntimeData[] MasterChoices = Array.Empty<SkillChoiceRuntimeData>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
        public SkillExecutionPlanNode[] NormalizedPlanNodes = Array.Empty<SkillExecutionPlanNode>();
    }

    /*
     * 버프 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class BuffSkillRuntimeData : SkillRuntimeData
    {
        [Header("Buff")]
        public float BuffDuration;
        public BuffTarget Target;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public string ApplyStatusTag;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Attached Damage")]
        public bool HasAttachedDamage;
        public SkillDamageSpec AttachedDamage = new SkillDamageSpec();
        public float AttachedDamageRadius;
        public StatusApplicationSpec AttachedStatus = new StatusApplicationSpec();
    }

    /*
     * 회복 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class HealSkillRuntimeData : SkillRuntimeData
    {
        public SkillDamageSpec Healing = new SkillDamageSpec();
        public bool AttachVisualToTarget = true;
    }

    /*
     * 연쇄 공격 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class ChainAttackSkillRuntimeData : SkillRuntimeData
    {
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public float ChainDamageMultiplier = 0.5f;
        public float ChainDelaySeconds = 0.5f;
        public float ChainRadius;
        public bool ExcludePrimaryTarget = true;
    }

    /*
     * 돌진 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class ChargeSkillRuntimeData : SkillRuntimeData
    {
        public float TargetMaxHealthRatio = 1f;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /*
     * 빔 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class BeamSkillRuntimeData : SkillRuntimeData
    {
        [Header("Beam")]
        public float BeamWidth;
        public float BeamLength;
        public float KnockbackDistance;
        public bool StopAtFirstTarget;

        [Header("Tick Damage")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /*
     * 투사체 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class ProjectileSkillRuntimeData : SkillRuntimeData
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
        [Min(0f)] public float ImpactDelaySeconds;
        public GameObject ImpactEffectPrefab;
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        public bool HasImpactArea;
        public AreaBlueprintSpec ImpactArea = new AreaBlueprintSpec();
        public SkillDamageSpec ImpactDamage = new SkillDamageSpec();
        public StatusApplicationSpec ImpactStatus = new StatusApplicationSpec();
    }

    /*
     * 단일 공격 데이터에 필요한 값을 보관한다.
     */
    public sealed class SingleAttackSkillRuntimeData : SkillRuntimeData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool UsePrefabHitbox;
        public bool UseMultiDeployment;
        public bool HitAllTargets;
        [Min(1)] public int HitTargetCount = 1;
        [Min(1)] public int DeploymentCount = 1;
        public string DeploymentRequiredTargetStatusId;
        [Min(0)] public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        [Min(0)] public int TargetStatusStackMaxStacks;
        public string ConsumeTargetStatusId;
        [Range(0f, 1f)] public float ConsumeTargetStatusRatio;
        [Min(0)] public int ConsumeTargetStatusStacks;
        [Min(0f)] public float DamageDelaySeconds;
        [Range(0f, 1f)] public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        [Range(0f, 1f)] public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;

        [Header("Enemy Effect")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public SkillDamageSpec TargetStatusStackDamage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }

    /*
     * 지속 범위 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class ZoneSkillRuntimeData : SkillRuntimeData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool HitAllTargets;
        [Min(1)] public int HitTargetCount = 1;

        [Header("Enemy Effect")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnTickStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }

    /*
     * 보호막 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class ShieldSkillRuntimeData : SkillRuntimeData
    {
        [Header("Shield")]
        public BuffTarget Target;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public ShieldRefreshRule RefreshRule;
        public RuntimeStatusData ShieldStatus;

        [Header("Reflect")]
        public bool CanReflectDamage;
        public float ReflectDamageRate;
        public DamageAttribute ReflectElement;
    }

    /*
     * 패시브 스킬 데이터에 필요한 값을 보관한다.
     */
    public sealed class PassiveSkillRuntimeData : SkillRuntimeData
    {
        [Header("Choices")]
        public SkillChoiceRuntimeData[] BaseModifierChoices = Array.Empty<SkillChoiceRuntimeData>();

        [Header("Trigger")]
        public PassiveTrigger TriggerType;
        public string ConditionTag;
        public int ConditionMinStacks;
        [Range(0f, 1f)] public float TriggerChance = 1f;
        public int TriggerHitCount;
        public float InternalCooldown;

        [Header("Target")]
        public PassiveTarget ApplyTarget;
        public DamageAttribute TargetElement;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();
        public float BuffDuration;

        [Header("Linked Skill")]
        public string LinkedSkillId;
        public float LinkedSkillPowerRate;

        [Header("Secondary Trigger")]
        public bool HasSecondaryTrigger;
        public PassiveTrigger SecondaryTriggerType;
        public string SecondaryConditionTag;
        public int SecondaryConditionMinStacks;
        [Range(0f, 1f)] public float SecondaryTriggerChance = 1f;
        public int SecondaryTriggerHitCount;
    }
}
