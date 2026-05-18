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
        public string StatusTag { get; private set; }
        public float StatusChanceBonus { get; private set; }
        public int StatusStacksBonus { get; private set; }
        public bool HasStatusStacksSet { get; private set; }
        public int StatusStacksSet { get; private set; }
        public GameObject SkillEffectPrefab { get; private set; }

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

            CooldownMultiplier *= PositiveOrDefault(spec.CooldownMultiplier, 1f);
            RadiusMultiplier *= PositiveOrDefault(spec.RadiusMultiplier, 1f);
            DurationMultiplier *= PositiveOrDefault(spec.DurationMultiplier, 1f);

            if (spec.HasMagazineBonus)
            {
                MagazineBonus += spec.MagazineBonus;
            }

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

            if (!string.IsNullOrWhiteSpace(spec.AddedStatusTag))
            {
                StatusTag = spec.AddedStatusTag;
            }

            if (spec.SkillEffectPrefab != null)
            {
                SkillEffectPrefab = spec.SkillEffectPrefab;
            }
        }

        public void ApplyModifierRecord(SkillChoiceModifierRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (record.HasDamageMultiplier)
            {
                DamageMultiplier *= PositiveOrDefault(record.DamageMultiplier, 1f);
            }

            BaseDamageBonus += record.BaseDamageBonus;
            MagazineBonus += record.MagazineBonus;
            AdditionalProjectileBonus += record.AdditionalProjectileBonus;
            PierceBonus += record.PierceBonus;

            if (record.HasReloadTimeMultiplier)
            {
                ReloadTimeMultiplier *= PositiveOrDefault(record.ReloadTimeMultiplier, 1f);
            }

            if (record.HasShotIntervalMultiplier)
            {
                ShotIntervalMultiplier *= PositiveOrDefault(record.ShotIntervalMultiplier, 1f);
            }

            if (record.HasRadiusMultiplier)
            {
                RadiusMultiplier *= PositiveOrDefault(record.RadiusMultiplier, 1f);
            }

            RadiusBonus += record.RadiusBonus;

            if (record.HasDurationMultiplier)
            {
                DurationMultiplier *= PositiveOrDefault(record.DurationMultiplier, 1f);
            }

            DurationBonus += record.DurationBonus;
            BranchChanceBonus += record.BranchChanceBonus;

            if (record.HasBranchChanceSet)
            {
                HasBranchChanceSet = true;
                BranchChanceSet = record.BranchChanceSet;
            }

            if (record.HasBranchCount)
            {
                HasBranchCount = true;
                BranchCount = record.BranchCount;
            }

            if (record.HasBranchDamageMultiplier)
            {
                HasBranchDamageMultiplier = true;
                BranchDamageMultiplier = record.BranchDamageMultiplier;
            }

            if (record.HasBranchSearchRadius)
            {
                HasBranchSearchRadius = true;
                BranchSearchRadius = record.BranchSearchRadius;
            }

            if (!string.IsNullOrWhiteSpace(record.StatusTag))
            {
                StatusTag = record.StatusTag;
            }

            StatusChanceBonus += record.StatusChanceBonus;
            StatusStacksBonus += record.StatusStacksBonus;

            if (record.HasStatusStacksSet)
            {
                HasStatusStacksSet = true;
                StatusStacksSet = record.StatusStacksSet;
            }
        }

        private static float PositiveOrDefault(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }
    }
}
