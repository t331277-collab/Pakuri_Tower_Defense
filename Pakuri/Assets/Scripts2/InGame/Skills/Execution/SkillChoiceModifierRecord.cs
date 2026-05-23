using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pakuri.InGame
{
    public sealed class SkillChoiceModifierRecord
    {
        public string ChoiceId { get; set; }
        public bool HasDamageMultiplier { get; set; }
        public float DamageMultiplier { get; set; }
        public float BaseDamageBonus { get; set; }
        public int MagazineBonus { get; set; }
        public int AdditionalProjectileBonus { get; set; }
        public int PierceBonus { get; set; }
        public bool HasReloadTimeMultiplier { get; set; }
        public float ReloadTimeMultiplier { get; set; }
        public bool HasShotIntervalMultiplier { get; set; }
        public float ShotIntervalMultiplier { get; set; }
        public bool HasRadiusMultiplier { get; set; }
        public float RadiusMultiplier { get; set; }
        public float RadiusBonus { get; set; }
        public bool HasDurationMultiplier { get; set; }
        public float DurationMultiplier { get; set; }
        public float DurationBonus { get; set; }
        public float BranchChanceBonus { get; set; }
        public bool HasBranchChanceSet { get; set; }
        public float BranchChanceSet { get; set; }
        public bool HasBranchCount { get; set; }
        public int BranchCount { get; set; }
        public bool HasBranchDamageMultiplier { get; set; }
        public float BranchDamageMultiplier { get; set; }
        public bool HasBranchSearchRadius { get; set; }
        public float BranchSearchRadius { get; set; }
        public string StatusTag { get; set; }
        public float StatusChanceBonus { get; set; }
        public int StatusStacksBonus { get; set; }
        public bool HasStatusStacksSet { get; set; }
        public int StatusStacksSet { get; set; }
        public string StatusMaxStacksBonusStatusId { get; set; }
        public int StatusMaxStacksBonus { get; set; }
        public string StatusDurationBonusStatusId { get; set; }
        public float StatusDurationBonus { get; set; }
        public string ThresholdStatusId { get; set; }
        public int ThresholdStatusMinStacks { get; set; }
        public string ThresholdApplyStatusId { get; set; }
        public bool HasConditionalDamageMultiplier { get; set; }
        public float ConditionalDamageMultiplier { get; set; }
        public string ConditionalTargetStatusId { get; set; }
        public int ConditionalTargetStatusMinStacks { get; set; }

        public static SkillChoiceModifierRecord FromRow(IDictionary<string, string> row)
        {
            if (row == null)
            {
                return null;
            }

            var record = new SkillChoiceModifierRecord
            {
                ChoiceId = Get(row, "choice_id"),
                StatusTag = Get(row, "status_tag")
            };

            record.HasDamageMultiplier = TryGetFloat(row, "damage_multiplier", out var damageMultiplier);
            record.DamageMultiplier = damageMultiplier;
            record.BaseDamageBonus = GetFloat(row, "base_damage_bonus");
            record.MagazineBonus = GetInt(row, "magazine_bonus");
            record.AdditionalProjectileBonus = GetInt(row, "additional_projectile_bonus");
            record.PierceBonus = GetInt(row, "pierce_bonus");
            record.HasReloadTimeMultiplier = TryGetFloat(row, "reload_time_multiplier", out var reloadTimeMultiplier);
            record.ReloadTimeMultiplier = reloadTimeMultiplier;
            record.HasShotIntervalMultiplier = TryGetFloat(row, "shot_interval_multiplier", out var shotIntervalMultiplier);
            record.ShotIntervalMultiplier = shotIntervalMultiplier;
            record.HasRadiusMultiplier = TryGetFloat(row, "radius_multiplier", out var radiusMultiplier);
            record.RadiusMultiplier = radiusMultiplier;
            record.RadiusBonus = GetFloat(row, "radius_bonus");
            record.HasDurationMultiplier = TryGetFloat(row, "duration_multiplier", out var durationMultiplier);
            record.DurationMultiplier = durationMultiplier;
            record.DurationBonus = GetFloat(row, "duration_bonus");
            record.BranchChanceBonus = GetFloat(row, "branch_chance_bonus");
            record.HasBranchChanceSet = TryGetFloat(row, "branch_chance_set", out var branchChanceSet);
            record.BranchChanceSet = branchChanceSet;
            record.HasBranchCount = TryGetInt(row, "branch_count", out var branchCount);
            record.BranchCount = branchCount;
            record.HasBranchDamageMultiplier = TryGetFloat(row, "branch_damage_multiplier", out var branchDamageMultiplier);
            record.BranchDamageMultiplier = branchDamageMultiplier;
            record.HasBranchSearchRadius = TryGetFloat(row, "branch_search_radius", out var branchSearchRadius);
            record.BranchSearchRadius = branchSearchRadius;
            record.StatusChanceBonus = GetFloat(row, "status_chance_bonus");
            record.StatusStacksBonus = GetInt(row, "status_stacks_bonus");
            record.HasStatusStacksSet = TryGetInt(row, "status_stacks_set", out var statusStacksSet);
            record.StatusStacksSet = statusStacksSet;
            record.StatusMaxStacksBonusStatusId = Get(row, "status_max_stacks_bonus_status_id");
            record.StatusMaxStacksBonus = GetInt(row, "status_max_stacks_bonus");
            record.StatusDurationBonusStatusId = Get(row, "status_duration_bonus_status_id");
            record.StatusDurationBonus = GetFloat(row, "status_duration_bonus");
            record.ThresholdStatusId = Get(row, "threshold_status_id");
            record.ThresholdStatusMinStacks = GetInt(row, "threshold_status_min_stacks");
            record.ThresholdApplyStatusId = Get(row, "threshold_apply_status_id");
            record.HasConditionalDamageMultiplier = TryGetFloat(row, "conditional_damage_multiplier", out var conditionalDamageMultiplier);
            record.ConditionalDamageMultiplier = conditionalDamageMultiplier;
            record.ConditionalTargetStatusId = Get(row, "conditional_target_status_id");
            record.ConditionalTargetStatusMinStacks = GetInt(row, "conditional_target_status_min_stacks");
            return string.IsNullOrWhiteSpace(record.ChoiceId) ? null : record;
        }

        private static string Get(IDictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) && !IsNull(value) ? value : string.Empty;
        }

        private static float GetFloat(IDictionary<string, string> row, string key)
        {
            return TryGetFloat(row, key, out var value) ? value : 0f;
        }

        private static int GetInt(IDictionary<string, string> row, string key)
        {
            return TryGetInt(row, key, out var value) ? value : 0;
        }

        private static bool TryGetFloat(IDictionary<string, string> row, string key, out float value)
        {
            value = 0f;
            return row.TryGetValue(key, out var raw)
                && !IsNull(raw)
                && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetInt(IDictionary<string, string> row, string key, out int value)
        {
            value = 0;
            return row.TryGetValue(key, out var raw)
                && !IsNull(raw)
                && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool IsNull(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase);
        }
    }
}
