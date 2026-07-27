using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 슬롯, 기본 정의, 상태 적용, 실행 계획을 포함한 스킬 정의 형식을 제공
 CSV에서 만들어진 스킬을 런타임이 이해하는 형식으로 제공 모든 스킬 종류(투사체, 장판, 단일 공격등.. 여러 스킬 정의)
 */
namespace Pakuri.Data
{
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

    /*
     * 스킬 데이터가 런타임에서 지원되는 단계를 구분
     */
    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    /*
     * 스킬을 실행할 공용 런타임 종류를 구분
     */
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

    /*
     * 액티브 스킬의 실행 종류, 피해, 투사체, 상태, 성장 선택지를 보관한다.
     */
    [Serializable]
    public class SkillSourceDefinition
    {
        // 스킬 식별과 표시 정보
        public string SkillId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        [TextArea(2, 5)] public string DescriptionText;
        // 기본 피해와 대상 범위
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float Radius;
        public float LineLength;
        public int CastRepeatCount = 1;
        public float CastRepeatIntervalSeconds;
        public float CastRange;
        public float EffectRadius;
        public string TargetScope;
        // 특수 실행 프로필과 전용 수치
        public string ExecutionProfile;
        public float FlatValue;
        public float ProjectileLifetimeSeconds;
        public float IncomingDamageMultiplier = 1f;
        public float MoveSpeedMultiplier = 1f;
        public float OutgoingDamageMultiplier = 1f;
        public float ChainDamageMultiplier;
        public float ChainDelaySeconds;
        public float ChainRadius;
        public bool ExcludePrimaryTarget;
        public float TargetMaxHealthRatio;
        public float ChargeRampSeconds = 3f;
        public float ChargeMoveSpeedMultiplier = 2.5f;
        public float KnockbackDistance;
        public float DamageDelaySeconds;
        public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;
        // 명중 대상 수와 대상 선택
        public string HitTargetCount;
        public bool UsePrefabHitbox;
        public string TargetSelection;
        public string TargetSelectionStatusId;
        public int TargetSelectionStatusMinStacks;
        // 재사용 대기시간과 투사체 동작
        public float CooldownSeconds;
        public float ActiveDurationSeconds;
        public int MagazineCapacity;
        public float ReloadSeconds;
        public float ShotIntervalSeconds;
        public float BurstIntervalSeconds;
        public int ProjectileBurstCount;
        public int BurstDamageProjectileIndex;
        public float BurstDamageMultiplier = 1f;
        public float ProjectileSpeed;
        public int PierceCount;
        public bool CriticalAllowed = true;
        // 대상 상태 중첩을 이용하는 공격 설정
        public string DeploymentRequiredTargetStatusId;
        public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        public int TargetStatusStackMaxStacks;
        public float TargetStatusStackBaseDamage;
        public float TargetStatusStackAttackPowerCoefficient;
        public float TargetStatusStackSpellPowerCoefficient;
        public string ConsumeTargetStatusId;
        public float ConsumeTargetStatusRatio;
        public int ConsumeTargetStatusStacks;
        // 스킬이 적용할 상태와 능력치 변경값
        public string StatusEffectId;
        public float StatusChance;
        public string StatusEffectLabel;
        public GameObject StatusEffectPrefab;
        public float StatusDurationSeconds;
        public int StatusMaxStacks;
        public int StatusStackAmount;
        public string StatusTargetScope;
        public string StatusMergePolicy;
        public string ShieldAmountRefreshPolicy;
        public float StatusActionSpeedBonus;
        public float StatusMoveSpeedBonus;
        public float StatusAttackPowerBonus;
        public float StatusSpellPowerBonus;
        public float StatusDamageBonusRate;
        public bool StatusPermanent;
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        // 성장 선택지와 추가 효과 그래프
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] MasterSkillChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }

    /*
     * 패시브 스킬의 요구 슬롯, 성장 선택지, 효과 그래프를 보관한다.
     */
    [Serializable]
    public class PassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] BaseModifierChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] PassiveEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }
}

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
        public float Cooldown;
        public float ActiveDuration;
        public float TickInterval;
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
        public int SelectionStatusMinStacks;
        public SkillTargetShape Shape = SkillTargetShape.Single;
        [Tooltip("Deprecated. InGame skills target the whole battlefield; runtime ignores this value.")]
        public float Range;
        public float Radius;
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
        public float BaseDamage;
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
        public float Chance = 1f;
        public int Stacks = 1;
        public bool RefreshDuration = true;
    }

    /*
     * 투사체 설계값 설정에 필요한 값을 보관한다.
     */
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

    /*
     * 범위 설계값 설정에 필요한 값을 보관한다.
     */
    [Serializable]
    public class AreaBlueprintSpec
    {
        public float Radius;
        public float Duration;
        public float TickInterval;
        public bool CoverAll;
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
    public class SkillDefinition
    {
        // 모든 스킬 계열이 공유하는 런타임 Definition의 기본 필드를 구현.
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

    /*
     * 회복 스킬 데이터에 필요한 값을 보관한다.
     */
    public class BuffHealSkillDefinition : SkillDefinition
    {
        public SkillDamageSpec Healing = new SkillDamageSpec();
    }

    /*
     * Single 계열의 연쇄 공격 값을 보관한다.
     */
    public class SingleChainSkillDefinition : SkillDefinition
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
    public class SingleChargeSkillDefinition : SkillDefinition
    {
        public float TargetMaxHealthRatio = 1f;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }

    /*
     * Line 스킬 데이터에 필요한 값을 보관한다.
     */
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

    /*
     * 투사체 스킬 데이터에 필요한 값을 보관한다.
     */
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

    /*
     * 단일 공격 데이터에 필요한 값을 보관한다.
     */
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

    /*
     * 지속 범위 스킬 데이터에 필요한 값을 보관한다.
     */
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

    /*
     * 보호막 스킬 데이터에 필요한 값을 보관한다.
     */
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

    /*
     * 패시브 스킬 데이터에 필요한 값을 보관한다.
     */
    public class PassiveSkillDefinition : SkillDefinition
    {
        [Header("Choices")]
        public SkillChoice[] BaseModifierChoices = Array.Empty<SkillChoice>();
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
        [NonSerialized]
        private Dictionary<string, SkillChoiceRuntimePlan> runtimePlansByTarget;

        public SkillChoiceDefinition Source;

        public string ChoiceId => Source.ChoiceId;

        /*
         * 같은 Choice가 같은 대상 스킬에 반복 적용될 때 이미 파싱한 실행 노드를 재사용한다.
         * 실행 스냅샷은 매 시전마다 새로 만들어지지만, Choice 정의와 그 정규화 노드는 카탈로그 수명 동안
         * 변하지 않는다. 따라서 문자열 Params를 매번 다시 파싱하지 않고 대상 스킬 ID별 결과를 캐시한다.
         */
        internal bool TryGetRuntimePlan(string targetSkillId /* 적용 대상 스킬 식별자 */, out SkillChoiceRuntimePlan plan /* 컴파일된 실행 계획 */)
        {
            plan = null;
            return runtimePlansByTarget != null
                && runtimePlansByTarget.TryGetValue(targetSkillId ?? string.Empty, out plan);
        }

        /*
         * SkillNodeMapper가 대상 스킬에 맞춰 만든 불변 실행 계획을 Choice에 기록한다.
         * 다음 시전부터 같은 Handler 문자열과 Params를 다시 필터링·파싱하지 않는다.
         */
        internal void CacheRuntimePlan(string targetSkillId /* 적용 대상 스킬 식별자 */, SkillChoiceRuntimePlan plan /* 컴파일된 실행 계획 */)
        {
            if (runtimePlansByTarget == null)
            {
                runtimePlansByTarget = new Dictionary<string, SkillChoiceRuntimePlan>(StringComparer.OrdinalIgnoreCase);
            }

            runtimePlansByTarget[targetSkillId ?? string.Empty] = plan ?? SkillChoiceRuntimePlan.Empty;
        }
    }

    /*
     * Choice의 정규화 노드를 특정 대상 스킬에 적용하기 위해 한 번 컴파일한 결과다.
     * 모든 Handler가 SkillNode로 변환되므로 별도 가변 필드 사본은 보관하지 않는다.
     */
    internal sealed class SkillChoiceRuntimePlan
    {
        internal static readonly SkillChoiceRuntimePlan Empty = new SkillChoiceRuntimePlan(
            Array.Empty<SkillNode>());

        internal SkillChoiceRuntimePlan(SkillNode[] nodes /* 컴파일된 실행 노드 */)
        {
            Nodes = nodes ?? Array.Empty<SkillNode>();
        }

        internal SkillNode[] Nodes { get; }
    }
}


/*
 * 선택지가 액티브·패시브의 어느 성장 단계인지 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }

    /*
     * 스킬 성장 선택지가 변경할 전투 수치와 조건을 보관한다.
     */
    [Serializable]
    public class SkillChoiceDefinition
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
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }
}


/*
 * 한 스킬에 연결되는 추가 효과의 동작을 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillMultiEffectKind
    {
        Damage,
        Status,
        ExtendStatusDuration,
        RecastZone
    }

    /*
     * 추가 효과가 적용될 진영을 구분한다.
     */
    public enum SkillMultiEffectTargetSide
    {
        Enemy,
        Self,
        AllAllies
    }

    /*
     * 추가 효과가 대상을 고르는 방식을 구분한다.
     */
    public enum SkillMultiEffectTargetSelection
    {
        Nearest,
        Owner,
        EventTarget
    }

    /*
     * 추가 효과의 대상 범위를 구분한다.
     */
    public enum SkillMultiEffectTargetShape
    {
        Single,
        Circle,
        Battlefield
    }

    /*
     * 추가 효과가 실행되는 시점을 구분한다.
     */
    public enum SkillMultiEffectTiming
    {
        OnCast,
        OnDeploymentCast,
        Delayed,
        OnHit,
        OnExpire,
        OnHitCount
    }

    /*
     * 범위 효과의 중심 위치를 구분한다.
     */
    public enum SkillMultiEffectCenterMode
    {
        EffectTarget,
        PrimarySkillCenter,
        Caster,
        NearestEnemy
    }

    /*
     * 스킬 Trigger를 발생시키는 전투 사건을 구분한다.
     */
    public enum SkillTriggerEvent
    {
        OnMagazineLastProjectileHit,
        OnShieldExpire,
        OnShieldAbsorb,
        OnStatusExpire,
        OnOutgoingDamage,
        OnKill,
        OnSkillCast,
        CombatStart
    }

    /*
     * Trigger 피해가 기준으로 삼을 값을 구분한다.
     */
    public enum SkillTriggerDamageSource
    {
        Fixed,
        ShieldAppliedAmount,
        ShieldRemainingAmount,
        ShieldAbsorbedAmount,
        TrackedIncomingDamage,
        EventAppliedDamage
    }

    /*
     * Trigger가 실행할 결과 동작을 구분한다.
     */
    public enum SkillTriggerActionKind
    {
        Auto,
        SingleAttack,
        LineAttack,
        TriggeredSkill,
        Effect,
        CooldownRefund,
        ReloadReduce
    }

    /*
     * 전투 사건의 조건과 그때 실행할 스킬·효과 정보를 보관한다.
     */
    [Serializable]
    public class SkillTriggerDefinition
    {
        // Trigger 식별과 발생 조건
        public string TriggerId;
        public string MonsterId;
        public string SourceSkillId;
        public SkillTriggerEvent TriggerEvent;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        public string ConditionStatusId;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string ConditionStatusSourceSkillId;
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public string TriggerAttribute;
        // Trigger 실행 동작과 대상 선택
        public SkillTriggerActionKind TriggerAction;
        public string EventSkillId;
        public string EventSkillRuntimeKinds;
        public SkillRuntimeKindCondition[] EventSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();
        public float ProcChance = 1f;
        public float InternalCooldownSeconds;
        public float TriggerDelaySeconds;
        public int TriggerEveryCount;
        public string EventSourceScope;
        public string TriggeredSkillId;
        public string TargetSkillId;
        public string TriggeredEffectId;
        public SkillRuntimeKind RuntimeKind;
        public int SortOrder;
        public SkillMultiEffectTargetSide TargetSide;
        public SkillMultiEffectTargetSelection TargetSelection;
        public SkillMultiEffectTargetShape TargetShape;
        public SkillMultiEffectCenterMode CenterMode;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float DamageMultiplier = 1f;
        public SkillTriggerDamageSource DamageSource;
        public float DamageSourceMultiplier;
        // Trigger 피해 계산
        public DamageAttribute TrackedAttribute;
        public float Radius;
        public bool CoverAll;
        // 반복 실행과 재사용 대기시간 변경
        public string HitTargetCount;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public float CooldownRefundRatio;
        public float ReloadReduceRatio;
        // Trigger 표시와 런타임 지원 상태
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
    }

    /*
     * 기본 스킬에 덧붙일 피해, 상태, 재시전 효과와 실행 조건을 보관한다.
     */
    [Serializable]
    public class SkillEffectDefinition
    {
        // 효과 식별과 실행 시점
        public string EffectId;

        public string RuntimeObjectName(string prefix /* 런타임 오브젝트 이름 앞부분 */)
        {
            return prefix + "_" + EffectId;
        }

        public string SkillId;
        public int SortOrder;
        public SkillMultiEffectKind EffectKind;
        public SkillMultiEffectTargetSide TargetSide;
        public SkillMultiEffectTargetSelection TargetSelection;
        public SkillMultiEffectTargetShape TargetShape;
        public SkillMultiEffectCenterMode CenterMode;
        public SkillMultiEffectVisualAnchorMode VisualAnchorMode;
        public SkillMultiEffectTiming EffectTiming;
        public float DelaySeconds;
        public bool EnabledByDefault;
        public string RequiresActiveChoiceId;
        public string ExcludesActiveChoiceId;
        public string RequiresPassiveSkillId;
        public string ExcludesPassiveSkillId;
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        // 선택지·패시브·상태 기반 실행 조건
        public bool ApplyOnce;
        public string ConditionStatusId;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string ConditionStatusSourceSkillId;
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public SkillMultiEffectTargetSide ConditionTargetSide;
        public string ConditionSkillAttribute;
        public float ConditionHealthRatioMax;
        public int ConditionHitCountMin;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float DamageMultiplier = 1f;
        public float Radius;
        public bool CoverAll;
        public float ActiveDurationSeconds;
        // 피해와 범위 설정
        public float TickIntervalSeconds;
        // 기존 장판을 다시 생성할 때 사용할 설정
        public string RecastSourceSkillId;
        public float RecastDurationSeconds;
        public float RecastRadiusMultiplier = 1f;
        public bool RecastInheritSkillData = true;
        public int RecastMaxGeneration = 1;
        public string StatusEffectId;
        public StatusEffectKind StatusKind;
        public StatusRuntimeData CompiledStatusData;
        public float StatusChance;
        public string StatusEffectLabel;
        public GameObject StatusEffectPrefab;
        public float StatusDurationSeconds;
        public int StatusMaxStacks;
        public int StatusStackAmount;
        public string StatusTargetScope;
        public string StatusMergePolicy;
        public string ShieldAmountRefreshPolicy;
        public float StatusActionSpeedBonus;
        public float StatusMoveSpeedBonus;
        public float StatusAttackPowerBonus;
        public float StatusSpellPowerBonus;
        public float StatusDamageBonusRate;
        // 상태 적용과 상태 능력치 변경
        public float StatusShieldReceivedBonus;
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        public float StatusCriticalChanceBonus;
        public StatusEffectKind[] StatusConditionalTargetStatusKinds = Array.Empty<StatusEffectKind>();
        public float StatusConditionalStatusChanceBonus;
        public string StatusConditionalIncomingSkillRuntimeKinds;
        public string StatusConditionalOutgoingSkillRuntimeKinds;
        public string StatusAppliedStatusDurationBonusStatusId;
        public float StatusAppliedStatusDurationBonus;
        public float StatusOutgoingAdditionalDamageMultiplier;
        public DamageAttribute StatusOutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute StatusOutgoingAdditionalDamageAttribute;
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
    }

    /*
     * 그래프 노드에 전달할 매개변수 하나를 보관한다.
     */
    [Serializable]
    public class SkillNodeParamDefinition
    {
        public string ParamKey;
        public string Value;
    }

    /*
     * 스킬·선택지·Trigger가 소유하는 실행 그래프 노드를 보관한다.
     */
    [Serializable]
    public class SkillNodeDefinition
    {
        public string OwnerKind;
        public string TargetSkillId;
        public string HandlerId;
        public bool EnabledByDefault;
        public SkillNodeParamDefinition[] Params = Array.Empty<SkillNodeParamDefinition>();
    }
}


/*
 * 추가 효과의 시각 오브젝트가 붙을 위치를 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillMultiEffectVisualAnchorMode
    {
        Center,
        AppliedTargets
    }

    /*
     * 스킬 시각 오브젝트가 스킬과 상태 중 어디에 속하는지 구분한다.
     */
    public enum RuntimeSkillVisualAnchor
    {
        Skill,
        StatusTarget
    }

    /*
     * 런타임 스킬 충돌 영역의 크기를 보관한다.
     */
    [Serializable]
    public class RuntimeSkillHitboxSpec
    {
        public Vector2 Size;

        /*
         * 너비와 높이가 모두 설정됐는지 확인한다.
         */
        public bool HasHitbox()
        {
            return Size.x > 0f && Size.y > 0f;
        }
    }

    /*
     * 런타임에서 조합할 스프라이트, 애니메이터, 크기, 충돌 영역을 보관한다.
     */
    [Serializable]
    public class RuntimeSkillVisualSpec
    {
        public Sprite Sprite;
        public RuntimeAnimatorController AnimatorController;
        public Vector3 LocalScale = Vector3.one;
        public int SortingOrder;
        public RuntimeSkillVisualAnchor Anchor = RuntimeSkillVisualAnchor.Skill;
        public RuntimeSkillHitboxSpec Hitbox = new RuntimeSkillHitboxSpec();

        /*
         * 화면 표시나 충돌 영역으로 사용할 데이터가 있는지 확인한다.
         */
        public bool HasVisual()
        {
            return Sprite != null
                || AnimatorController != null
                || (Hitbox != null && Hitbox.HasHitbox());
        }
    }
}
