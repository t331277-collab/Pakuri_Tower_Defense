using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * CSV 스킬 정의를 전투에서 실행할 때 사용하는 공통 실행 데이터 구조.
 * 대상 지정, 피해, 상태, 투사체, 범위, 버프 설정과 실행 계획을 보관하고
 * 버프·회복·연쇄·돌진·빔·투사체·단일·범위·보호막·패시브별 세부 데이터를 정의한다.
 */
namespace Pakuri.InGame
{
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
     * 스킬 실행 시간 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public class SkillTimingSpec
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
    public class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy;
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusId;
        public StatusEffectKind SelectionStatusKind;
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
    public class SkillDamageSpec
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
    public class StatusApplicationSpec
    {
        public StatusRuntimeData Status;
        [Range(0f, 1f)] public float Chance = 1f;
        [Min(0)] public int Stacks = 1;
        public bool RefreshDuration = true;
    }

    /*
     * 투사체 설계값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public class ProjectileBlueprintSpec
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
    }

    /*
     * 범위 설계값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public class AreaBlueprintSpec
    {
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
    public class AllyEffectSpec
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

    /*
     * 스킬 데이터에 필요한 값을 보관한다.
     */
    public abstract class SkillExecutionDefinition
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
        public SkillChoice[] EnhancementChoices = Array.Empty<SkillChoice>();
        public SkillChoice[] MasterChoices = Array.Empty<SkillChoice>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
        public SkillNode[] NormalizedPlanNodes = Array.Empty<SkillNode>();
    }

    /*
     * 버프 스킬 데이터에 필요한 값을 보관한다.
     */
    public class BuffSkillDefinition : SkillExecutionDefinition
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

    /*
     * 회복 스킬 데이터에 필요한 값을 보관한다.
     */
    public class BuffHealSkillDefinition : SkillExecutionDefinition
    {
        public SkillDamageSpec Healing = new SkillDamageSpec();
        public bool AttachVisualToTarget = true;
    }

    /*
     * Single 계열의 연쇄 공격 값을 보관한다.
     */
    public class SingleChainSkillDefinition : SkillExecutionDefinition
    {
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public float ChainDamageMultiplier = 0.5f;
        public float ChainDelaySeconds = 0.5f;
        public float ChainRadius;
        public bool ExcludePrimaryTarget = true;
    }

    /*
     * Single 계열의 돌진 공격 값을 보관한다.
     */
    public class SingleChargeSkillDefinition : SkillExecutionDefinition
    {
        public float TargetMaxHealthRatio = 1f;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /*
     * Line 스킬 데이터에 필요한 값을 보관한다.
     */
    public class LineSkillDefinition : SkillExecutionDefinition
    {
        [Header("Line")]
        public float LineWidth;
        public float LineLength;
        public float KnockbackDistance;
        public bool StopAtFirstTarget;

        [Header("Tick Damage")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /*
     * 투사체 스킬 데이터에 필요한 값을 보관한다.
     */
    public class ProjectileSkillDefinition : SkillExecutionDefinition
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
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        public bool HasImpactArea;
        public AreaBlueprintSpec ImpactArea = new AreaBlueprintSpec();
        public SkillDamageSpec ImpactDamage = new SkillDamageSpec();
        public StatusApplicationSpec ImpactStatus = new StatusApplicationSpec();
    }

    /*
     * 단일 공격 데이터에 필요한 값을 보관한다.
     */
    public class SingleSkillDefinition : SkillExecutionDefinition
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
        public StatusEffectKind DeploymentRequiredTargetStatusKind;
        [Min(0)] public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        public StatusEffectKind TargetStatusStackStatusKind;
        [Min(0)] public int TargetStatusStackMaxStacks;
        public string ConsumeTargetStatusId;
        public StatusEffectKind ConsumeTargetStatusKind;
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
    public class ZoneSkillDefinition : SkillExecutionDefinition
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
    public class BuffShieldSkillDefinition : SkillExecutionDefinition
    {
        [Header("Shield")]
        public SkillTargetSide Target = SkillTargetSide.AllAllies;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public ShieldRefreshRule RefreshRule;
        public StatusRuntimeData ShieldStatus;

        [Header("Reflect")]
        public bool CanReflectDamage;
        public float ReflectDamageRate;
        public DamageAttribute ReflectElement;
    }

    /*
     * 패시브 스킬 데이터에 필요한 값을 보관한다.
     */
    public class PassiveSkillDefinition : SkillExecutionDefinition
    {
        [Header("Choices")]
        public SkillChoice[] BaseModifierChoices = Array.Empty<SkillChoice>();

        [Header("Trigger")]
        public string ConditionTag;
        public int ConditionMinStacks;
        [Range(0f, 1f)] public float TriggerChance = 1f;
        public int TriggerHitCount;
        public float InternalCooldown;

        [Header("Target")]
        public DamageAttribute TargetElement;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();
        public float BuffDuration;

        [Header("Linked Skill")]
        public string LinkedSkillId;
        public float LinkedSkillPowerRate;

        [Header("Secondary Trigger")]
        public bool HasSecondaryTrigger;
        public string SecondaryConditionTag;
        public int SecondaryConditionMinStacks;
        [Range(0f, 1f)] public float SecondaryTriggerChance = 1f;
        public int SecondaryTriggerHitCount;
    }
}


/*
 * 선택지 원본과 실행용 계산값을 함께 보관한다.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public class SkillChoice
    {
        public SkillChoiceDefinition Source;
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
