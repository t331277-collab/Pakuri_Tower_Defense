using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 슬롯, 작성 원본, 공통 실행 설정과 계열별 런타임 Definition 형식을 제공한다.
 * Pakuri.Data 영역은 카탈로그·Choice·Trigger·Node 작성 계약을,
 * Pakuri.InGame 영역은 Projectile·Line·Single·Zone·Buff·Passive 실행 설계도를 보관한다.
 * 이 파일은 값을 정의할 뿐 대상 선택, 피해 적용, Trigger 발동이나 Node 실행은 담당하지 않는다.
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
        public SkillNodeDefinition[] NormalizedNodes = Array.Empty<SkillNodeDefinition>();
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
        public SkillNodeDefinition[] NormalizedNodes = Array.Empty<SkillNodeDefinition>();
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
        public SkillNode[] NormalizedNodes = Array.Empty<SkillNode>();
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
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;

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
        private Dictionary<string, SkillNode[]> runtimeNodesByTarget;

        public SkillChoiceDefinition Source;

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

        /*
         * 같은 Choice가 같은 대상 스킬에 반복 적용될 때 이미 파싱한 실행 노드를 재사용한다.
         * 실행 스냅샷은 매 시전마다 새로 만들어지지만, Choice 정의와 그 정규화 노드는 카탈로그 수명 동안
         * 변하지 않는다. 따라서 문자열 Params를 매번 다시 파싱하지 않고 대상 스킬 ID별 결과를 캐시한다.
         */
        internal bool TryGetRuntimeNodes(string targetSkillId /* 적용 대상 스킬 식별자 */, out SkillNode[] nodes /* 컴파일된 실행 노드 */)
        {
            nodes = null;
            return runtimeNodesByTarget != null
                && runtimeNodesByTarget.TryGetValue(targetSkillId ?? string.Empty, out nodes);
        }

        /*
         * SkillNodeMapper가 대상 스킬에 맞춰 만든 불변 실행 계획을 Choice에 기록한다.
         * 다음 시전부터 같은 Handler 문자열과 Params를 다시 필터링·파싱하지 않는다.
         */
        internal void CacheRuntimeNodes(string targetSkillId /* 적용 대상 스킬 식별자 */, SkillNode[] nodes /* 컴파일된 실행 노드 */)
        {
            if (runtimeNodesByTarget == null)
            {
                runtimeNodesByTarget = new Dictionary<string, SkillNode[]>(StringComparer.OrdinalIgnoreCase);
            }

            runtimeNodesByTarget[targetSkillId ?? string.Empty] = nodes ?? Array.Empty<SkillNode>();
        }
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
        public SkillNodeDefinition[] NormalizedNodes = Array.Empty<SkillNodeDefinition>();
    }
}


/*
 * 한 스킬에 연결되는 추가 효과의 동작을 구분한다.
 */
namespace Pakuri.Data
{
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

    /*
     * 전투 사건의 활성화 조건과 실행할 Node 목록을 보관한다.
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
        public string[] RequiredActiveChoiceIds = Array.Empty<string>();
        public string ExcludesActiveChoiceId;
        public string[] ExcludedActiveChoiceIds = Array.Empty<string>();
        public string RequiredSourceStatusId;
        public StatusEffectKind RequiredSourceStatusKind;
        public int RequiredSourceStatusMinStacks;
        public string ConditionStatusId;
        public StatusConditionGroup[] ConditionStatuses = Array.Empty<StatusConditionGroup>();
        public string ConditionStatusSourceSkillId;
        public string[] ConditionStatusSourceSkillIds = Array.Empty<string>();
        public string TriggerAttribute;
        public DamageAttribute[] TriggerAttributes = Array.Empty<DamageAttribute>();
        public string EventSkillId;
        public string[] EventSkillIds = Array.Empty<string>();
        public string EventSkillRuntimeKinds;
        public SkillRuntimeKindCondition[] EventSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();
        public float ProcChance = 1f;
        public float InternalCooldownSeconds;
        public float TriggerDelaySeconds;
        public int TriggerEveryCount;
        public string EventSourceScope;
        public SkillTriggerEventSourceScope EventSourceScopeValue;
        public int SortOrder;
        public int RepeatCount = 1;
        public float RepeatIntervalSeconds;
        public bool RequireEventExecute;
        public SkillNodeDefinition[] NormalizedNodes = Array.Empty<SkillNodeDefinition>();
        [NonSerialized] public Pakuri.InGame.SkillNode[] Nodes = Array.Empty<Pakuri.InGame.SkillNode>();
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
        public GameObject ResolvedPrefab;
        public RuntimeSkillVisualSpec ResolvedRuntimeVisual;
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
