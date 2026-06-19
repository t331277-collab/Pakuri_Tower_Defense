using Pakuri.Data;
using Pakuri.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    public enum CastConditionOpKind
    {
        TargetHealthRatioBonus
    }

    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    public enum CritModifierOpKind
    {
        ExecuteChanceBonus
    }

    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

    public readonly struct CastConditionOp
    {
        public CastConditionOp(CastConditionOpKind kind, float value)
        {
            Kind = kind;
            Value = value;
        }

        public CastConditionOpKind Kind { get; }
        public float Value { get; }
    }

    public readonly struct DamageModifierOp
    {
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    public readonly struct CritModifierOp
    {
        public CritModifierOp(CritModifierOpKind kind, float chanceBonus)
        {
            Kind = kind;
            ChanceBonus = chanceBonus;
        }

        public CritModifierOpKind Kind { get; }
        public float ChanceBonus { get; }
    }

    public readonly struct KillActionOp
    {
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

    public sealed class SkillExecutionSnapshot
    {
        public SkillExecutionSnapshot(SkillData source)
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

        public SkillData Source { get; }
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

        public void ApplyChoiceSpec(SkillChoiceEffectSpec spec)
        {
            if (spec == null)
            {
                return;
            }

            if (spec.HasDamageMultiplier)
            {
                DamageMultiplier *= PositiveOrDefault(spec.DamageMultiplier, 1f);
            }

            if (spec.HasShieldAmountMultiplier)
            {
                ShieldAmountMultiplier *= PositiveOrDefault(spec.ShieldAmountMultiplier, 1f);
            }

            BaseDamageBonus += spec.BaseDamageBonus;

            if (spec.HasCooldownMultiplier)
            {
                CooldownMultiplier *= PositiveOrDefault(spec.CooldownMultiplier, 1f);
            }

            if (spec.HasRadiusMultiplier)
            {
                RadiusMultiplier *= PositiveOrDefault(spec.RadiusMultiplier, 1f);
            }

            RadiusBonus += spec.RadiusBonus;
            BeamWidthBonus += spec.BeamWidthBonus;
            if (spec.HasKnockbackDistanceMultiplier)
            {
                KnockbackDistanceMultiplier *= PositiveOrDefault(spec.KnockbackDistanceMultiplier, 1f);
            }

            if (spec.HasDamageDelayMultiplier)
            {
                DamageDelayMultiplier *= PositiveOrDefault(spec.DamageDelayMultiplier, 1f);
            }

            if (spec.HasExecuteHealthRatioBonus)
            {
                ExecuteHealthRatioBonus += spec.ExecuteHealthRatioBonus;
            }

            if (spec.HasDurationMultiplier)
            {
                DurationMultiplier *= PositiveOrDefault(spec.DurationMultiplier, 1f);
            }

            DurationBonus += spec.DurationBonus;

            if (spec.HasMagazineBonus)
            {
                MagazineBonus += spec.MagazineBonus;
            }

            AdditionalProjectileBonus += spec.AdditionalProjectileBonus;
            PierceBonus += spec.PierceBonus;

            if (spec.HasReloadTimeMultiplier)
            {
                ReloadTimeMultiplier *= PositiveOrDefault(spec.ReloadTimeMultiplier, 1f);
            }

            if (spec.HasShotIntervalMultiplier)
            {
                ShotIntervalMultiplier *= PositiveOrDefault(spec.ShotIntervalMultiplier, 1f);
            }

            if (spec.HasBurstDamageMultiplier
                && spec.BurstDamageMultiplier > 0f
                && spec.HasBurstDamageProjectileIndex)
            {
                burstDamageRules.Add(new BurstDamageRule(
                    spec.BurstDamageProjectileIndex,
                    spec.BurstDamageMultiplier));
            }

            if (spec.HasBurstStatusProjectileIndex && spec.BurstStatusStacksBonus != 0)
            {
                burstStatusRules.Add(new BurstStatusRule(
                    spec.BurstStatusProjectileIndex,
                    spec.BurstStatusStacksBonus));
            }

            if (spec.FollowUpProjectileCount > 0)
            {
                FollowUpProjectileCount = spec.FollowUpProjectileCount;
                FollowUpProjectileDelaySeconds = Mathf.Max(0f, spec.FollowUpProjectileDelaySeconds);
                FollowUpProjectileDamageMultiplier = Mathf.Max(0f, spec.FollowUpProjectileDamageMultiplier);
            }

            if (spec.HasStatusChanceBonus)
            {
                StatusChanceBonus += spec.StatusChanceBonus;
            }

            if (spec.HasStatusActionSpeedBonus)
            {
                HasStatusActionSpeedBonus = true;
                if (string.IsNullOrWhiteSpace(spec.StatusActionSpeedBonusStatusId))
                {
                    StatusActionSpeedBonus += spec.StatusActionSpeedBonus;
                }
                else
                {
                    StatusActionSpeedBonusStatusId = spec.StatusActionSpeedBonusStatusId;
                    if (statusActionSpeedBonuses.TryGetValue(spec.StatusActionSpeedBonusStatusId, out var currentBonus))
                    {
                        statusActionSpeedBonuses[spec.StatusActionSpeedBonusStatusId] = currentBonus + spec.StatusActionSpeedBonus;
                    }
                    else
                    {
                        statusActionSpeedBonuses[spec.StatusActionSpeedBonusStatusId] = spec.StatusActionSpeedBonus;
                    }
                }
            }

            if (spec.HasStatusAttackPowerBonus)
            {
                HasStatusAttackPowerBonus = true;
                StatusAttackPowerBonus += spec.StatusAttackPowerBonus;
            }

            BranchChanceBonus += spec.BranchChanceBonus;

            if (spec.HasBranchChanceSet)
            {
                HasBranchChanceSet = true;
                BranchChanceSet = spec.BranchChanceSet;
            }

            if (spec.HasBranchCount)
            {
                HasBranchCount = true;
                BranchCount = spec.BranchCount;
            }

            if (spec.HasBranchDamageMultiplier)
            {
                HasBranchDamageMultiplier = true;
                BranchDamageMultiplier = spec.BranchDamageMultiplier;
            }

            if (spec.HasBranchSearchRadius)
            {
                HasBranchSearchRadius = true;
                BranchSearchRadius = spec.BranchSearchRadius;
            }

            if (spec.BranchLaunchPeriod > 0)
            {
                BranchLaunchPeriod = spec.BranchLaunchPeriod;
            }

            if (spec.HasBranchLaunchChanceSet)
            {
                HasBranchLaunchChanceSet = true;
                BranchLaunchChanceSet = spec.BranchLaunchChanceSet;
            }

            HitTargetCountBonus += spec.HitTargetCountBonus;
            CritChanceBonus += spec.CritChanceBonus;
            CritDamageBonus += spec.CritDamageBonus;
            ExecuteCritChanceBonus += spec.ExecuteCritChanceBonus;
            if (spec.HasBossDamageMultiplier)
            {
                BossDamageMultiplier *= PositiveOrDefault(spec.BossDamageMultiplier, 1f);
            }

            if (spec.HasKillCooldownRefundRatioBonus)
            {
                KillCooldownRefundRatioBonus += spec.KillCooldownRefundRatioBonus;
            }

            if (spec.KillResetsCooldown)
            {
                KillResetsCooldown = true;
            }

            if (spec.KillResetsCooldownRequiresExecute)
            {
                KillResetsCooldownRequiresExecute = true;
            }

            if (!string.IsNullOrWhiteSpace(spec.StatusTag))
            {
                StatusTag = spec.StatusTag;
            }

            StatusStacksBonus += spec.StatusStacksBonus;
            if (spec.HasStatusStacksSet)
            {
                HasStatusStacksSet = true;
                StatusStacksSet = spec.StatusStacksSet;
            }

            if (spec.HasStatusElementDamageTakenBonus)
            {
                HasStatusElementDamageTakenBonus = true;
                StatusElementDamageTakenBonus = spec.StatusElementDamageTakenBonus;
            }

            if (spec.HasStatusCriticalDamageTakenBonus)
            {
                HasStatusCriticalDamageTakenBonus = true;
                StatusCriticalDamageTakenBonus = spec.StatusCriticalDamageTakenBonus;
            }

            if (spec.HasStatusAilmentResistanceBonus)
            {
                HasStatusAilmentResistanceBonus = true;
                StatusAilmentResistanceBonus = spec.StatusAilmentResistanceBonus;
            }

            if (!string.IsNullOrWhiteSpace(spec.StatusMaxStacksBonusStatusId)
                && spec.StatusMaxStacksBonus != 0)
            {
                if (statusMaxStacksBonuses.TryGetValue(spec.StatusMaxStacksBonusStatusId, out var currentBonus))
                {
                    statusMaxStacksBonuses[spec.StatusMaxStacksBonusStatusId] = currentBonus + spec.StatusMaxStacksBonus;
                }
                else
                {
                    statusMaxStacksBonuses[spec.StatusMaxStacksBonusStatusId] = spec.StatusMaxStacksBonus;
                }
            }

            if (!string.IsNullOrWhiteSpace(spec.StatusDurationBonusStatusId)
                && !Mathf.Approximately(spec.StatusDurationBonus, 0f))
            {
                if (statusDurationBonuses.TryGetValue(spec.StatusDurationBonusStatusId, out var currentBonus))
                {
                    statusDurationBonuses[spec.StatusDurationBonusStatusId] = currentBonus + spec.StatusDurationBonus;
                }
                else
                {
                    statusDurationBonuses[spec.StatusDurationBonusStatusId] = spec.StatusDurationBonus;
                }
            }

            if (spec.HasStatusConditionalDamageTakenBonus)
            {
                HasStatusConditionalDamageTakenBonus = true;
                StatusConditionalDamageTakenBonus = spec.StatusConditionalDamageTakenBonus;
                StatusConditionalSourceStatusId = spec.StatusConditionalSourceStatusId;
            }

            if (spec.HasOnHitAdditionalDamage)
            {
                HasOnHitAdditionalDamage = true;
                OnHitAdditionalDamageChance = Mathf.Clamp01(spec.OnHitAdditionalDamageChance);
                OnHitAdditionalDamageMultiplier = Mathf.Max(0f, spec.OnHitAdditionalDamageMultiplier);
                OnHitAdditionalDamageAttribute = spec.OnHitAdditionalDamageAttribute;
                OnHitAdditionalDamageTarget = spec.OnHitAdditionalDamageTarget;
            }

            if (spec.OnHitChainHitPeriod > 0)
            {
                OnHitChainHitPeriod = spec.OnHitChainHitPeriod;
            }

            if (spec.OnHitChainTargetCount > 0)
            {
                OnHitChainTargetCount = spec.OnHitChainTargetCount;
            }

            if (spec.OnHitChainSearchRadius > 0f)
            {
                OnHitChainSearchRadius = spec.OnHitChainSearchRadius;
            }

            if (spec.OnHitChainDamageMultiplier > 0f)
            {
            OnHitChainDamageMultiplier = spec.OnHitChainDamageMultiplier;
            }

            OnHitChainDamageAttribute = spec.OnHitChainDamageAttribute;

            if (!string.IsNullOrWhiteSpace(spec.ReloadReduceTargetSkillId)
                && spec.ReloadReduceSecondsPerHit > 0f)
            {
                ReloadReduceTargetSkillId = spec.ReloadReduceTargetSkillId;
                ReloadReduceSecondsPerHit += spec.ReloadReduceSecondsPerHit;
            }

            if (!string.IsNullOrWhiteSpace(spec.CoreHitboxName))
            {
                CoreHitboxName = spec.CoreHitboxName.Trim();
            }

            if (spec.HasCoreDamageMultiplier)
            {
                HasCoreDamageMultiplier = true;
                CoreDamageMultiplier = PositiveOrDefault(spec.CoreDamageMultiplier, 1f);
            }

            if (spec.HasCoreOnHitAdditionalDamage)
            {
                HasCoreOnHitAdditionalDamage = true;
                CoreOnHitAdditionalDamageChance = Mathf.Clamp01(spec.CoreOnHitAdditionalDamageChance);
                CoreOnHitAdditionalDamageMultiplier = Mathf.Max(0f, spec.CoreOnHitAdditionalDamageMultiplier);
                CoreOnHitAdditionalDamageAttribute = spec.CoreOnHitAdditionalDamageAttribute;
            }

            if (!string.IsNullOrWhiteSpace(spec.HitCountCooldownRefundTargetSkillId)
                && spec.HitCountCooldownRefundMinTargets > 0
                && spec.HitCountCooldownRefundRatio > 0f)
            {
                HitCountCooldownRefundTargetSkillId = spec.HitCountCooldownRefundTargetSkillId;
                HitCountCooldownRefundMinTargets = spec.HitCountCooldownRefundMinTargets;
                HitCountCooldownRefundRatio = Mathf.Clamp01(spec.HitCountCooldownRefundRatio);
            }

            if (spec.RepeatCountPerTarget > 0)
            {
                RepeatCountPerTarget += spec.RepeatCountPerTarget;
                RepeatIntervalSeconds = Mathf.Max(RepeatIntervalSeconds, spec.RepeatIntervalSeconds);
                if (spec.RepeatDamageMultiplier > 0f)
                {
                    RepeatDamageMultiplier *= PositiveOrDefault(spec.RepeatDamageMultiplier, 1f);
                }
            }

            if (!string.IsNullOrWhiteSpace(spec.ThresholdStatusId)
                && spec.ThresholdStatusMinStacks > 0
                && !string.IsNullOrWhiteSpace(spec.ThresholdApplyStatusId))
            {
                ThresholdStatusId = spec.ThresholdStatusId;
                ThresholdStatusMinStacks = spec.ThresholdStatusMinStacks;
                ThresholdApplyStatusId = spec.ThresholdApplyStatusId;
            }

            if (spec.HasTargetStatusStackDamageMultiplier && spec.TargetStatusStackDamageMultiplier > 0f)
            {
                TargetStatusStackDamageMultiplier *= PositiveOrDefault(spec.TargetStatusStackDamageMultiplier, 1f);
            }

            if (spec.HasConsumeTargetStatusRatioOverride)
            {
                HasConsumeTargetStatusRatioOverride = true;
                ConsumeTargetStatusRatioOverride = Mathf.Clamp01(spec.ConsumeTargetStatusRatioOverride);
            }

            if (spec.HasConsumeTargetStatusStacksOverride)
            {
                HasConsumeTargetStatusStacksOverride = true;
                ConsumeTargetStatusStacksOverride = Mathf.Max(0, spec.ConsumeTargetStatusStacksOverride);
            }

            if (spec.HasConditionalDamageMultiplier
                && spec.ConditionalDamageMultiplier > 0f
                && !string.IsNullOrWhiteSpace(spec.ConditionalTargetStatusId)
                && spec.ConditionalTargetStatusMinStacks > 0)
            {
                conditionalDamageRules.Add(new ConditionalDamageRule(
                    spec.ConditionalDamageMultiplier,
                    spec.ConditionalTargetStatusId,
                    spec.ConditionalTargetStatusMinStacks));
            }

            if (!Mathf.Approximately(spec.ConditionalCritChanceBonus, 0f)
                && !string.IsNullOrWhiteSpace(spec.ConditionalCritTargetStatusId)
                && spec.ConditionalCritTargetStatusMinStacks > 0)
            {
                conditionalCritChanceRules.Add(new ConditionalCritChanceRule(
                    spec.ConditionalCritChanceBonus,
                    spec.ConditionalCritTargetStatusId,
                    spec.ConditionalCritTargetStatusMinStacks));
            }

            if (spec.RedistributeConsumedStatusRatioOnKill > 0f
                && !string.IsNullOrWhiteSpace(spec.RedistributeConsumedStatusId)
                && spec.RedistributeConsumedStatusSearchRadius > 0f)
            {
                RedistributeConsumedStatusRatioOnKill = Mathf.Clamp01(spec.RedistributeConsumedStatusRatioOnKill);
                RedistributeConsumedStatusId = spec.RedistributeConsumedStatusId;
                RedistributeConsumedStatusSearchRadius = Mathf.Max(0f, spec.RedistributeConsumedStatusSearchRadius);
                RedistributeConsumedStatusTargetCount = Mathf.Max(0, spec.RedistributeConsumedStatusTargetCount);
            }

            if (spec.ConsecutiveHitBonusRate > 0f)
            {
                ConsecutiveHitBonusRate = Mathf.Max(0f, spec.ConsecutiveHitBonusRate);
            }

            if (spec.ConsecutiveHitMax > 0f)
            {
                ConsecutiveHitMax = Mathf.Max(0f, spec.ConsecutiveHitMax);
            }

            if (spec.SkillEffectPrefab != null)
            {
                SkillEffectPrefab = spec.SkillEffectPrefab;
            }

            AddNormalizedPlanNodes(spec.NormalizedPlanNodes);
            RefreshSingleAttackOperationBridges();
            RebuildExecutionPlan();
        }

        public void ApplyDynamicDamageMultiplier(float multiplier)
        {
            DamageMultiplier *= PositiveOrDefault(multiplier, 1f);
        }

        public void ApplyChoiceDefinition(SkillChoiceDefinition choice)
        {
            if (choice == null)
            {
                return;
            }

            var spec = new SkillChoiceEffectSpec
            {
                ChoiceId = choice.ChoiceId,
                Title = choice.Title,
                Description = choice.DescriptionText,
                Icon = choice.SkillIcon,
                SkillEffectPrefab = choice.SkillEffectPrefab,
                HasDamageMultiplier = choice.HasDamageMultiplier,
                DamageMultiplier = choice.DamageMultiplier,
                HasShieldAmountMultiplier = false,
                ShieldAmountMultiplier = 1f,
                BaseDamageBonus = choice.BaseDamageBonus,
                HasCooldownMultiplier = choice.HasCooldownMultiplier,
                CooldownMultiplier = choice.CooldownMultiplier,
                HasRadiusMultiplier = choice.HasRadiusMultiplier,
                RadiusMultiplier = choice.RadiusMultiplier,
                RadiusBonus = choice.RadiusBonus,
                BeamWidthBonus = choice.BeamWidthBonus,
                HasKnockbackDistanceMultiplier = choice.HasKnockbackDistanceMultiplier,
                KnockbackDistanceMultiplier = choice.KnockbackDistanceMultiplier,
                HasDamageDelayMultiplier = choice.HasDamageDelayMultiplier,
                DamageDelayMultiplier = choice.DamageDelayMultiplier,
                HasExecuteHealthRatioBonus = choice.HasExecuteHealthRatioBonus,
                ExecuteHealthRatioBonus = choice.ExecuteHealthRatioBonus,
                HasDurationMultiplier = choice.HasDurationMultiplier,
                DurationMultiplier = choice.DurationMultiplier,
                DurationBonus = choice.DurationBonus,
                HasMagazineBonus = choice.HasMagazineBonus,
                MagazineBonus = choice.MagazineBonus,
                AdditionalProjectileBonus = choice.AdditionalProjectileBonus,
                PierceBonus = choice.PierceBonus,
                HasReloadTimeMultiplier = choice.HasReloadTimeMultiplier,
                ReloadTimeMultiplier = choice.ReloadTimeMultiplier,
                HasShotIntervalMultiplier = choice.HasShotIntervalMultiplier,
                ShotIntervalMultiplier = choice.ShotIntervalMultiplier,
                HasBurstDamageProjectileIndex = choice.HasBurstDamageProjectileIndex,
                BurstDamageProjectileIndex = choice.BurstDamageProjectileIndex,
                HasBurstDamageMultiplier = choice.HasBurstDamageMultiplier,
                BurstDamageMultiplier = choice.BurstDamageMultiplier,
                HasBurstStatusProjectileIndex = choice.HasBurstStatusProjectileIndex,
                BurstStatusProjectileIndex = choice.BurstStatusProjectileIndex,
                BurstStatusStacksBonus = choice.BurstStatusStacksBonus,
                FollowUpProjectileCount = choice.FollowUpProjectileCount,
                FollowUpProjectileDelaySeconds = choice.FollowUpProjectileDelaySeconds,
                FollowUpProjectileDamageMultiplier = choice.FollowUpProjectileDamageMultiplier,
                HasStatusChanceBonus = choice.HasStatusChanceBonus,
                StatusChanceBonus = choice.StatusChanceBonus,
                BranchChanceBonus = choice.BranchChanceBonus,
                HasBranchChanceSet = choice.HasBranchChanceSet,
                BranchChanceSet = choice.BranchChanceSet,
                HasBranchCount = choice.HasBranchCount,
                BranchCount = choice.BranchCount,
                HasBranchDamageMultiplier = choice.HasBranchDamageMultiplier,
                BranchDamageMultiplier = choice.BranchDamageMultiplier,
                HasBranchSearchRadius = choice.HasBranchSearchRadius,
                BranchSearchRadius = choice.BranchSearchRadius,
                BranchLaunchPeriod = choice.BranchLaunchPeriod,
                HasBranchLaunchChanceSet = choice.HasBranchLaunchChanceSet,
                BranchLaunchChanceSet = choice.BranchLaunchChanceSet,
                HasMaxHealthBonus = choice.HasMaxHealthBonus,
                MaxHealthBonus = choice.MaxHealthBonus,
                HitTargetCountBonus = choice.HitTargetCountBonus,
                CritChanceBonus = choice.CritChanceBonus,
                CritDamageBonus = choice.CritDamageBonus,
                ExecuteCritChanceBonus = choice.ExecuteCritChanceBonus,
                HasBossDamageMultiplier = choice.HasBossDamageMultiplier,
                BossDamageMultiplier = choice.BossDamageMultiplier,
                HasKillCooldownRefundRatioBonus = choice.HasKillCooldownRefundRatioBonus,
                KillCooldownRefundRatioBonus = choice.KillCooldownRefundRatioBonus,
                KillResetsCooldown = choice.KillResetsCooldown,
                KillResetsCooldownRequiresExecute = choice.KillResetsCooldownRequiresExecute,
                StatusTag = choice.StatusTag,
                HasStatusActionSpeedBonus = choice.HasStatusActionSpeedBonus,
                StatusActionSpeedBonusStatusId = string.Empty,
                StatusActionSpeedBonus = choice.StatusActionSpeedBonus,
                HasStatusAttackPowerBonus = choice.HasStatusAttackPowerBonus,
                StatusAttackPowerBonus = choice.StatusAttackPowerBonus,
                StatusStacksBonus = choice.StatusStacksBonus,
                HasStatusStacksSet = choice.HasStatusStacksSet,
                StatusStacksSet = choice.StatusStacksSet,
                HasStatusElementDamageTakenBonus = choice.HasStatusElementDamageTakenBonus,
                StatusElementDamageTakenBonus = choice.StatusElementDamageTakenBonus,
                HasStatusCriticalDamageTakenBonus = choice.HasStatusCriticalDamageTakenBonus,
                StatusCriticalDamageTakenBonus = choice.StatusCriticalDamageTakenBonus,
                HasStatusAilmentResistanceBonus = choice.HasStatusAilmentResistanceBonus,
                StatusAilmentResistanceBonus = choice.StatusAilmentResistanceBonus,
                StatusMaxStacksBonusStatusId = choice.StatusMaxStacksBonusStatusId,
                StatusMaxStacksBonus = choice.StatusMaxStacksBonus,
                StatusDurationBonusStatusId = choice.StatusDurationBonusStatusId,
                StatusDurationBonus = choice.StatusDurationBonus,
                ThresholdStatusId = choice.ThresholdStatusId,
                ThresholdStatusMinStacks = choice.ThresholdStatusMinStacks,
                ThresholdApplyStatusId = choice.ThresholdApplyStatusId,
                HasTargetStatusStackDamageMultiplier = choice.HasTargetStatusStackDamageMultiplier,
                TargetStatusStackDamageMultiplier = choice.TargetStatusStackDamageMultiplier,
                HasConsumeTargetStatusRatioOverride = choice.HasConsumeTargetStatusRatioOverride,
                ConsumeTargetStatusRatioOverride = choice.ConsumeTargetStatusRatioOverride,
                HasConsumeTargetStatusStacksOverride = choice.HasConsumeTargetStatusStacksOverride,
                ConsumeTargetStatusStacksOverride = choice.ConsumeTargetStatusStacksOverride,
                HasConditionalDamageMultiplier = choice.HasConditionalDamageMultiplier,
                ConditionalDamageMultiplier = choice.ConditionalDamageMultiplier,
                ConditionalTargetStatusId = choice.ConditionalTargetStatusId,
                ConditionalTargetStatusMinStacks = choice.ConditionalTargetStatusMinStacks,
                ConditionalCritChanceBonus = choice.ConditionalCritChanceBonus,
                ConditionalCritTargetStatusId = choice.ConditionalCritTargetStatusId,
                ConditionalCritTargetStatusMinStacks = choice.ConditionalCritTargetStatusMinStacks,
                RedistributeConsumedStatusRatioOnKill = choice.RedistributeConsumedStatusRatioOnKill,
                RedistributeConsumedStatusId = choice.RedistributeConsumedStatusId,
                RedistributeConsumedStatusSearchRadius = choice.RedistributeConsumedStatusSearchRadius,
                RedistributeConsumedStatusTargetCount = choice.RedistributeConsumedStatusTargetCount,
                CountStatusId = choice.CountStatusId,
                CountTargetSide = choice.CountTargetSide,
                DamageMultiplierPerCount = choice.DamageMultiplierPerCount,
                CountMax = choice.CountMax,
                ConsecutiveHitBonusRate = choice.ConsecutiveHitBonusRate,
                ConsecutiveHitMax = choice.ConsecutiveHitMax,
                HasStatusConditionalDamageTakenBonus = choice.HasStatusConditionalDamageTakenBonus,
                StatusConditionalDamageTakenBonus = choice.StatusConditionalDamageTakenBonus,
                StatusConditionalSourceStatusId = choice.StatusConditionalSourceStatusId,
                HasOnHitAdditionalDamage = choice.HasOnHitAdditionalDamage,
                OnHitAdditionalDamageChance = choice.OnHitAdditionalDamageChance,
                OnHitAdditionalDamageMultiplier = choice.OnHitAdditionalDamageMultiplier,
                OnHitAdditionalDamageAttribute = choice.OnHitAdditionalDamageAttribute,
                OnHitAdditionalDamageTarget = choice.OnHitAdditionalDamageTarget,
                OnHitChainHitPeriod = choice.OnHitChainHitPeriod,
                OnHitChainTargetCount = choice.OnHitChainTargetCount,
                OnHitChainSearchRadius = choice.OnHitChainSearchRadius,
                OnHitChainDamageMultiplier = choice.OnHitChainDamageMultiplier,
                OnHitChainDamageAttribute = choice.OnHitChainDamageAttribute,
                ReloadReduceTargetSkillId = choice.ReloadReduceTargetSkillId,
                ReloadReduceSecondsPerHit = choice.ReloadReduceSecondsPerHit,
                CoreHitboxName = choice.CoreHitboxName,
                HasCoreDamageMultiplier = choice.HasCoreDamageMultiplier,
                CoreDamageMultiplier = choice.CoreDamageMultiplier,
                HasCoreOnHitAdditionalDamage = choice.HasCoreOnHitAdditionalDamage,
                CoreOnHitAdditionalDamageChance = choice.CoreOnHitAdditionalDamageChance,
                CoreOnHitAdditionalDamageMultiplier = choice.CoreOnHitAdditionalDamageMultiplier,
                CoreOnHitAdditionalDamageAttribute = choice.CoreOnHitAdditionalDamageAttribute,
                HitCountCooldownRefundTargetSkillId = choice.HitCountCooldownRefundTargetSkillId,
                HitCountCooldownRefundMinTargets = choice.HitCountCooldownRefundMinTargets,
                HitCountCooldownRefundRatio = choice.HitCountCooldownRefundRatio,
                RepeatCountPerTarget = choice.RepeatCountPerTarget,
                RepeatIntervalSeconds = choice.RepeatIntervalSeconds,
                RepeatDamageMultiplier = choice.RepeatDamageMultiplier,
                NormalizedPlanNodes = InGameSkillDefinitionMapper.MapSkillNodeDefinitions(choice.NormalizedPlanNodes)
            };

            InGameSkillDefinitionMapper.ApplyNormalizedChoiceNodes(spec, choice.NormalizedPlanNodes);
            ApplyChoiceSpec(spec);
        }

        public void AddActiveChoiceId(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                activeChoiceIds.Add(choiceId);
            }
        }

        public bool HasActiveChoice(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && activeChoiceIds.Contains(choiceId);
        }

        public float ResolveStatusDurationBonus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0f;
            }

            return statusDurationBonuses.TryGetValue(statusId, out var bonus) ? bonus : 0f;
        }

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

        public int ResolveStatusMaxStacksBonus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            return statusMaxStacksBonuses.TryGetValue(statusId, out var bonus) ? bonus : 0;
        }

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

        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex == projectileIndex;
        }

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

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

        private void RebuildExecutionPlan()
        {
            Plan = SkillExecutionPlanCompiler.Compile(Source, this, normalizedPlanNodes);
        }

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

        private readonly struct ConditionalDamageRule
        {
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

        private readonly struct ConditionalCritChanceRule
        {
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

        private readonly struct BurstDamageRule
        {
            public BurstDamageRule(int projectileIndex, float damageMultiplier)
            {
                ProjectileIndex = projectileIndex;
                DamageMultiplier = damageMultiplier;
            }

            public int ProjectileIndex { get; }
            public float DamageMultiplier { get; }
        }

        private readonly struct BurstStatusRule
        {
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
