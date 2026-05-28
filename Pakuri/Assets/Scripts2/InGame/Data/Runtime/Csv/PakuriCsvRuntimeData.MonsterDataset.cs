using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class MonsterRow
        {
            public string Id;
            public string DisplayName;
            public string RoleSummary;
            public string ElementLabel;
            public DamageAttribute PrimaryAttribute;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public string MonsterIconImagePath;
            public float MaxHealth;
            public float PowerStat;
            public float BaseDamage;
            public float PowerCoefficient;
            public float BaseAttackPower;
            public float BaseSpellPower;
            public float BaseMoveSpeed;
            public float BaseCriticalChance;
            public float BaseCriticalDamage;
            public float BaseCriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
        }

        private sealed class RewardChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public int SortOrder;
        }

        private sealed class SkillRow
        {
            public string Id;
            public string MonsterId;
            public PakuriCsvSkillKind SkillKind;
            public SkillSlot Slot;
            public string DisplayName;
            public SkillRuntimeKind RuntimeKind;
            public SkillImplementationState ImplementationState;
            public bool IsDefaultLearned;
            public bool IsAvailableWithoutActiveRequirement;
            public SkillSlot RequiredActiveSlot;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
            public string DescriptionText;
            public string Summary;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public float KnockbackDistance;
            public float DamageDelaySeconds;
            public float ExecuteHealthRatioThreshold;
            public bool RequireExecuteThresholdToCast;
            public float ExecuteDamageMultiplier = 1f;
            public float KillCooldownRefundRatio;
            public float BossDamageMultiplier = 1f;
            public string HitTargetCount;
            public string TargetSelection;
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
            public bool CriticalAllowed;
            public StatusPayloadRow Status = new StatusPayloadRow();
        }

        private sealed class SkillChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string SkillId;
            public string TargetSkillId;
            public PakuriCsvChoiceGroup ChoiceGroup;
            public int SortOrder;
            public string Title;
            public string DescriptionText;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
            public bool HasDamageMultiplier;
            public float DamageMultiplier = 1f;
            public float BaseDamageBonus;
            public bool HasCooldownMultiplier;
            public float CooldownMultiplier = 1f;
            public bool HasMagazineBonus;
            public int MagazineBonus;
            public int AdditionalProjectileBonus;
            public int PierceBonus;
            public bool HasShotIntervalMultiplier;
            public float ShotIntervalMultiplier = 1f;
            public bool HasBurstDamageProjectileIndex;
            public int BurstDamageProjectileIndex;
            public bool HasBurstDamageMultiplier;
            public float BurstDamageMultiplier = 1f;
            public int FollowUpProjectileCount;
            public float FollowUpProjectileDelaySeconds;
            public float FollowUpProjectileDamageMultiplier = 1f;
            public bool HasReloadTimeMultiplier;
            public float ReloadTimeMultiplier = 1f;
            public bool HasRadiusMultiplier;
            public float RadiusMultiplier = 1f;
            public float RadiusBonus;
            public float BeamWidthBonus;
            public bool HasKnockbackDistanceMultiplier;
            public float KnockbackDistanceMultiplier = 1f;
            public bool HasDamageDelayMultiplier;
            public float DamageDelayMultiplier = 1f;
            public bool HasExecuteHealthRatioBonus;
            public float ExecuteHealthRatioBonus;
            public bool HasDurationMultiplier;
            public float DurationMultiplier = 1f;
            public float DurationBonus;
            public float BranchChanceBonus;
            public bool HasBranchChanceSet;
            public float BranchChanceSet;
            public bool HasBranchCount;
            public int BranchCount;
            public bool HasBranchDamageMultiplier;
            public float BranchDamageMultiplier = 1f;
            public bool HasBranchSearchRadius;
            public float BranchSearchRadius;
            public int BranchLaunchPeriod;
            public bool HasBranchLaunchChanceSet;
            public float BranchLaunchChanceSet;
            public bool HasMaxHealthBonus;
            public float MaxHealthBonus;
            public int HitTargetCountBonus;
            public float CritChanceBonus;
            public float CritDamageBonus;
            public float ExecuteCritChanceBonus;
            public bool HasBossDamageMultiplier;
            public float BossDamageMultiplier = 1f;
            public bool HasKillCooldownRefundRatioBonus;
            public float KillCooldownRefundRatioBonus;
            public bool KillResetsCooldown;
            public bool KillResetsCooldownRequiresExecute;
            public string StatusTag;
            public bool HasStatusChanceBonus;
            public float StatusChanceBonus;
            public int StatusStacksBonus;
            public bool HasStatusStacksSet;
            public int StatusStacksSet;
            public bool HasStatusElementDamageTakenBonus;
            public float StatusElementDamageTakenBonus;
            public bool HasStatusCriticalDamageTakenBonus;
            public float StatusCriticalDamageTakenBonus;
            public bool HasStatusAilmentResistanceBonus;
            public float StatusAilmentResistanceBonus;
            public string StatusMaxStacksBonusStatusId;
            public int StatusMaxStacksBonus;
            public string StatusDurationBonusStatusId;
            public float StatusDurationBonus;
            public string ThresholdStatusId;
            public int ThresholdStatusMinStacks;
            public string ThresholdApplyStatusId;
            public bool HasConditionalDamageMultiplier;
            public float ConditionalDamageMultiplier = 1f;
            public string ConditionalTargetStatusId;
            public int ConditionalTargetStatusMinStacks;
            public string CountStatusId;
            public SkillMultiEffectTargetSide CountTargetSide;
            public float DamageMultiplierPerCount;
            public int CountMax;
            public float ConsecutiveHitBonusRate;
            public float ConsecutiveHitMax;
            public bool HasStatusConditionalDamageTakenBonus;
            public float StatusConditionalDamageTakenBonus;
            public string StatusConditionalSourceStatusId;
            public bool HasOnHitAdditionalDamage;
            public float OnHitAdditionalDamageChance;
            public float OnHitAdditionalDamageMultiplier = 1f;
            public DamageAttribute OnHitAdditionalDamageAttribute;
            public string OnHitAdditionalDamageTarget;
            public int OnHitChainHitPeriod;
            public int OnHitChainTargetCount;
            public float OnHitChainSearchRadius;
            public float OnHitChainDamageMultiplier = 1f;
            public DamageAttribute OnHitChainDamageAttribute;
            public string ReloadReduceTargetSkillId;
            public float ReloadReduceSecondsPerHit;
            public string CoreHitboxName;
            public bool HasCoreDamageMultiplier;
            public float CoreDamageMultiplier = 1f;
            public bool HasCoreOnHitAdditionalDamage;
            public float CoreOnHitAdditionalDamageChance;
            public float CoreOnHitAdditionalDamageMultiplier = 1f;
            public DamageAttribute CoreOnHitAdditionalDamageAttribute;
            public string HitCountCooldownRefundTargetSkillId;
            public int HitCountCooldownRefundMinTargets;
            public float HitCountCooldownRefundRatio;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        private sealed class SkillEffectRow
        {
            public string Id;
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
            public bool ApplyOnce;
            public string ConditionStatusId;
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
            public float TickIntervalSeconds;
            public StatusPayloadRow Status = new StatusPayloadRow();
            public string SkillEffectPrefabPath;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        private sealed class SkillTriggerRow
        {
            public string Id;
            public string MonsterId;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string RequiresActiveChoiceId;
            public string ExcludesActiveChoiceId;
            public string ConditionStatusId;
            public string ConditionStatusSourceSkillId;
            public string TriggerAttribute;
            public SkillTriggerActionKind TriggerAction;
            public string EventSkillId;
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
            public DamageAttribute TrackedAttribute;
            public float Radius;
            public bool CoverAll;
            public string HitTargetCount;
            public int RepeatCount = 1;
            public float RepeatIntervalSeconds;
            public bool RequireEventExecute;
            public float CooldownRefundRatio;
            public float ReloadReduceRatio;
            public string SkillEffectPrefabPath;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        private static MonsterRow ParseMonsterRow(CsvRecord record)
        {
            return new MonsterRow
            {
                Id = record.ReadRequiredString("id"),
                DisplayName = record.ReadRequiredString("display_name"),
                RoleSummary = record.ReadString("role_summary"),
                ElementLabel = record.ReadString("element_label"),
                PrimaryAttribute = record.ReadEnum<DamageAttribute>("primary_attribute"),
                ActiveSkillName = record.ReadString("active_skill_name"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                MonsterIconImagePath = ReadOptionalStringIfColumnExists(record, "MonsterIconImage"),
                MaxHealth = record.ReadFloat("max_health"),
                PowerStat = record.ReadFloat("power_stat"),
                BaseDamage = record.ReadFloat("base_damage"),
                PowerCoefficient = record.ReadFloat("power_coefficient"),
                BaseAttackPower = record.ReadFloat("base_attack_power"),
                BaseSpellPower = record.ReadFloat("base_spell_power"),
                BaseMoveSpeed = record.ReadFloat("base_move_speed"),
                BaseCriticalChance = record.ReadFloat("base_crit_chance"),
                BaseCriticalDamage = record.ReadFloat("base_crit_damage"),
                BaseCriticalResistance = record.ReadFloat("base_crit_resistance"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy")
            };
        }

        private static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record)
        {
            return new RewardChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                ActiveSkillId = record.ReadString("active_skill_id"),
                PassiveSkillId = record.ReadString("passive_skill_id"),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        private static SkillRow ParseSkillRow(CsvRecord record)
        {
            return new SkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SkillKind = record.ReadEnum<PakuriCsvSkillKind>("skill_kind"),
                Slot = record.ReadEnum<SkillSlot>("slot"),
                DisplayName = record.ReadRequiredString("display_name"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                ImplementationState = record.ReadEnum<SkillImplementationState>("implementation_state"),
                IsDefaultLearned = record.ReadBool("is_default_learned"),
                IsAvailableWithoutActiveRequirement = record.ReadBool("is_available_without_active_requirement"),
                RequiredActiveSlot = record.ReadEnum<SkillSlot>("required_active_slot"),
                SkillIconPath = record.ReadString("skill_icon_path"),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                DescriptionText = record.ReadString("description_text"),
                Summary = record.ReadString("summary"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                Radius = record.ReadFloat("radius"),
                KnockbackDistance = ReadOptionalFloatIfColumnExists(record, "knockback_distance"),
                DamageDelaySeconds = ReadOptionalFloatIfColumnExists(record, "damage_delay_seconds"),
                ExecuteHealthRatioThreshold = ReadOptionalFloatIfColumnExists(record, "execute_health_ratio_threshold"),
                RequireExecuteThresholdToCast = ReadOptionalBoolIfColumnExists(record, "require_execute_threshold_to_cast"),
                ExecuteDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "execute_damage_multiplier", 1f),
                KillCooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "kill_cooldown_refund_ratio"),
                BossDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "boss_damage_multiplier", 1f),
                HitTargetCount = record.ReadString("hit_target_count"),
                TargetSelection = record.ReadString("target_selection"),
                CooldownSeconds = record.ReadFloat("cooldown_seconds"),
                ActiveDurationSeconds = record.ReadFloat("active_duration_seconds"),
                MagazineCapacity = record.ReadInt("magazine_capacity"),
                ReloadSeconds = record.ReadFloat("reload_seconds"),
                ShotIntervalSeconds = record.ReadFloat("shot_interval_seconds"),
                BurstIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "burst_interval_seconds"),
                ProjectileBurstCount = record.ReadInt("projectile_burst_count"),
                BurstDamageProjectileIndex = ReadOptionalIntIfColumnExists(record, "burst_damage_projectile_index"),
                BurstDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "burst_damage_multiplier", 1f),
                ProjectileSpeed = record.ReadFloat("projectile_speed"),
                PierceCount = record.ReadInt("pierce_count"),
                CriticalAllowed = record.ReadBool("critical_allowed"),
                Status = ReadStatusPayload(record, false)
            };
        }

        private static SkillChoiceRow ParseSkillChoiceRow(CsvRecord record)
        {
            var row = new SkillChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SkillId = record.ReadRequiredString("skill_id"),
                TargetSkillId = record.ReadString("target_skill_id"),
                ChoiceGroup = record.ReadEnum<PakuriCsvChoiceGroup>("choice_group"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                DescriptionText = record.ReadString("description_text"),
                SkillIconPath = record.ReadString("skill_icon_path"),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                StatusTag = record.ReadString("status_tag"),
                RuntimeSupportState = record.ReadString("runtime_support_state"),
                RuntimeSupportNotes = record.ReadString("runtime_support_notes")
            };

            row.HasDamageMultiplier = TryReadFloat(record, "damage_multiplier", out var damageMultiplier);
            row.DamageMultiplier = damageMultiplier;
            row.BaseDamageBonus = ReadOptionalFloat(record, "base_damage_bonus");
            row.HasCooldownMultiplier = TryReadFloat(record, "cooldown_multiplier", out var cooldownMultiplier);
            row.CooldownMultiplier = cooldownMultiplier;
            row.HasMagazineBonus = TryReadInt(record, "magazine_bonus", out var magazineBonus);
            row.MagazineBonus = magazineBonus;
            row.AdditionalProjectileBonus = ReadOptionalInt(record, "additional_projectile_bonus");
            row.PierceBonus = ReadOptionalInt(record, "pierce_bonus");
            row.HasShotIntervalMultiplier = TryReadFloat(record, "shot_interval_multiplier", out var shotIntervalMultiplier);
            row.ShotIntervalMultiplier = shotIntervalMultiplier;
            var burstDamageProjectileIndex = 0;
            row.HasBurstDamageProjectileIndex = record.HasColumn("burst_damage_projectile_index")
                && TryReadInt(record, "burst_damage_projectile_index", out burstDamageProjectileIndex);
            row.BurstDamageProjectileIndex = burstDamageProjectileIndex;
            row.HasBurstDamageMultiplier = TryReadFloatIfColumnExists(record, "burst_damage_multiplier", out var burstDamageMultiplier);
            row.BurstDamageMultiplier = burstDamageMultiplier;
            row.FollowUpProjectileCount = ReadOptionalIntIfColumnExists(record, "follow_up_projectile_count");
            row.FollowUpProjectileDelaySeconds = ReadOptionalFloatIfColumnExists(record, "follow_up_projectile_delay_seconds");
            row.FollowUpProjectileDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "follow_up_projectile_damage_multiplier", 1f);
            row.HasReloadTimeMultiplier = TryReadFloat(record, "reload_time_multiplier", out var reloadTimeMultiplier);
            row.ReloadTimeMultiplier = reloadTimeMultiplier;
            row.HasRadiusMultiplier = TryReadFloat(record, "radius_multiplier", out var radiusMultiplier);
            row.RadiusMultiplier = radiusMultiplier;
            row.RadiusBonus = ReadOptionalFloat(record, "radius_bonus");
            row.BeamWidthBonus = ReadOptionalFloat(record, "beam_width_bonus");
            row.HasKnockbackDistanceMultiplier = TryReadFloatIfColumnExists(record, "knockback_distance_multiplier", out var knockbackDistanceMultiplier);
            row.KnockbackDistanceMultiplier = knockbackDistanceMultiplier;
            row.HasDamageDelayMultiplier = TryReadFloatIfColumnExists(record, "damage_delay_multiplier", out var damageDelayMultiplier);
            row.DamageDelayMultiplier = damageDelayMultiplier;
            row.HasExecuteHealthRatioBonus = TryReadFloatIfColumnExists(record, "execute_health_ratio_bonus", out var executeHealthRatioBonus);
            row.ExecuteHealthRatioBonus = executeHealthRatioBonus;
            row.HasDurationMultiplier = TryReadFloat(record, "duration_multiplier", out var durationMultiplier);
            row.DurationMultiplier = durationMultiplier;
            row.DurationBonus = ReadOptionalFloat(record, "duration_bonus");
            row.BranchChanceBonus = ReadOptionalFloat(record, "branch_chance_bonus");
            row.HasBranchChanceSet = TryReadFloat(record, "branch_chance_set", out var branchChanceSet);
            row.BranchChanceSet = branchChanceSet;
            row.HasBranchCount = TryReadInt(record, "branch_count", out var branchCount);
            row.BranchCount = branchCount;
            row.HasBranchDamageMultiplier = TryReadFloat(record, "branch_damage_multiplier", out var branchDamageMultiplier);
            row.BranchDamageMultiplier = branchDamageMultiplier;
            row.HasBranchSearchRadius = TryReadFloat(record, "branch_search_radius", out var branchSearchRadius);
            row.BranchSearchRadius = branchSearchRadius;
            row.BranchLaunchPeriod = ReadOptionalIntIfColumnExists(record, "branch_launch_period");
            row.HasBranchLaunchChanceSet = TryReadFloatIfColumnExists(record, "branch_launch_chance_set", out var branchLaunchChanceSet);
            row.BranchLaunchChanceSet = branchLaunchChanceSet;
            row.HasMaxHealthBonus = TryReadFloat(record, "max_health_bonus", out var maxHealthBonus);
            row.MaxHealthBonus = maxHealthBonus;
            row.HitTargetCountBonus = ReadOptionalInt(record, "hit_target_count_bonus");
            row.CritChanceBonus = ReadOptionalFloat(record, "crit_chance_bonus");
            row.CritDamageBonus = ReadOptionalFloat(record, "crit_damage_bonus");
            row.ExecuteCritChanceBonus = ReadOptionalFloatIfColumnExists(record, "execute_crit_chance_bonus");
            row.HasBossDamageMultiplier = TryReadFloatIfColumnExists(record, "boss_damage_multiplier", out var bossDamageMultiplier);
            row.BossDamageMultiplier = bossDamageMultiplier;
            row.HasKillCooldownRefundRatioBonus = TryReadFloatIfColumnExists(record, "kill_cooldown_refund_ratio_bonus", out var killCooldownRefundRatioBonus);
            row.KillCooldownRefundRatioBonus = killCooldownRefundRatioBonus;
            row.KillResetsCooldown = ReadOptionalBoolIfColumnExists(record, "kill_resets_cooldown");
            row.KillResetsCooldownRequiresExecute = ReadOptionalBoolIfColumnExists(record, "kill_resets_cooldown_requires_execute");
            row.HasStatusChanceBonus = TryReadFloat(record, "status_chance_bonus", out var statusChanceBonus);
            row.StatusChanceBonus = statusChanceBonus;
            row.StatusStacksBonus = ReadOptionalInt(record, "status_stacks_bonus");
            row.HasStatusStacksSet = TryReadInt(record, "status_stacks_set", out var statusStacksSet);
            row.StatusStacksSet = statusStacksSet;
            row.HasStatusElementDamageTakenBonus = TryReadFloat(record, "status_element_damage_taken_bonus", out var statusElementDamageTakenBonus);
            row.StatusElementDamageTakenBonus = statusElementDamageTakenBonus;
            row.HasStatusCriticalDamageTakenBonus = TryReadFloat(record, "status_critical_damage_taken_bonus", out var statusCriticalDamageTakenBonus);
            row.StatusCriticalDamageTakenBonus = statusCriticalDamageTakenBonus;
            row.HasStatusAilmentResistanceBonus = TryReadFloat(record, "status_ailment_resistance_bonus", out var statusAilmentResistanceBonus);
            row.StatusAilmentResistanceBonus = statusAilmentResistanceBonus;
            row.StatusMaxStacksBonusStatusId = record.ReadString("status_max_stacks_bonus_status_id");
            row.StatusMaxStacksBonus = ReadOptionalInt(record, "status_max_stacks_bonus");
            row.StatusDurationBonusStatusId = record.ReadString("status_duration_bonus_status_id");
            row.StatusDurationBonus = ReadOptionalFloat(record, "status_duration_bonus");
            row.ThresholdStatusId = record.ReadString("threshold_status_id");
            row.ThresholdStatusMinStacks = ReadOptionalInt(record, "threshold_status_min_stacks");
            row.ThresholdApplyStatusId = record.ReadString("threshold_apply_status_id");
            row.HasConditionalDamageMultiplier = TryReadFloat(record, "conditional_damage_multiplier", out var conditionalDamageMultiplier);
            row.ConditionalDamageMultiplier = conditionalDamageMultiplier;
            row.ConditionalTargetStatusId = record.ReadString("conditional_target_status_id");
            row.ConditionalTargetStatusMinStacks = ReadOptionalInt(record, "conditional_target_status_min_stacks");
            row.CountStatusId = record.ReadString("count_status_id");
            row.CountTargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("count_target_side");
            row.DamageMultiplierPerCount = ReadOptionalFloat(record, "damage_multiplier_per_count");
            row.CountMax = ReadOptionalInt(record, "count_max");
            row.ConsecutiveHitBonusRate = ReadOptionalFloatIfColumnExists(record, "consecutive_hit_bonus_rate");
            row.ConsecutiveHitMax = ReadOptionalFloatIfColumnExists(record, "consecutive_hit_max");
            row.HasStatusConditionalDamageTakenBonus = TryReadFloat(record, "status_conditional_damage_taken_bonus", out var statusConditionalDamageTakenBonus);
            row.StatusConditionalDamageTakenBonus = statusConditionalDamageTakenBonus;
            row.StatusConditionalSourceStatusId = record.ReadString("status_conditional_source_status_id");
            row.HasOnHitAdditionalDamage = TryReadFloatIfColumnExists(record, "on_hit_additional_damage_chance", out var onHitAdditionalDamageChance);
            row.OnHitAdditionalDamageChance = onHitAdditionalDamageChance;
            row.OnHitAdditionalDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "on_hit_additional_damage_multiplier");
            row.OnHitAdditionalDamageAttribute = ReadOptionalEnumIfColumnExists(record, "on_hit_additional_damage_attribute", DamageAttribute.Physical);
            row.OnHitAdditionalDamageTarget = ReadOptionalStringIfColumnExists(record, "on_hit_additional_damage_target");
            row.OnHitChainHitPeriod = ReadOptionalIntIfColumnExists(record, "on_hit_chain_hit_period");
            row.OnHitChainTargetCount = ReadOptionalIntIfColumnExists(record, "on_hit_chain_target_count");
            row.OnHitChainSearchRadius = ReadOptionalFloatIfColumnExists(record, "on_hit_chain_search_radius");
            row.OnHitChainDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "on_hit_chain_damage_multiplier");
            row.OnHitChainDamageAttribute = ReadOptionalEnumIfColumnExists(record, "on_hit_chain_damage_attribute", DamageAttribute.Physical);
            row.ReloadReduceTargetSkillId = ReadOptionalStringIfColumnExists(record, "reload_reduce_target_skill_id");
            row.ReloadReduceSecondsPerHit = ReadOptionalFloatIfColumnExists(record, "reload_reduce_seconds_per_hit");
            row.CoreHitboxName = ReadOptionalStringIfColumnExists(record, "core_hitbox_name");
            row.HasCoreDamageMultiplier = TryReadFloatIfColumnExists(record, "core_damage_multiplier", out var coreDamageMultiplier);
            row.CoreDamageMultiplier = coreDamageMultiplier;
            row.HasCoreOnHitAdditionalDamage = TryReadFloatIfColumnExists(record, "core_on_hit_additional_damage_chance", out var coreOnHitAdditionalDamageChance);
            row.CoreOnHitAdditionalDamageChance = coreOnHitAdditionalDamageChance;
            row.CoreOnHitAdditionalDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "core_on_hit_additional_damage_multiplier");
            row.CoreOnHitAdditionalDamageAttribute = ReadOptionalEnumIfColumnExists(record, "core_on_hit_additional_damage_attribute", DamageAttribute.Physical);
            row.HitCountCooldownRefundTargetSkillId = ReadOptionalStringIfColumnExists(record, "hit_count_cooldown_refund_target_skill_id");
            row.HitCountCooldownRefundMinTargets = ReadOptionalIntIfColumnExists(record, "hit_count_cooldown_refund_min_targets");
            row.HitCountCooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "hit_count_cooldown_refund_ratio");
            return row;
        }

        private static SkillEffectRow ParseSkillEffectRow(CsvRecord record)
        {
            var row = new SkillEffectRow
            {
                Id = record.ReadRequiredString("effect_id"),
                SkillId = record.ReadRequiredString("skill_id"),
                SortOrder = record.ReadInt("sort_order"),
                EffectKind = record.ReadEnum<SkillMultiEffectKind>("effect_kind"),
                TargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("target_side"),
                TargetSelection = record.ReadEnum<SkillMultiEffectTargetSelection>("target_selection"),
                TargetShape = record.ReadEnum<SkillMultiEffectTargetShape>("target_shape"),
                CenterMode = record.ReadEnum<SkillMultiEffectCenterMode>("center_mode"),
                VisualAnchorMode = record.ReadEnum<SkillMultiEffectVisualAnchorMode>("visual_anchor_mode"),
                EffectTiming = record.ReadEnum<SkillMultiEffectTiming>("effect_timing"),
                DelaySeconds = record.ReadFloat("delay_seconds"),
                EnabledByDefault = record.ReadBool("enabled_by_default"),
                RequiresActiveChoiceId = record.ReadString("requires_active_choice_id"),
                ExcludesActiveChoiceId = record.ReadString("excludes_active_choice_id"),
                RequiresPassiveSkillId = record.ReadString("requires_passive_skill_id"),
                ExcludesPassiveSkillId = record.ReadString("excludes_passive_skill_id"),
                ApplyOnce = record.ReadBool("apply_once"),
                ConditionStatusId = record.ReadString("condition_status_id"),
                ConditionTargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("condition_target_side"),
                ConditionSkillAttribute = record.ReadString("condition_skill_attribute"),
                ConditionHealthRatioMax = ReadOptionalFloatIfColumnExists(record, "condition_health_ratio_max"),
                ConditionHitCountMin = ReadOptionalIntIfColumnExists(record, "condition_hit_count_min"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                DamageMultiplier = ReadOptionalFloat(record, "damage_multiplier"),
                Radius = record.ReadFloat("radius"),
                CoverAll = record.ReadBool("cover_all"),
                ActiveDurationSeconds = ReadOptionalFloatIfColumnExists(record, "active_duration_seconds"),
                TickIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "tick_interval_seconds"),
                Status = ReadStatusPayload(record, true),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                RuntimeSupportState = record.ReadString("runtime_support_state"),
                RuntimeSupportNotes = record.ReadString("runtime_support_notes")
            };

            if (row.DamageMultiplier <= 0f)
            {
                row.DamageMultiplier = 1f;
            }

            return row;
        }

        private static SkillTriggerRow ParseSkillTriggerRow(CsvRecord record)
        {
            var row = new SkillTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                RequiresActiveChoiceId = record.ReadString("requires_active_choice_id"),
                ExcludesActiveChoiceId = record.ReadString("excludes_active_choice_id"),
                ConditionStatusId = record.ReadString("condition_status_id"),
                ConditionStatusSourceSkillId = ReadOptionalStringIfColumnExists(record, "condition_status_source_skill_id"),
                TriggerAttribute = record.ReadString("trigger_attribute"),
                TriggerAction = ReadOptionalEnumIfColumnExists(record, "trigger_action", SkillTriggerActionKind.Auto),
                EventSkillId = ReadOptionalStringIfColumnExists(record, "event_skill_id"),
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                TriggeredEffectId = ReadOptionalStringIfColumnExists(record, "triggered_effect_id"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                TargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("target_side"),
                TargetSelection = record.ReadEnum<SkillMultiEffectTargetSelection>("target_selection"),
                TargetShape = record.ReadEnum<SkillMultiEffectTargetShape>("target_shape"),
                CenterMode = record.ReadEnum<SkillMultiEffectCenterMode>("center_mode"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                DamageMultiplier = ReadOptionalFloat(record, "damage_multiplier"),
                DamageSource = record.ReadEnum<SkillTriggerDamageSource>("damage_source"),
                DamageSourceMultiplier = record.ReadFloat("damage_source_multiplier"),
                TrackedAttribute = record.ReadEnum<DamageAttribute>("tracked_attribute"),
                Radius = record.ReadFloat("radius"),
                CoverAll = record.ReadBool("cover_all"),
                HitTargetCount = record.ReadString("hit_target_count"),
                RepeatCount = record.ReadInt("repeat_count"),
                RepeatIntervalSeconds = record.ReadFloat("repeat_interval_seconds"),
                TriggerDelaySeconds = ReadOptionalFloatIfColumnExists(record, "trigger_delay_seconds"),
                TriggerEveryCount = ReadOptionalIntIfColumnExists(record, "trigger_every_count"),
                EventSourceScope = ReadOptionalStringIfColumnExists(record, "event_source_scope"),
                RequireEventExecute = ReadOptionalBoolIfColumnExists(record, "require_event_execute"),
                CooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "cooldown_refund_ratio"),
                ReloadReduceRatio = ReadOptionalFloatIfColumnExists(record, "reload_reduce_ratio"),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                RuntimeSupportState = record.ReadString("runtime_support_state"),
                RuntimeSupportNotes = record.ReadString("runtime_support_notes")
            };

            if (TryReadFloat(record, "proc_chance", out var procChance))
            {
                row.ProcChance = procChance;
            }

            if (TryReadFloat(record, "internal_cooldown_seconds", out var internalCooldownSeconds))
            {
                row.InternalCooldownSeconds = internalCooldownSeconds;
            }

            if (row.DamageMultiplier <= 0f)
            {
                row.DamageMultiplier = 1f;
            }

            if (row.ProcChance <= 0f)
            {
                row.ProcChance = 1f;
            }

            if (row.RepeatCount <= 0)
            {
                row.RepeatCount = 1;
            }

            if (row.TriggerEveryCount < 0)
            {
                row.TriggerEveryCount = 0;
            }

            return row;
        }

        private static float ReadOptionalFloat(CsvRecord record, string columnName)
        {
            return TryReadFloat(record, columnName, out var value) ? value : 0f;
        }

        private static int ReadOptionalInt(CsvRecord record, string columnName)
        {
            return TryReadInt(record, columnName, out var value) ? value : 0;
        }

        private static int ReadOptionalIntIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? ReadOptionalInt(record, columnName) : 0;
        }

        private static float ReadOptionalFloatIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? ReadOptionalFloat(record, columnName) : 0f;
        }

        private static float ReadOptionalFloatWithDefaultIfColumnExists(CsvRecord record, string columnName, float fallback)
        {
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out var value)
                ? value
                : fallback;
        }

        private static string ReadOptionalStringIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadString(columnName) : string.Empty;
        }

        private static bool ReadOptionalBoolIfColumnExists(CsvRecord record, string columnName)
        {
            if (!record.HasColumn(columnName))
            {
                return false;
            }

            var raw = record.ReadString(columnName);
            return !string.IsNullOrWhiteSpace(raw) && record.ReadBool(columnName);
        }

        private static T ReadOptionalEnumIfColumnExists<T>(CsvRecord record, string columnName, T fallback) where T : struct
        {
            if (!record.HasColumn(columnName))
            {
                return fallback;
            }

            var raw = record.ReadString(columnName);
            return !string.IsNullOrWhiteSpace(raw) && Enum.TryParse(raw, true, out T value)
                ? value
                : fallback;
        }

        private static bool TryReadFloatIfColumnExists(CsvRecord record, string columnName, out float value)
        {
            value = 0f;
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out value);
        }

        private static bool TryReadFloat(CsvRecord record, string columnName, out float value)
        {
            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                value = 0f;
                return false;
            }

            value = record.ReadFloat(columnName);
            return true;
        }

        private static bool TryReadInt(CsvRecord record, string columnName, out int value)
        {
            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                value = 0;
                return false;
            }

            value = record.ReadInt(columnName);
            return true;
        }

        private static void ValidateExpectedSlots(
            string monsterId,
            HashSet<SkillSlot> slots,
            SkillSlot first,
            SkillSlot last,
            string kindLabel,
            List<string> errors)
        {
            for (var slot = first; slot <= last; slot++)
            {
                if (!slots.Contains(slot))
                {
                    errors.Add($"Monster '{monsterId}' is missing {kindLabel} slot '{slot}'.");
                }
            }
        }
    }
}
