using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;

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
        public bool HasBurstDamageProjectileIndex { get; set; }
        public int BurstDamageProjectileIndex { get; set; }
        public bool HasBurstDamageMultiplier { get; set; }
        public float BurstDamageMultiplier { get; set; }
        public int FollowUpProjectileCount { get; set; }
        public float FollowUpProjectileDelaySeconds { get; set; }
        public float FollowUpProjectileDamageMultiplier { get; set; }
        public bool HasRadiusMultiplier { get; set; }
        public float RadiusMultiplier { get; set; }
        public float RadiusBonus { get; set; }
        public bool HasKnockbackDistanceMultiplier { get; set; }
        public float KnockbackDistanceMultiplier { get; set; }
        public bool HasDamageDelayMultiplier { get; set; }
        public float DamageDelayMultiplier { get; set; }
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
        public int BranchLaunchPeriod { get; set; }
        public bool HasBranchLaunchChanceSet { get; set; }
        public float BranchLaunchChanceSet { get; set; }
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
        public bool HasTargetStatusStackDamageMultiplier { get; set; }
        public float TargetStatusStackDamageMultiplier { get; set; }
        public bool HasConsumeTargetStatusRatioOverride { get; set; }
        public float ConsumeTargetStatusRatioOverride { get; set; }
        public bool HasConsumeTargetStatusStacksOverride { get; set; }
        public int ConsumeTargetStatusStacksOverride { get; set; }
        public float ConditionalCritChanceBonus { get; set; }
        public string ConditionalCritTargetStatusId { get; set; }
        public int ConditionalCritTargetStatusMinStacks { get; set; }
        public float RedistributeConsumedStatusRatioOnKill { get; set; }
        public string RedistributeConsumedStatusId { get; set; }
        public float RedistributeConsumedStatusSearchRadius { get; set; }
        public int RedistributeConsumedStatusTargetCount { get; set; }
        public bool HasOnHitAdditionalDamage { get; set; }
        public float OnHitAdditionalDamageChance { get; set; }
        public float OnHitAdditionalDamageMultiplier { get; set; }
        public DamageAttribute OnHitAdditionalDamageAttribute { get; set; }
        public string OnHitAdditionalDamageTarget { get; set; }
        public int OnHitChainHitPeriod { get; set; }
        public int OnHitChainTargetCount { get; set; }
        public float OnHitChainSearchRadius { get; set; }
        public float OnHitChainDamageMultiplier { get; set; }
        public DamageAttribute OnHitChainDamageAttribute { get; set; }
        public string ReloadReduceTargetSkillId { get; set; }
        public float ReloadReduceSecondsPerHit { get; set; }
        public string CoreHitboxName { get; set; }
        public bool HasCoreDamageMultiplier { get; set; }
        public float CoreDamageMultiplier { get; set; }
        public bool HasCoreOnHitAdditionalDamage { get; set; }
        public float CoreOnHitAdditionalDamageChance { get; set; }
        public float CoreOnHitAdditionalDamageMultiplier { get; set; }
        public DamageAttribute CoreOnHitAdditionalDamageAttribute { get; set; }
        public string HitCountCooldownRefundTargetSkillId { get; set; }
        public int HitCountCooldownRefundMinTargets { get; set; }
        public float HitCountCooldownRefundRatio { get; set; }

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
            record.HasBurstDamageProjectileIndex = TryGetInt(row, "burst_damage_projectile_index", out var burstDamageProjectileIndex);
            record.BurstDamageProjectileIndex = burstDamageProjectileIndex;
            record.HasBurstDamageMultiplier = TryGetFloat(row, "burst_damage_multiplier", out var burstDamageMultiplier);
            record.BurstDamageMultiplier = burstDamageMultiplier;
            record.FollowUpProjectileCount = GetInt(row, "follow_up_projectile_count");
            record.FollowUpProjectileDelaySeconds = GetFloat(row, "follow_up_projectile_delay_seconds");
            record.FollowUpProjectileDamageMultiplier = GetFloat(row, "follow_up_projectile_damage_multiplier");
            record.HasRadiusMultiplier = TryGetFloat(row, "radius_multiplier", out var radiusMultiplier);
            record.RadiusMultiplier = radiusMultiplier;
            record.RadiusBonus = GetFloat(row, "radius_bonus");
            record.HasKnockbackDistanceMultiplier = TryGetFloat(row, "knockback_distance_multiplier", out var knockbackDistanceMultiplier);
            record.KnockbackDistanceMultiplier = knockbackDistanceMultiplier;
            record.HasDamageDelayMultiplier = TryGetFloat(row, "damage_delay_multiplier", out var damageDelayMultiplier);
            record.DamageDelayMultiplier = damageDelayMultiplier;
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
            record.BranchLaunchPeriod = GetInt(row, "branch_launch_period");
            record.HasBranchLaunchChanceSet = TryGetFloat(row, "branch_launch_chance_set", out var branchLaunchChanceSet);
            record.BranchLaunchChanceSet = branchLaunchChanceSet;
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
            record.HasTargetStatusStackDamageMultiplier = TryGetFloat(row, "target_status_stack_damage_multiplier", out var targetStatusStackDamageMultiplier);
            record.TargetStatusStackDamageMultiplier = targetStatusStackDamageMultiplier;
            record.HasConsumeTargetStatusRatioOverride = TryGetFloat(row, "consume_target_status_ratio_override", out var consumeTargetStatusRatioOverride);
            record.ConsumeTargetStatusRatioOverride = consumeTargetStatusRatioOverride;
            record.HasConsumeTargetStatusStacksOverride = TryGetInt(row, "consume_target_status_stacks_override", out var consumeTargetStatusStacksOverride);
            record.ConsumeTargetStatusStacksOverride = consumeTargetStatusStacksOverride;
            record.ConditionalCritChanceBonus = GetFloat(row, "conditional_crit_chance_bonus");
            record.ConditionalCritTargetStatusId = Get(row, "conditional_crit_target_status_id");
            record.ConditionalCritTargetStatusMinStacks = GetInt(row, "conditional_crit_target_status_min_stacks");
            record.RedistributeConsumedStatusRatioOnKill = GetFloat(row, "redistribute_consumed_status_ratio_on_kill");
            record.RedistributeConsumedStatusId = Get(row, "redistribute_consumed_status_id");
            record.RedistributeConsumedStatusSearchRadius = GetFloat(row, "redistribute_consumed_status_search_radius");
            record.RedistributeConsumedStatusTargetCount = GetInt(row, "redistribute_consumed_status_target_count");
            record.HasOnHitAdditionalDamage = TryGetFloat(row, "on_hit_additional_damage_chance", out var onHitAdditionalDamageChance);
            record.OnHitAdditionalDamageChance = onHitAdditionalDamageChance;
            record.OnHitAdditionalDamageMultiplier = GetFloat(row, "on_hit_additional_damage_multiplier");
            record.OnHitAdditionalDamageAttribute = GetEnum(row, "on_hit_additional_damage_attribute", DamageAttribute.Physical);
            record.OnHitAdditionalDamageTarget = Get(row, "on_hit_additional_damage_target");
            record.OnHitChainHitPeriod = GetInt(row, "on_hit_chain_hit_period");
            record.OnHitChainTargetCount = GetInt(row, "on_hit_chain_target_count");
            record.OnHitChainSearchRadius = GetFloat(row, "on_hit_chain_search_radius");
            record.OnHitChainDamageMultiplier = GetFloat(row, "on_hit_chain_damage_multiplier");
            record.OnHitChainDamageAttribute = GetEnum(row, "on_hit_chain_damage_attribute", DamageAttribute.Physical);
            record.ReloadReduceTargetSkillId = Get(row, "reload_reduce_target_skill_id");
            record.ReloadReduceSecondsPerHit = GetFloat(row, "reload_reduce_seconds_per_hit");
            record.CoreHitboxName = Get(row, "core_hitbox_name");
            record.HasCoreDamageMultiplier = TryGetFloat(row, "core_damage_multiplier", out var coreDamageMultiplier);
            record.CoreDamageMultiplier = coreDamageMultiplier;
            record.HasCoreOnHitAdditionalDamage = TryGetFloat(row, "core_on_hit_additional_damage_chance", out var coreOnHitAdditionalDamageChance);
            record.CoreOnHitAdditionalDamageChance = coreOnHitAdditionalDamageChance;
            record.CoreOnHitAdditionalDamageMultiplier = GetFloat(row, "core_on_hit_additional_damage_multiplier");
            record.CoreOnHitAdditionalDamageAttribute = GetEnum(row, "core_on_hit_additional_damage_attribute", DamageAttribute.Physical);
            record.HitCountCooldownRefundTargetSkillId = Get(row, "hit_count_cooldown_refund_target_skill_id");
            record.HitCountCooldownRefundMinTargets = GetInt(row, "hit_count_cooldown_refund_min_targets");
            record.HitCountCooldownRefundRatio = GetFloat(row, "hit_count_cooldown_refund_ratio");
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

        private static T GetEnum<T>(IDictionary<string, string> row, string key, T fallback) where T : struct
        {
            var raw = Get(row, key);
            return !string.IsNullOrWhiteSpace(raw) && Enum.TryParse(raw, true, out T value)
                ? value
                : fallback;
        }

        private static bool IsNull(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase);
        }
    }
}
