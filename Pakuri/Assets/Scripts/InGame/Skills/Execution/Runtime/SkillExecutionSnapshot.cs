using Pakuri.Data;
using Pakuri.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 시전 조건 규칙 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum CastConditionOpKind
    {
        TargetHealthRatioBonus
    }

    /*
     * 피해 보정값 규칙 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    /*
     * 치명타 보정값 규칙 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum CritModifierOpKind
    {
        ExecuteChanceBonus
    }

    /*
     * 처치 행동 규칙 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

    /*
     * 스킬 행동 규칙 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillActionOpKind
    {
        DamageMultiplier,
        ShieldAmountMultiplier,
        CountStatusDamageMultiplier,
        CooldownMultiplier,
        MagazineBonus,
        ReloadTimeMultiplier,
        PierceBonus,
        RadiusMultiplier,
        RadiusBonus,
        DurationBonus,
        DurationMultiplier,
        DamageDelayMultiplier,
        AdditionalProjectileBonus,
        ShotIntervalMultiplier,
        ConsecutiveHitDamageBonus,
        BranchDamage,
        StatusStackAmountBonus,
        StatusStackAmountSet,
        StatusMaxStacksBonus,
        ConditionalDamageMultiplier,
        TargetStatusStackDamageRateBonus,
        TriggerProcChanceBonus,
        HitTargetCountBonus,
        StatusActionSpeedBonus,
        StatusAttackPowerBonus,
        StatusAilmentResistanceBonus,
        StatusDamageBonusRate,
        StatusShieldReceivedBonus,
        StatusCriticalChanceBonus,
        StatusDamageTakenBonus,
        StatusFlatElementResistReduction,
        StatusDurationBonus,
        StatusConditionalDamageTakenBonus,
        StatusElementDamageTakenBonus,
        StatusCriticalDamageTakenBonus
    }

    /*
     * 시전 조건 규칙에 필요한 값을 보관한다.
     */
    public readonly struct CastConditionOp
    {
        /*
         * 시전 조건 규칙에 필요한 값을 초기화한다.
         */
        public CastConditionOp(CastConditionOpKind kind, float value)
        {
            Kind = kind;
            Value = value;
        }

        public CastConditionOpKind Kind { get; }
        public float Value { get; }
    }

    /*
     * 피해 보정값 규칙에 필요한 값을 보관한다.
     */
    public readonly struct DamageModifierOp
    {
        /*
         * 피해 보정값 규칙에 필요한 값을 초기화한다.
         */
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    /*
     * 치명타 보정값 규칙에 필요한 값을 보관한다.
     */
    public readonly struct CritModifierOp
    {
        /*
         * 치명타 보정값 규칙에 필요한 값을 초기화한다.
         */
        public CritModifierOp(CritModifierOpKind kind, float chanceBonus)
        {
            Kind = kind;
            ChanceBonus = chanceBonus;
        }

        public CritModifierOpKind Kind { get; }
        public float ChanceBonus { get; }
    }

    /*
     * 처치 행동 규칙에 필요한 값을 보관한다.
     */
    public readonly struct KillActionOp
    {
        /*
         * 처치 행동 규칙에 필요한 값을 초기화한다.
         */
        public KillActionOp(KillActionOpKind kind, float ratioBonus, bool requiresExecute)
        {
            Kind = kind;
            RatioBonus = ratioBonus;
            RequiresExecute = requiresExecute;
        }

        public KillActionOpKind Kind { get; }
        public float RatioBonus { get; }
        public bool RequiresExecute { get; }
    }

    /*
     * 스킬 행동 규칙에 필요한 값을 보관한다.
     */
    public readonly struct SkillActionOp
    {
        /*
         * 스킬 행동 규칙에 필요한 값을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind,
            float floatValue = 0f,
            int intValue = 0,
            string stringValue = null,
            string secondaryStringValue = null,
            SkillMultiEffectTargetSide targetSide = SkillMultiEffectTargetSide.Enemy,
            float secondaryFloatValue = 0f,
            float thirdFloatValue = 0f)
        {
            Kind = kind;
            FloatValue = floatValue;
            IntValue = intValue;
            StringValue = stringValue ?? string.Empty;
            SecondaryStringValue = secondaryStringValue ?? string.Empty;
            TargetSide = targetSide;
            SecondaryFloatValue = secondaryFloatValue;
            ThirdFloatValue = thirdFloatValue;
        }

        public SkillActionOpKind Kind { get; }
        public float FloatValue { get; }
        public int IntValue { get; }
        public string StringValue { get; }
        public string SecondaryStringValue { get; }
        public SkillMultiEffectTargetSide TargetSide { get; }
        public float SecondaryFloatValue { get; }
        public float ThirdFloatValue { get; }
    }

    /*
     * 스킬 실행 실행 정보에 필요한 값을 보관한다.
     */
    public sealed class SkillExecutionSnapshot
    {
        /*
         * 스킬 실행 실행 정보에 필요한 값을 초기화한다.
         */
        public SkillExecutionSnapshot(SkillRuntimeData source)
        {
            Source = source;
            SkillId = source != null ? source.SkillId : string.Empty;
            DamageMultiplier = 1f;
            ShieldAmountMultiplier = 1f;
            CooldownMultiplier = 1f;
            RadiusMultiplier = 1f;
            DurationMultiplier = 1f;
            KnockbackDistanceMultiplier = 1f;
            DamageDelayMultiplier = 1f;
            ReloadTimeMultiplier = 1f;
            ShotIntervalMultiplier = 1f;
            BossDamageMultiplier = 1f;
            BranchDamageMultiplier = 1f;
            OnHitAdditionalDamageMultiplier = 1f;
            OnHitChainDamageMultiplier = 1f;
            SkillEffectPrefab = source != null ? source.SkillEffectPrefab : null;
            AddNormalizedPlanNodes(source != null ? source.NormalizedPlanNodes : null);
            RebuildExecutionPlan();
        }

        public SkillRuntimeData Source { get; }
        public string SkillId { get; }
        public SkillExecutionPlan Plan { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float ShieldAmountMultiplier { get; private set; }
        public float CooldownMultiplier { get; private set; }
        public float RadiusMultiplier { get; private set; }
        public float DurationMultiplier { get; private set; }
        public float BaseDamageBonus { get; private set; }
        public int MagazineBonus { get; private set; }
        public int AdditionalProjectileBonus { get; private set; }
        public int PierceBonus { get; private set; }
        public float ReloadTimeMultiplier { get; private set; }
        public float ShotIntervalMultiplier { get; private set; }
        public int FollowUpProjectileCount { get; private set; }
        public float FollowUpProjectileDelaySeconds { get; private set; }
        public float FollowUpProjectileDamageMultiplier { get; private set; } = 1f;
        public float RadiusBonus { get; private set; }
        public float BeamWidthBonus { get; private set; }
        public float KnockbackDistanceMultiplier { get; private set; }
        public float DamageDelayMultiplier { get; private set; }
        public float ExecuteHealthRatioBonus { get; private set; }
        public float DurationBonus { get; private set; }
        public float BranchChanceBonus { get; private set; }
        public bool HasBranchChanceSet { get; private set; }
        public float BranchChanceSet { get; private set; }
        public bool HasBranchCount { get; private set; }
        public int BranchCount { get; private set; }
        public bool HasBranchDamageMultiplier { get; private set; }
        public float BranchDamageMultiplier { get; private set; }
        public bool HasBranchSearchRadius { get; private set; }
        public float BranchSearchRadius { get; private set; }
        public int BranchLaunchPeriod { get; private set; }
        public bool HasBranchLaunchChanceSet { get; private set; }
        public float BranchLaunchChanceSet { get; private set; }
        public int HitTargetCountBonus { get; private set; }
        public float CritChanceBonus { get; private set; }
        public float CritDamageBonus { get; private set; }
        public float ExecuteCritChanceBonus { get; private set; }
        public float ConsecutiveHitBonusRate { get; private set; }
        public float ConsecutiveHitMax { get; private set; }
        public float BossDamageMultiplier { get; private set; }
        public float KillCooldownRefundRatioBonus { get; private set; }
        public bool KillResetsCooldown { get; private set; }
        public bool KillResetsCooldownRequiresExecute { get; private set; }
        public string StatusTag { get; private set; }
        public float StatusChanceBonus { get; private set; }
        public bool HasStatusActionSpeedBonus { get; private set; }
        public string StatusActionSpeedBonusStatusId { get; private set; }
        public float StatusActionSpeedBonus { get; private set; }
        public bool HasStatusAttackPowerBonus { get; private set; }
        public float StatusAttackPowerBonus { get; private set; }
        public int StatusStacksBonus { get; private set; }
        public bool HasStatusStacksSet { get; private set; }
        public int StatusStacksSet { get; private set; }
        public bool HasStatusElementDamageTakenBonus { get; private set; }
        public float StatusElementDamageTakenBonus { get; private set; }
        public bool HasStatusCriticalDamageTakenBonus { get; private set; }
        public float StatusCriticalDamageTakenBonus { get; private set; }
        public bool HasStatusAilmentResistanceBonus { get; private set; }
        public float StatusAilmentResistanceBonus { get; private set; }
        public bool HasStatusDamageBonusRate { get; private set; }
        public float StatusDamageBonusRate { get; private set; }
        public bool HasStatusShieldReceivedBonus { get; private set; }
        public float StatusShieldReceivedBonus { get; private set; }
        public bool HasStatusCriticalChanceBonus { get; private set; }
        public float StatusCriticalChanceBonus { get; private set; }
        public bool HasStatusDamageTakenBonus { get; private set; }
        public float StatusDamageTakenBonus { get; private set; }
        public bool HasStatusFlatElementResistReduction { get; private set; }
        public float StatusFlatElementResistReduction { get; private set; }
        public bool HasStatusConditionalDamageTakenBonus { get; private set; }
        public float StatusConditionalDamageTakenBonus { get; private set; }
        public string StatusConditionalSourceStatusId { get; private set; }
        public bool HasOnHitAdditionalDamage { get; private set; }
        public float OnHitAdditionalDamageChance { get; private set; }
        public float OnHitAdditionalDamageMultiplier { get; private set; }
        public DamageAttribute OnHitAdditionalDamageAttribute { get; private set; }
        public string OnHitAdditionalDamageTarget { get; private set; }
        public int OnHitChainHitPeriod { get; private set; }
        public int OnHitChainTargetCount { get; private set; }
        public float OnHitChainSearchRadius { get; private set; }
        public float OnHitChainDamageMultiplier { get; private set; }
        public DamageAttribute OnHitChainDamageAttribute { get; private set; }
        public string ReloadReduceTargetSkillId { get; private set; }
        public float ReloadReduceSecondsPerHit { get; private set; }
        public string CoreHitboxName { get; private set; }
        public bool HasCoreDamageMultiplier { get; private set; }
        public float CoreDamageMultiplier { get; private set; } = 1f;
        public bool HasCoreOnHitAdditionalDamage { get; private set; }
        public float CoreOnHitAdditionalDamageChance { get; private set; }
        public float CoreOnHitAdditionalDamageMultiplier { get; private set; } = 1f;
        public DamageAttribute CoreOnHitAdditionalDamageAttribute { get; private set; }
        public string HitCountCooldownRefundTargetSkillId { get; private set; }
        public int HitCountCooldownRefundMinTargets { get; private set; }
        public float HitCountCooldownRefundRatio { get; private set; }
        public int RepeatCountPerTarget { get; private set; }
        public float RepeatIntervalSeconds { get; private set; }
        public float RepeatDamageMultiplier { get; private set; } = 1f;
        public string ThresholdStatusId { get; private set; }
        public int ThresholdStatusMinStacks { get; private set; }
        public string ThresholdApplyStatusId { get; private set; }
        public float TargetStatusStackDamageMultiplier { get; private set; } = 1f;
        public bool HasConsumeTargetStatusRatioOverride { get; private set; }
        public float ConsumeTargetStatusRatioOverride { get; private set; }
        public bool HasConsumeTargetStatusStacksOverride { get; private set; }
        public int ConsumeTargetStatusStacksOverride { get; private set; }
        public float RedistributeConsumedStatusRatioOnKill { get; private set; }
        public string RedistributeConsumedStatusId { get; private set; }
        public float RedistributeConsumedStatusSearchRadius { get; private set; }
        public int RedistributeConsumedStatusTargetCount { get; private set; }
        public GameObject SkillEffectPrefab { get; private set; }
        private readonly HashSet<string> activeChoiceIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> statusActionSpeedBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> targetStatusStackDamageRateBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> triggerProcChanceBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<ConditionalDamageRule> conditionalDamageRules = new List<ConditionalDamageRule>();
        private readonly List<ConditionalCritChanceRule> conditionalCritChanceRules = new List<ConditionalCritChanceRule>();
        private readonly List<BurstDamageRule> burstDamageRules = new List<BurstDamageRule>();
        private readonly List<BurstStatusRule> burstStatusRules = new List<BurstStatusRule>();
        private readonly List<CastConditionOp> castConditionOps = new List<CastConditionOp>();
        private readonly List<DamageModifierOp> damageModifierOps = new List<DamageModifierOp>();
        private readonly List<CritModifierOp> critModifierOps = new List<CritModifierOp>();
        private readonly List<KillActionOp> killActionOps = new List<KillActionOp>();
        private readonly List<SkillExecutionPlanNode> normalizedPlanNodes = new List<SkillExecutionPlanNode>();

        public IReadOnlyList<CastConditionOp> CastConditionOps => castConditionOps;
        public IReadOnlyList<DamageModifierOp> DamageModifierOps => damageModifierOps;
        public IReadOnlyList<CritModifierOp> CritModifierOps => critModifierOps;
        public IReadOnlyList<KillActionOp> KillActionOps => killActionOps;
        public IReadOnlyList<SkillExecutionPlanNode> NormalizedPlanNodes => normalizedPlanNodes;

        public bool HasBranchBehavior =>
            BranchChanceBonus > 0f
            || HasBranchChanceSet
            || HasBranchCount
            || HasBranchDamageMultiplier
            || HasBranchSearchRadius
            || HasBranchLaunchTrigger;

        public bool HasBranchLaunchTrigger =>
            BranchLaunchPeriod > 0
            && HasBranchLaunchChanceSet;

        public bool HasOnHitAdditionalDamageBehavior =>
            HasOnHitAdditionalDamage
            || HasOnHitChainDamageBehavior;

        public bool HasOnHitChainDamageBehavior =>
            OnHitChainHitPeriod > 0
            && OnHitChainTargetCount > 0
            && OnHitChainSearchRadius > 0f
            && OnHitChainDamageMultiplier > 0f;

        public bool HasFollowUpProjectile =>
            FollowUpProjectileCount > 0
            && FollowUpProjectileDamageMultiplier > 0f;

        /*
         * 선택지 설정을 적용한다.
         */
        public void ApplyChoiceSpec(SkillChoiceRuntimeData spec)
        {
            if (spec == null)
            {
                return;
            }

            var choice = spec.Source;
            if (HasNormalizedPlanNodes(choice))
            {
                ApplyNodeBackedChoiceDefinition(choice);
                return;
            }

            if (choice.HasDamageMultiplier)
            {
                DamageMultiplier *= PositiveOrDefault(choice.DamageMultiplier, 1f);
            }

            if (spec.HasShieldAmountMultiplier)
            {
                ShieldAmountMultiplier *= PositiveOrDefault(spec.ShieldAmountMultiplier, 1f);
            }

            BaseDamageBonus += choice.BaseDamageBonus;

            if (choice.HasCooldownMultiplier)
            {
                CooldownMultiplier *= PositiveOrDefault(choice.CooldownMultiplier, 1f);
            }

            if (choice.HasRadiusMultiplier)
            {
                RadiusMultiplier *= PositiveOrDefault(choice.RadiusMultiplier, 1f);
            }

            RadiusBonus += choice.RadiusBonus;
            BeamWidthBonus += choice.BeamWidthBonus;
            if (choice.HasKnockbackDistanceMultiplier)
            {
                KnockbackDistanceMultiplier *= PositiveOrDefault(choice.KnockbackDistanceMultiplier, 1f);
            }

            if (choice.HasDamageDelayMultiplier)
            {
                DamageDelayMultiplier *= PositiveOrDefault(choice.DamageDelayMultiplier, 1f);
            }

            if (choice.HasExecuteHealthRatioBonus)
            {
                ExecuteHealthRatioBonus += choice.ExecuteHealthRatioBonus;
            }

            if (choice.HasDurationMultiplier)
            {
                DurationMultiplier *= PositiveOrDefault(choice.DurationMultiplier, 1f);
            }

            DurationBonus += choice.DurationBonus;

            if (choice.HasMagazineBonus)
            {
                MagazineBonus += choice.MagazineBonus;
            }

            AdditionalProjectileBonus += choice.AdditionalProjectileBonus;
            PierceBonus += choice.PierceBonus;

            if (choice.HasReloadTimeMultiplier)
            {
                ReloadTimeMultiplier *= PositiveOrDefault(choice.ReloadTimeMultiplier, 1f);
            }

            if (choice.HasShotIntervalMultiplier)
            {
                ShotIntervalMultiplier *= PositiveOrDefault(choice.ShotIntervalMultiplier, 1f);
            }

            if (choice.HasBurstDamageMultiplier
                && choice.BurstDamageMultiplier > 0f
                && choice.HasBurstDamageProjectileIndex)
            {
                burstDamageRules.Add(new BurstDamageRule(
                    choice.BurstDamageProjectileIndex,
                    choice.BurstDamageMultiplier));
            }

            if (choice.HasBurstStatusProjectileIndex && choice.BurstStatusStacksBonus != 0)
            {
                burstStatusRules.Add(new BurstStatusRule(
                    choice.BurstStatusProjectileIndex,
                    choice.BurstStatusStacksBonus));
            }

            if (choice.FollowUpProjectileCount > 0)
            {
                FollowUpProjectileCount = choice.FollowUpProjectileCount;
                FollowUpProjectileDelaySeconds = Mathf.Max(0f, choice.FollowUpProjectileDelaySeconds);
                FollowUpProjectileDamageMultiplier = Mathf.Max(0f, choice.FollowUpProjectileDamageMultiplier);
            }

            if (choice.HasStatusChanceBonus)
            {
                StatusChanceBonus += choice.StatusChanceBonus;
            }

            if (choice.HasStatusActionSpeedBonus)
            {
                HasStatusActionSpeedBonus = true;
                if (string.IsNullOrWhiteSpace(spec.StatusActionSpeedBonusStatusId))
                {
                    StatusActionSpeedBonus += choice.StatusActionSpeedBonus;
                }
                else
                {
                    StatusActionSpeedBonusStatusId = spec.StatusActionSpeedBonusStatusId;
                    if (statusActionSpeedBonuses.TryGetValue(spec.StatusActionSpeedBonusStatusId, out var currentBonus))
                    {
                        statusActionSpeedBonuses[spec.StatusActionSpeedBonusStatusId] = currentBonus + choice.StatusActionSpeedBonus;
                    }
                    else
                    {
                        statusActionSpeedBonuses[spec.StatusActionSpeedBonusStatusId] = choice.StatusActionSpeedBonus;
                    }
                }
            }

            if (choice.HasStatusAttackPowerBonus)
            {
                HasStatusAttackPowerBonus = true;
                StatusAttackPowerBonus += choice.StatusAttackPowerBonus;
            }

            BranchChanceBonus += choice.BranchChanceBonus;

            if (choice.HasBranchChanceSet)
            {
                HasBranchChanceSet = true;
                BranchChanceSet = choice.BranchChanceSet;
            }

            if (choice.HasBranchCount)
            {
                HasBranchCount = true;
                BranchCount = choice.BranchCount;
            }

            if (choice.HasBranchDamageMultiplier)
            {
                HasBranchDamageMultiplier = true;
                BranchDamageMultiplier = choice.BranchDamageMultiplier;
            }

            if (choice.HasBranchSearchRadius)
            {
                HasBranchSearchRadius = true;
                BranchSearchRadius = choice.BranchSearchRadius;
            }

            if (choice.BranchLaunchPeriod > 0)
            {
                BranchLaunchPeriod = choice.BranchLaunchPeriod;
            }

            if (choice.HasBranchLaunchChanceSet)
            {
                HasBranchLaunchChanceSet = true;
                BranchLaunchChanceSet = choice.BranchLaunchChanceSet;
            }

            HitTargetCountBonus += choice.HitTargetCountBonus;
            CritChanceBonus += choice.CritChanceBonus;
            CritDamageBonus += choice.CritDamageBonus;
            ExecuteCritChanceBonus += choice.ExecuteCritChanceBonus;
            if (choice.HasBossDamageMultiplier)
            {
                BossDamageMultiplier *= PositiveOrDefault(choice.BossDamageMultiplier, 1f);
            }

            if (choice.HasKillCooldownRefundRatioBonus)
            {
                KillCooldownRefundRatioBonus += choice.KillCooldownRefundRatioBonus;
            }

            if (choice.KillResetsCooldown)
            {
                KillResetsCooldown = true;
            }

            if (choice.KillResetsCooldownRequiresExecute)
            {
                KillResetsCooldownRequiresExecute = true;
            }

            if (!string.IsNullOrWhiteSpace(choice.StatusTag))
            {
                StatusTag = choice.StatusTag;
            }

            StatusStacksBonus += choice.StatusStacksBonus;
            if (choice.HasStatusStacksSet)
            {
                HasStatusStacksSet = true;
                StatusStacksSet = choice.StatusStacksSet;
            }

            if (choice.HasStatusElementDamageTakenBonus)
            {
                HasStatusElementDamageTakenBonus = true;
                StatusElementDamageTakenBonus += choice.StatusElementDamageTakenBonus;
            }

            if (choice.HasStatusCriticalDamageTakenBonus)
            {
                HasStatusCriticalDamageTakenBonus = true;
                StatusCriticalDamageTakenBonus += choice.StatusCriticalDamageTakenBonus;
            }

            if (choice.HasStatusAilmentResistanceBonus)
            {
                HasStatusAilmentResistanceBonus = true;
                StatusAilmentResistanceBonus += choice.StatusAilmentResistanceBonus;
            }

            if (spec.HasStatusDamageBonusRate)
            {
                HasStatusDamageBonusRate = true;
                StatusDamageBonusRate += spec.StatusDamageBonusRate;
            }

            if (spec.HasStatusShieldReceivedBonus)
            {
                HasStatusShieldReceivedBonus = true;
                StatusShieldReceivedBonus += spec.StatusShieldReceivedBonus;
            }

            if (spec.HasStatusCriticalChanceBonus)
            {
                HasStatusCriticalChanceBonus = true;
                StatusCriticalChanceBonus += spec.StatusCriticalChanceBonus;
            }

            if (spec.HasStatusDamageTakenBonus)
            {
                HasStatusDamageTakenBonus = true;
                StatusDamageTakenBonus += spec.StatusDamageTakenBonus;
            }

            if (spec.HasStatusFlatElementResistReduction)
            {
                HasStatusFlatElementResistReduction = true;
                StatusFlatElementResistReduction += spec.StatusFlatElementResistReduction;
            }

            if (!string.IsNullOrWhiteSpace(choice.StatusMaxStacksBonusStatusId)
                && choice.StatusMaxStacksBonus != 0)
            {
                if (statusMaxStacksBonuses.TryGetValue(choice.StatusMaxStacksBonusStatusId, out var currentBonus))
                {
                    statusMaxStacksBonuses[choice.StatusMaxStacksBonusStatusId] = currentBonus + choice.StatusMaxStacksBonus;
                }
                else
                {
                    statusMaxStacksBonuses[choice.StatusMaxStacksBonusStatusId] = choice.StatusMaxStacksBonus;
                }
            }

            if (!string.IsNullOrWhiteSpace(choice.StatusDurationBonusStatusId)
                && !Mathf.Approximately(choice.StatusDurationBonus, 0f))
            {
                if (statusDurationBonuses.TryGetValue(choice.StatusDurationBonusStatusId, out var currentBonus))
                {
                    statusDurationBonuses[choice.StatusDurationBonusStatusId] = currentBonus + choice.StatusDurationBonus;
                }
                else
                {
                    statusDurationBonuses[choice.StatusDurationBonusStatusId] = choice.StatusDurationBonus;
                }
            }

            if (choice.HasStatusConditionalDamageTakenBonus)
            {
                HasStatusConditionalDamageTakenBonus = true;
                StatusConditionalDamageTakenBonus = choice.StatusConditionalDamageTakenBonus;
                StatusConditionalSourceStatusId = choice.StatusConditionalSourceStatusId;
            }

            if (choice.HasOnHitAdditionalDamage)
            {
                HasOnHitAdditionalDamage = true;
                OnHitAdditionalDamageChance = Mathf.Clamp01(choice.OnHitAdditionalDamageChance);
                OnHitAdditionalDamageMultiplier = Mathf.Max(0f, choice.OnHitAdditionalDamageMultiplier);
                OnHitAdditionalDamageAttribute = choice.OnHitAdditionalDamageAttribute;
                OnHitAdditionalDamageTarget = choice.OnHitAdditionalDamageTarget;
            }

            if (choice.OnHitChainHitPeriod > 0)
            {
                OnHitChainHitPeriod = choice.OnHitChainHitPeriod;
            }

            if (choice.OnHitChainTargetCount > 0)
            {
                OnHitChainTargetCount = choice.OnHitChainTargetCount;
            }

            if (choice.OnHitChainSearchRadius > 0f)
            {
                OnHitChainSearchRadius = choice.OnHitChainSearchRadius;
            }

            if (choice.OnHitChainDamageMultiplier > 0f)
            {
            OnHitChainDamageMultiplier = choice.OnHitChainDamageMultiplier;
            }

            OnHitChainDamageAttribute = choice.OnHitChainDamageAttribute;

            if (!string.IsNullOrWhiteSpace(choice.ReloadReduceTargetSkillId)
                && choice.ReloadReduceSecondsPerHit > 0f)
            {
                ReloadReduceTargetSkillId = choice.ReloadReduceTargetSkillId;
                ReloadReduceSecondsPerHit += choice.ReloadReduceSecondsPerHit;
            }

            if (!string.IsNullOrWhiteSpace(choice.CoreHitboxName))
            {
                CoreHitboxName = choice.CoreHitboxName.Trim();
            }

            if (choice.HasCoreDamageMultiplier)
            {
                HasCoreDamageMultiplier = true;
                CoreDamageMultiplier = PositiveOrDefault(choice.CoreDamageMultiplier, 1f);
            }

            if (choice.HasCoreOnHitAdditionalDamage)
            {
                HasCoreOnHitAdditionalDamage = true;
                CoreOnHitAdditionalDamageChance = Mathf.Clamp01(choice.CoreOnHitAdditionalDamageChance);
                CoreOnHitAdditionalDamageMultiplier = Mathf.Max(0f, choice.CoreOnHitAdditionalDamageMultiplier);
                CoreOnHitAdditionalDamageAttribute = choice.CoreOnHitAdditionalDamageAttribute;
            }

            if (!string.IsNullOrWhiteSpace(choice.HitCountCooldownRefundTargetSkillId)
                && choice.HitCountCooldownRefundMinTargets > 0
                && choice.HitCountCooldownRefundRatio > 0f)
            {
                HitCountCooldownRefundTargetSkillId = choice.HitCountCooldownRefundTargetSkillId;
                HitCountCooldownRefundMinTargets = choice.HitCountCooldownRefundMinTargets;
                HitCountCooldownRefundRatio = Mathf.Clamp01(choice.HitCountCooldownRefundRatio);
            }

            if (choice.RepeatCountPerTarget > 0)
            {
                RepeatCountPerTarget += choice.RepeatCountPerTarget;
                RepeatIntervalSeconds = Mathf.Max(RepeatIntervalSeconds, choice.RepeatIntervalSeconds);
                if (choice.RepeatDamageMultiplier > 0f)
                {
                    RepeatDamageMultiplier *= PositiveOrDefault(choice.RepeatDamageMultiplier, 1f);
                }
            }

            if (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId)
                && choice.ThresholdStatusMinStacks > 0
                && !string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId))
            {
                ThresholdStatusId = choice.ThresholdStatusId;
                ThresholdStatusMinStacks = choice.ThresholdStatusMinStacks;
                ThresholdApplyStatusId = choice.ThresholdApplyStatusId;
            }

            if (choice.HasTargetStatusStackDamageMultiplier && choice.TargetStatusStackDamageMultiplier > 0f)
            {
                TargetStatusStackDamageMultiplier *= PositiveOrDefault(choice.TargetStatusStackDamageMultiplier, 1f);
            }

            if (choice.HasConsumeTargetStatusRatioOverride)
            {
                HasConsumeTargetStatusRatioOverride = true;
                ConsumeTargetStatusRatioOverride = Mathf.Clamp01(choice.ConsumeTargetStatusRatioOverride);
            }

            if (choice.HasConsumeTargetStatusStacksOverride)
            {
                HasConsumeTargetStatusStacksOverride = true;
                ConsumeTargetStatusStacksOverride = Mathf.Max(0, choice.ConsumeTargetStatusStacksOverride);
            }

            if (choice.HasConditionalDamageMultiplier
                && choice.ConditionalDamageMultiplier > 0f
                && !string.IsNullOrWhiteSpace(choice.ConditionalTargetStatusId)
                && choice.ConditionalTargetStatusMinStacks > 0)
            {
                conditionalDamageRules.Add(new ConditionalDamageRule(
                    choice.ConditionalDamageMultiplier,
                    choice.ConditionalTargetStatusId,
                    choice.ConditionalTargetStatusMinStacks));
            }

            if (!Mathf.Approximately(choice.ConditionalCritChanceBonus, 0f)
                && !string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId)
                && choice.ConditionalCritTargetStatusMinStacks > 0)
            {
                conditionalCritChanceRules.Add(new ConditionalCritChanceRule(
                    choice.ConditionalCritChanceBonus,
                    choice.ConditionalCritTargetStatusId,
                    choice.ConditionalCritTargetStatusMinStacks));
            }

            if (choice.RedistributeConsumedStatusRatioOnKill > 0f
                && !string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId)
                && choice.RedistributeConsumedStatusSearchRadius > 0f)
            {
                RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(choice.RedistributeConsumedStatusRatioOnKill);
                RedistributeConsumedStatusId = choice.RedistributeConsumedStatusId;
                RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, choice.RedistributeConsumedStatusSearchRadius);
                RedistributeConsumedStatusTargetCount = Mathf.Max(0, choice.RedistributeConsumedStatusTargetCount);
            }

            if (choice.ConsecutiveHitBonusRate > 0f)
            {
                ConsecutiveHitBonusRate = Mathf.Max(0f, choice.ConsecutiveHitBonusRate);
            }

            if (choice.ConsecutiveHitMax > 0f)
            {
                ConsecutiveHitMax = Mathf.Max(0f, choice.ConsecutiveHitMax);
            }

            if (choice.SkillEffectPrefab != null)
            {
                SkillEffectPrefab = choice.SkillEffectPrefab;
            }

            AddNormalizedPlanNodes(spec.PlanNodes);
            RefreshSingleAttackOperationBridges();
            RebuildExecutionPlan();
        }

        /*
         * 동적 피해 배율을 적용한다.
         */
        public void ApplyDynamicDamageMultiplier(float multiplier)
        {
            DamageMultiplier *= PositiveOrDefault(multiplier, 1f);
        }

        /*
         * 노드 기반 선택지 정의를 적용한다.
         */
        private void ApplyNodeBackedChoiceDefinition(SkillChoiceDefinition choice)
        {
            if (choice.SkillEffectPrefab != null)
            {
                SkillEffectPrefab = choice.SkillEffectPrefab;
            }

            var targetNodes = SkillRuntimeCompiler.FilterSkillNodeDefinitionsForTarget(
                choice.NormalizedPlanNodes,
                SkillId);
            var compatibilitySpec = new SkillChoiceRuntimeData { Source = new SkillChoiceDefinition() };
            SkillRuntimeCompiler.ApplyNormalizedChoiceCompatibilityNodes(
                compatibilitySpec,
                targetNodes);
            ApplyNodeBackedChoiceFields(compatibilitySpec);

            var nodes = SkillRuntimeCompiler.MapSkillNodeDefinitions(targetNodes);
            AddNormalizedPlanNodes(nodes);
            ApplyPlanActionNodes(nodes);
            RefreshSingleAttackOperationBridges();
            RebuildExecutionPlan();
        }

        /*
         * 노드 기반 선택지 필드를 적용한다.
         */
        private void ApplyNodeBackedChoiceFields(SkillChoiceRuntimeData spec)
        {
            var choice = spec.Source;
            if (choice.HasBurstDamageMultiplier
                && choice.BurstDamageMultiplier > 0f
                && choice.HasBurstDamageProjectileIndex)
            {
                burstDamageRules.Add(new BurstDamageRule(
                    choice.BurstDamageProjectileIndex,
                    choice.BurstDamageMultiplier));
            }

            if (choice.HasBurstStatusProjectileIndex && choice.BurstStatusStacksBonus != 0)
            {
                burstStatusRules.Add(new BurstStatusRule(
                    choice.BurstStatusProjectileIndex,
                    choice.BurstStatusStacksBonus));
            }

            if (choice.FollowUpProjectileCount > 0)
            {
                FollowUpProjectileCount = choice.FollowUpProjectileCount;
                FollowUpProjectileDelaySeconds = Mathf.Max(0f, choice.FollowUpProjectileDelaySeconds);
                FollowUpProjectileDamageMultiplier = Mathf.Max(0f, choice.FollowUpProjectileDamageMultiplier);
            }

            if (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId)
                && choice.ThresholdStatusMinStacks > 0
                && !string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId))
            {
                ThresholdStatusId = choice.ThresholdStatusId;
                ThresholdStatusMinStacks = choice.ThresholdStatusMinStacks;
                ThresholdApplyStatusId = choice.ThresholdApplyStatusId;
            }

            if (choice.HasTargetStatusStackDamageMultiplier && choice.TargetStatusStackDamageMultiplier > 0f)
            {
                TargetStatusStackDamageMultiplier *= PositiveOrDefault(choice.TargetStatusStackDamageMultiplier, 1f);
            }

            if (choice.HasConsumeTargetStatusRatioOverride)
            {
                HasConsumeTargetStatusRatioOverride = true;
                ConsumeTargetStatusRatioOverride = Mathf.Clamp01(choice.ConsumeTargetStatusRatioOverride);
            }

            if (choice.RepeatCountPerTarget > 0)
            {
                RepeatCountPerTarget += choice.RepeatCountPerTarget;
                RepeatIntervalSeconds = Mathf.Max(RepeatIntervalSeconds, choice.RepeatIntervalSeconds);
                if (choice.RepeatDamageMultiplier > 0f)
                {
                    RepeatDamageMultiplier *= PositiveOrDefault(choice.RepeatDamageMultiplier, 1f);
                }
            }

            if (!Mathf.Approximately(choice.ConditionalCritChanceBonus, 0f)
                && !string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId)
                && choice.ConditionalCritTargetStatusMinStacks > 0)
            {
                conditionalCritChanceRules.Add(new ConditionalCritChanceRule(
                    choice.ConditionalCritChanceBonus,
                    choice.ConditionalCritTargetStatusId,
                    choice.ConditionalCritTargetStatusMinStacks));
            }

            if (choice.RedistributeConsumedStatusRatioOnKill > 0f
                && !string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId)
                && choice.RedistributeConsumedStatusSearchRadius > 0f)
            {
                RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(choice.RedistributeConsumedStatusRatioOnKill);
                RedistributeConsumedStatusId = choice.RedistributeConsumedStatusId;
                RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, choice.RedistributeConsumedStatusSearchRadius);
                RedistributeConsumedStatusTargetCount = Mathf.Max(0, choice.RedistributeConsumedStatusTargetCount);
            }
        }

        /*
         * 정규화된 계획 노드를 보유하고 있는지 확인한다.
         */
        private static bool HasNormalizedPlanNodes(SkillChoiceDefinition choice)
        {
            return choice != null
                && choice.NormalizedPlanNodes != null
                && choice.NormalizedPlanNodes.Length > 0;
        }

        /*
         * 계획 행동 노드를 적용한다.
         */
        private void ApplyPlanActionNodes(IReadOnlyList<SkillExecutionPlanNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var action = nodes[i] != null ? nodes[i].Action : null;
                if (action.HasValue)
                {
                    ApplyPlanAction(action.Value);
                }
            }
        }

        /*
         * 계획 행동을 적용한다.
         */
        private void ApplyPlanAction(SkillActionOp action)
        {
            // 각 노드는 기존 실행값에 곱하거나 더하며, 별도 규칙이 필요한 값은 전용 목록에 보관한다.
            switch (action.Kind)
            {
                case SkillActionOpKind.DamageMultiplier:
                    DamageMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.ShieldAmountMultiplier:
                    ShieldAmountMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.CooldownMultiplier:
                    CooldownMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.MagazineBonus:
                    MagazineBonus += action.IntValue;
                    break;
                case SkillActionOpKind.ReloadTimeMultiplier:
                    ReloadTimeMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.PierceBonus:
                    PierceBonus += action.IntValue;
                    break;
                case SkillActionOpKind.RadiusMultiplier:
                    RadiusMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.RadiusBonus:
                    RadiusBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.DurationBonus:
                    DurationBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.DurationMultiplier:
                    DurationMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.DamageDelayMultiplier:
                    DamageDelayMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.AdditionalProjectileBonus:
                    AdditionalProjectileBonus += action.IntValue;
                    break;
                case SkillActionOpKind.ShotIntervalMultiplier:
                    ShotIntervalMultiplier *= PositiveOrDefault(action.FloatValue, 1f);
                    break;
                case SkillActionOpKind.ConsecutiveHitDamageBonus:
                    ConsecutiveHitBonusRate += Mathf.Max(0f, action.FloatValue);
                    ConsecutiveHitMax += Mathf.Max(0f, action.SecondaryFloatValue);
                    break;
                case SkillActionOpKind.BranchDamage:
                    BranchChanceBonus += action.FloatValue;
                    if (action.IntValue > 0)
                    {
                        HasBranchCount = true;
                        BranchCount = action.IntValue;
                    }
                    if (action.SecondaryFloatValue > 0f)
                    {
                        HasBranchDamageMultiplier = true;
                        BranchDamageMultiplier = action.SecondaryFloatValue;
                    }
                    if (action.ThirdFloatValue > 0f)
                    {
                        HasBranchSearchRadius = true;
                        BranchSearchRadius = action.ThirdFloatValue;
                    }
                    break;
                case SkillActionOpKind.StatusStackAmountBonus:
                    StatusStacksBonus += action.IntValue;
                    break;
                case SkillActionOpKind.StatusStackAmountSet:
                    HasStatusStacksSet = true;
                    StatusStacksSet = Mathf.Max(0, action.IntValue);
                    break;
                case SkillActionOpKind.StatusMaxStacksBonus:
                    if (!string.IsNullOrWhiteSpace(action.StringValue) && action.IntValue != 0)
                    {
                        statusMaxStacksBonuses.TryGetValue(action.StringValue, out var currentMaxStacksBonus);
                        statusMaxStacksBonuses[action.StringValue] = currentMaxStacksBonus + action.IntValue;
                    }
                    break;
                case SkillActionOpKind.ConditionalDamageMultiplier:
                    AddConditionalDamageRule(action.FloatValue, action.StringValue, action.IntValue);
                    break;
                case SkillActionOpKind.TargetStatusStackDamageRateBonus:
                    if (!string.IsNullOrWhiteSpace(action.StringValue) && !Mathf.Approximately(action.FloatValue, 0f))
                    {
                        targetStatusStackDamageRateBonuses.TryGetValue(action.StringValue, out var currentRateBonus);
                        targetStatusStackDamageRateBonuses[action.StringValue] = currentRateBonus + action.FloatValue;
                    }
                    break;
                case SkillActionOpKind.TriggerProcChanceBonus:
                    if (!string.IsNullOrWhiteSpace(action.StringValue) && !Mathf.Approximately(action.FloatValue, 0f))
                    {
                        triggerProcChanceBonuses.TryGetValue(action.StringValue, out var currentProcBonus);
                        triggerProcChanceBonuses[action.StringValue] = currentProcBonus + action.FloatValue;
                    }
                    break;
                case SkillActionOpKind.HitTargetCountBonus:
                    HitTargetCountBonus += action.IntValue;
                    break;
                case SkillActionOpKind.StatusActionSpeedBonus:
                    ApplyStatusActionSpeedBonus(action.StringValue, action.FloatValue);
                    break;
                case SkillActionOpKind.StatusAttackPowerBonus:
                    HasStatusAttackPowerBonus = true;
                    StatusAttackPowerBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusAilmentResistanceBonus:
                    HasStatusAilmentResistanceBonus = true;
                    StatusAilmentResistanceBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusDamageBonusRate:
                    HasStatusDamageBonusRate = true;
                    StatusDamageBonusRate += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusShieldReceivedBonus:
                    HasStatusShieldReceivedBonus = true;
                    StatusShieldReceivedBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusCriticalChanceBonus:
                    HasStatusCriticalChanceBonus = true;
                    StatusCriticalChanceBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusDamageTakenBonus:
                    HasStatusDamageTakenBonus = true;
                    StatusDamageTakenBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusFlatElementResistReduction:
                    HasStatusFlatElementResistReduction = true;
                    StatusFlatElementResistReduction += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusDurationBonus:
                    ApplyStatusDurationBonus(action.StringValue, action.FloatValue);
                    break;
                case SkillActionOpKind.StatusConditionalDamageTakenBonus:
                    HasStatusConditionalDamageTakenBonus = true;
                    StatusConditionalDamageTakenBonus += action.FloatValue;
                    StatusConditionalSourceStatusId = action.StringValue;
                    break;
                case SkillActionOpKind.StatusElementDamageTakenBonus:
                    HasStatusElementDamageTakenBonus = true;
                    StatusElementDamageTakenBonus += action.FloatValue;
                    break;
                case SkillActionOpKind.StatusCriticalDamageTakenBonus:
                    HasStatusCriticalDamageTakenBonus = true;
                    StatusCriticalDamageTakenBonus += action.FloatValue;
                    break;
            }
        }

        /*
         * 상태 행동 속도 보너스를 적용한다.
         */
        private void ApplyStatusActionSpeedBonus(string statusId, float bonus)
        {
            HasStatusActionSpeedBonus = true;
            if (string.IsNullOrWhiteSpace(statusId))
            {
                StatusActionSpeedBonus += bonus;
                return;
            }

            StatusActionSpeedBonusStatusId = statusId;
            statusActionSpeedBonuses[statusId] = statusActionSpeedBonuses.TryGetValue(statusId, out var currentBonus)
                ? currentBonus + bonus
                : bonus;
        }

        /*
         * 상태 지속시간 보너스를 적용한다.
         */
        private void ApplyStatusDurationBonus(string statusId, float bonus)
        {
            if (string.IsNullOrWhiteSpace(statusId) || Mathf.Approximately(bonus, 0f))
            {
                return;
            }

            statusDurationBonuses[statusId] = statusDurationBonuses.TryGetValue(statusId, out var currentBonus)
                ? currentBonus + bonus
                : bonus;
        }

        /*
         * 조건부 피해 규칙을 추가한다.
         */
        private void AddConditionalDamageRule(float multiplier, string statusId, int minStacks)
        {
            if (string.IsNullOrWhiteSpace(statusId) || multiplier <= 0f)
            {
                return;
            }

            conditionalDamageRules.Add(new ConditionalDamageRule(multiplier, statusId, Mathf.Max(1, minStacks)));
        }

        /*
         * 활성 선택지 ID를 추가한다.
         */
        public void AddActiveChoiceId(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                activeChoiceIds.Add(choiceId);
            }
        }

        /*
         * 활성 선택지를 보유하고 있는지 확인한다.
         */
        public bool HasActiveChoice(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && activeChoiceIds.Contains(choiceId);
        }

        /*
         * 상태 지속시간 보너스를 결정한다.
         */
        public float ResolveStatusDurationBonus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0f;
            }

            return statusDurationBonuses.TryGetValue(statusId, out var bonus) ? bonus : 0f;
        }

        /*
         * 상태 행동 속도 보너스를 결정한다.
         */
        public float ResolveStatusActionSpeedBonus(string statusId)
        {
            var bonus = StatusActionSpeedBonus;
            if (!string.IsNullOrWhiteSpace(statusId)
                && statusActionSpeedBonuses.TryGetValue(statusId, out var targetedBonus))
            {
                bonus += targetedBonus;
            }

            return bonus;
        }

        /*
         * 상태 최대 중첩 보너스를 결정한다.
         */
        public int ResolveStatusMaxStacksBonus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            return statusMaxStacksBonuses.TryGetValue(statusId, out var bonus) ? bonus : 0;
        }

        /*
         * 대상 상태 중첩 피해 비율 보너스를 결정한다.
         */
        public float ResolveTargetStatusStackDamageRateBonus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0f;
            }

            return targetStatusStackDamageRateBonuses.TryGetValue(statusId, out var bonus) ? bonus : 0f;
        }

        /*
         * 트리거 발동 확률 보너스를 결정한다.
         */
        public float ResolveTriggerProcChanceBonus(string triggerId)
        {
            if (string.IsNullOrWhiteSpace(triggerId))
            {
                return 0f;
            }

            return triggerProcChanceBonuses.TryGetValue(triggerId, out var bonus) ? bonus : 0f;
        }

        /*
         * 조건부 피해 배율을 결정한다.
         */
        public float ResolveConditionalDamageMultiplier(BaseUnitRuntimeModel target)
        {
            if (target == null || conditionalDamageRules.Count == 0)
            {
                return 1f;
            }

            var multiplier = 1f;
            for (var i = 0; i < conditionalDamageRules.Count; i++)
            {
                var rule = conditionalDamageRules[i];
                if (!HasRequiredStacks(target, rule.StatusId, rule.MinStacks))
                {
                    continue;
                }

                multiplier *= PositiveOrDefault(rule.DamageMultiplier, 1f);
            }

            return multiplier;
        }

        /*
         * 조건부 치명타 확률 보너스를 결정한다.
         */
        public float ResolveConditionalCritChanceBonus(BaseUnitRuntimeModel target)
        {
            if (target == null || conditionalCritChanceRules.Count == 0)
            {
                return 0f;
            }

            var bonus = 0f;
            for (var i = 0; i < conditionalCritChanceRules.Count; i++)
            {
                var rule = conditionalCritChanceRules[i];
                if (!HasRequiredStacks(target, rule.StatusId, rule.MinStacks))
                {
                    continue;
                }

                bonus += rule.CritChanceBonus;
            }

            return bonus;
        }

        /*
         * 연속 발사 피해 배율을 결정한다.
         */
        public float ResolveBurstDamageMultiplier(int projectileIndex, int burstProjectileCount)
        {
            if (projectileIndex <= 0 || burstDamageRules.Count == 0)
            {
                return 1f;
            }

            var multiplier = 1f;
            for (var i = 0; i < burstDamageRules.Count; i++)
            {
                var rule = burstDamageRules[i];
                if (!MatchesBurstProjectileIndex(rule.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    continue;
                }

                multiplier *= PositiveOrDefault(rule.DamageMultiplier, 1f);
            }

            return multiplier;
        }

        /*
         * 연속 발사 상태 중첩 보너스를 결정한다.
         */
        public int ResolveBurstStatusStacksBonus(int projectileIndex, int burstProjectileCount)
        {
            if (projectileIndex <= 0 || burstStatusRules.Count == 0)
            {
                return 0;
            }

            var bonus = 0;
            for (var i = 0; i < burstStatusRules.Count; i++)
            {
                var rule = burstStatusRules[i];
                if (!MatchesBurstProjectileIndex(rule.ProjectileIndex, projectileIndex, burstProjectileCount))
                {
                    continue;
                }

                bonus += rule.StacksBonus;
            }

            return bonus;
        }

        /*
         * 필수 중첩을 보유하고 있는지 확인한다.
         */
        private static bool HasRequiredStacks(BaseUnitRuntimeModel target, string statusId, int minimumStacks)
        {
            if (target == null || minimumStacks <= 0 || string.IsNullOrWhiteSpace(statusId))
            {
                return false;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }

            return target.Statuses != null && target.Statuses.GetStacks(kind) >= minimumStacks;
        }

        /*
         * 현재 투사체가 연속 발사 보정 대상 순번인지 확인한다.
         */
        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex == projectileIndex;
        }

        /*
         * 값이 양수이면 사용하고 아니면 기본값을 반환한다.
         */
        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        /*
         * 정규화된 단일 공격 노드를 기존 실행 규칙 목록과 맞춘다.
         */
        private void RefreshSingleAttackOperationBridges()
        {
            castConditionOps.Clear();
            damageModifierOps.Clear();
            critModifierOps.Clear();
            killActionOps.Clear();

            if (!Mathf.Approximately(ExecuteHealthRatioBonus, 0f))
            {
                castConditionOps.Add(new CastConditionOp(CastConditionOpKind.TargetHealthRatioBonus, ExecuteHealthRatioBonus));
            }

            if (!Mathf.Approximately(BossDamageMultiplier, 1f))
            {
                damageModifierOps.Add(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, BossDamageMultiplier));
            }

            if (!Mathf.Approximately(ExecuteCritChanceBonus, 0f))
            {
                critModifierOps.Add(new CritModifierOp(CritModifierOpKind.ExecuteChanceBonus, ExecuteCritChanceBonus));
            }

            if (KillResetsCooldown)
            {
                killActionOps.Add(new KillActionOp(KillActionOpKind.CooldownReset, 0f, KillResetsCooldownRequiresExecute));
            }

            if (!Mathf.Approximately(KillCooldownRefundRatioBonus, 0f))
            {
                killActionOps.Add(new KillActionOp(KillActionOpKind.CooldownRefundBonus, KillCooldownRefundRatioBonus, false));
            }
        }

        /*
         * 실행 계획을 다시 구성한다.
         */
        private void RebuildExecutionPlan()
        {
            Plan = SkillExecutionPlanCompiler.Compile(Source, this, normalizedPlanNodes);
        }

        /*
         * 정규화된 계획 노드를 추가한다.
         */
        private void AddNormalizedPlanNodes(IReadOnlyList<SkillExecutionPlanNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null)
                {
                    normalizedPlanNodes.Add(nodes[i]);
                }
            }
        }

        /*
         * 조건부 피해 규칙에 필요한 값을 보관한다.
         */
        private readonly struct ConditionalDamageRule
        {
            /*
             * 조건부 피해 규칙에 필요한 값을 초기화한다.
             */
            public ConditionalDamageRule(float damageMultiplier, string statusId, int minStacks)
            {
                DamageMultiplier = damageMultiplier;
                StatusId = statusId;
                MinStacks = minStacks;
            }

            public float DamageMultiplier { get; }
            public string StatusId { get; }
            public int MinStacks { get; }
        }

        /*
         * 조건부 치명타 확률 규칙에 필요한 값을 보관한다.
         */
        private readonly struct ConditionalCritChanceRule
        {
            /*
             * 조건부 치명타 확률 규칙에 필요한 값을 초기화한다.
             */
            public ConditionalCritChanceRule(float critChanceBonus, string statusId, int minStacks)
            {
                CritChanceBonus = critChanceBonus;
                StatusId = statusId;
                MinStacks = minStacks;
            }

            public float CritChanceBonus { get; }
            public string StatusId { get; }
            public int MinStacks { get; }
        }

        /*
         * 연속 발사 피해 규칙에 필요한 값을 보관한다.
         */
        private readonly struct BurstDamageRule
        {
            /*
             * 연속 발사 피해 규칙에 필요한 값을 초기화한다.
             */
            public BurstDamageRule(int projectileIndex, float damageMultiplier)
            {
                ProjectileIndex = projectileIndex;
                DamageMultiplier = damageMultiplier;
            }

            public int ProjectileIndex { get; }
            public float DamageMultiplier { get; }
        }

        /*
         * 연속 발사 상태 규칙에 필요한 값을 보관한다.
         */
        private readonly struct BurstStatusRule
        {
            /*
             * 연속 발사 상태 규칙에 필요한 값을 초기화한다.
             */
            public BurstStatusRule(int projectileIndex, int stacksBonus)
            {
                ProjectileIndex = projectileIndex;
                StacksBonus = stacksBonus;
            }

            public int ProjectileIndex { get; }
            public int StacksBonus { get; }
        }
    }
}
