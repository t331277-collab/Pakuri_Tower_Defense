using Pakuri.Data;
using Pakuri.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class SkillExecutionSnapshot
    {
        public SkillExecutionSnapshot(SkillData source)
        {
            Source = source;
            SkillId = source != null ? source.SkillId : string.Empty;
            DamageMultiplier = 1f;
            CooldownMultiplier = 1f;
            RadiusMultiplier = 1f;
            DurationMultiplier = 1f;
            KnockbackDistanceMultiplier = 1f;
            ReloadTimeMultiplier = 1f;
            ShotIntervalMultiplier = 1f;
            BossDamageMultiplier = 1f;
            BranchDamageMultiplier = 1f;
            OnHitAdditionalDamageMultiplier = 1f;
            OnHitChainDamageMultiplier = 1f;
            SkillEffectPrefab = source != null ? source.SkillEffectPrefab : null;
        }

        public SkillData Source { get; }
        public string SkillId { get; }
        public float DamageMultiplier { get; private set; }
        public float CooldownMultiplier { get; private set; }
        public float RadiusMultiplier { get; private set; }
        public float DurationMultiplier { get; private set; }
        public float BaseDamageBonus { get; private set; }
        public int MagazineBonus { get; private set; }
        public int AdditionalProjectileBonus { get; private set; }
        public int PierceBonus { get; private set; }
        public float ReloadTimeMultiplier { get; private set; }
        public float ShotIntervalMultiplier { get; private set; }
        public float RadiusBonus { get; private set; }
        public float BeamWidthBonus { get; private set; }
        public float KnockbackDistanceMultiplier { get; private set; }
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
        public float BossDamageMultiplier { get; private set; }
        public float KillCooldownRefundRatioBonus { get; private set; }
        public bool KillResetsCooldown { get; private set; }
        public bool KillResetsCooldownRequiresExecute { get; private set; }
        public string StatusTag { get; private set; }
        public float StatusChanceBonus { get; private set; }
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
        public string ThresholdStatusId { get; private set; }
        public int ThresholdStatusMinStacks { get; private set; }
        public string ThresholdApplyStatusId { get; private set; }
        public GameObject SkillEffectPrefab { get; private set; }
        private readonly HashSet<string> activeChoiceIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> statusMaxStacksBonuses = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<ConditionalDamageRule> conditionalDamageRules = new List<ConditionalDamageRule>();

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

            if (spec.HasStatusChanceBonus)
            {
                StatusChanceBonus += spec.StatusChanceBonus;
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

            if (!string.IsNullOrWhiteSpace(spec.ThresholdStatusId)
                && spec.ThresholdStatusMinStacks > 0
                && !string.IsNullOrWhiteSpace(spec.ThresholdApplyStatusId))
            {
                ThresholdStatusId = spec.ThresholdStatusId;
                ThresholdStatusMinStacks = spec.ThresholdStatusMinStacks;
                ThresholdApplyStatusId = spec.ThresholdApplyStatusId;
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

            if (spec.SkillEffectPrefab != null)
            {
                SkillEffectPrefab = spec.SkillEffectPrefab;
            }
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

            ApplyChoiceSpec(new SkillChoiceEffectSpec
            {
                ChoiceId = choice.ChoiceId,
                Title = choice.Title,
                Description = choice.DescriptionText,
                Icon = choice.SkillIcon,
                SkillEffectPrefab = choice.SkillEffectPrefab,
                HasDamageMultiplier = choice.HasDamageMultiplier,
                DamageMultiplier = choice.DamageMultiplier,
                BaseDamageBonus = choice.BaseDamageBonus,
                HasCooldownMultiplier = choice.HasCooldownMultiplier,
                CooldownMultiplier = choice.CooldownMultiplier,
                HasRadiusMultiplier = choice.HasRadiusMultiplier,
                RadiusMultiplier = choice.RadiusMultiplier,
                RadiusBonus = choice.RadiusBonus,
                BeamWidthBonus = choice.BeamWidthBonus,
                HasKnockbackDistanceMultiplier = choice.HasKnockbackDistanceMultiplier,
                KnockbackDistanceMultiplier = choice.KnockbackDistanceMultiplier,
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
                HasConditionalDamageMultiplier = choice.HasConditionalDamageMultiplier,
                ConditionalDamageMultiplier = choice.ConditionalDamageMultiplier,
                ConditionalTargetStatusId = choice.ConditionalTargetStatusId,
                ConditionalTargetStatusMinStacks = choice.ConditionalTargetStatusMinStacks,
                CountStatusId = choice.CountStatusId,
                CountTargetSide = choice.CountTargetSide,
                DamageMultiplierPerCount = choice.DamageMultiplierPerCount,
                CountMax = choice.CountMax,
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
                ReloadReduceSecondsPerHit = choice.ReloadReduceSecondsPerHit
            });
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

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
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
    }
}
