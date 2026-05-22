using Pakuri.Data;
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
            ReloadTimeMultiplier = 1f;
            ShotIntervalMultiplier = 1f;
            BranchDamageMultiplier = 1f;
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
        public int HitTargetCountBonus { get; private set; }
        public float CritChanceBonus { get; private set; }
        public float CritDamageBonus { get; private set; }
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
        public string ThresholdStatusId { get; private set; }
        public int ThresholdStatusMinStacks { get; private set; }
        public string ThresholdApplyStatusId { get; private set; }
        public GameObject SkillEffectPrefab { get; private set; }
        private readonly HashSet<string> activeChoiceIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> statusDurationBonuses = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);

        public bool HasBranchBehavior =>
            BranchChanceBonus > 0f
            || HasBranchChanceSet
            || HasBranchCount
            || HasBranchDamageMultiplier
            || HasBranchSearchRadius;

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

            HitTargetCountBonus += spec.HitTargetCountBonus;
            CritChanceBonus += spec.CritChanceBonus;
            CritDamageBonus += spec.CritDamageBonus;

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

            if (!string.IsNullOrWhiteSpace(spec.ThresholdStatusId)
                && spec.ThresholdStatusMinStacks > 0
                && !string.IsNullOrWhiteSpace(spec.ThresholdApplyStatusId))
            {
                ThresholdStatusId = spec.ThresholdStatusId;
                ThresholdStatusMinStacks = spec.ThresholdStatusMinStacks;
                ThresholdApplyStatusId = spec.ThresholdApplyStatusId;
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
                HasMaxHealthBonus = choice.HasMaxHealthBonus,
                MaxHealthBonus = choice.MaxHealthBonus,
                HitTargetCountBonus = choice.HitTargetCountBonus,
                CritChanceBonus = choice.CritChanceBonus,
                CritDamageBonus = choice.CritDamageBonus,
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
                StatusDurationBonusStatusId = choice.StatusDurationBonusStatusId,
                StatusDurationBonus = choice.StatusDurationBonus,
                ThresholdStatusId = choice.ThresholdStatusId,
                ThresholdStatusMinStacks = choice.ThresholdStatusMinStacks,
                ThresholdApplyStatusId = choice.ThresholdApplyStatusId,
                CountStatusId = choice.CountStatusId,
                CountTargetSide = choice.CountTargetSide,
                DamageMultiplierPerCount = choice.DamageMultiplierPerCount,
                CountMax = choice.CountMax,
                HasStatusConditionalDamageTakenBonus = choice.HasStatusConditionalDamageTakenBonus,
                StatusConditionalDamageTakenBonus = choice.StatusConditionalDamageTakenBonus,
                StatusConditionalSourceStatusId = choice.StatusConditionalSourceStatusId
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

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }
    }
}
