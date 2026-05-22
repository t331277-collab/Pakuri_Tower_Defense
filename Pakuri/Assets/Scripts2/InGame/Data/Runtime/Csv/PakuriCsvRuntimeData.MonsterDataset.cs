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
            public string DescriptionText;
            public string Summary;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public string HitTargetCount;
            public string TargetSelection;
            public float CooldownSeconds;
            public float ActiveDurationSeconds;
            public int MagazineCapacity;
            public float ReloadSeconds;
            public float ShotIntervalSeconds;
            public int ProjectileBurstCount;
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
            public bool HasReloadTimeMultiplier;
            public float ReloadTimeMultiplier = 1f;
            public bool HasRadiusMultiplier;
            public float RadiusMultiplier = 1f;
            public float RadiusBonus;
            public float BeamWidthBonus;
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
            public bool HasMaxHealthBonus;
            public float MaxHealthBonus;
            public int HitTargetCountBonus;
            public float CritChanceBonus;
            public float CritDamageBonus;
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
            public string CountStatusId;
            public SkillMultiEffectTargetSide CountTargetSide;
            public float DamageMultiplierPerCount;
            public int CountMax;
            public bool HasStatusConditionalDamageTakenBonus;
            public float StatusConditionalDamageTakenBonus;
            public string StatusConditionalSourceStatusId;
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
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float DamageMultiplier = 1f;
            public float Radius;
            public bool CoverAll;
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
            public string TriggeredSkillId;
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
                DescriptionText = record.ReadString("description_text"),
                Summary = record.ReadString("summary"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                Radius = record.ReadFloat("radius"),
                HitTargetCount = record.ReadString("hit_target_count"),
                TargetSelection = record.ReadString("target_selection"),
                CooldownSeconds = record.ReadFloat("cooldown_seconds"),
                ActiveDurationSeconds = record.ReadFloat("active_duration_seconds"),
                MagazineCapacity = record.ReadInt("magazine_capacity"),
                ReloadSeconds = record.ReadFloat("reload_seconds"),
                ShotIntervalSeconds = record.ReadFloat("shot_interval_seconds"),
                ProjectileBurstCount = record.ReadInt("projectile_burst_count"),
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
            row.HasReloadTimeMultiplier = TryReadFloat(record, "reload_time_multiplier", out var reloadTimeMultiplier);
            row.ReloadTimeMultiplier = reloadTimeMultiplier;
            row.HasRadiusMultiplier = TryReadFloat(record, "radius_multiplier", out var radiusMultiplier);
            row.RadiusMultiplier = radiusMultiplier;
            row.RadiusBonus = ReadOptionalFloat(record, "radius_bonus");
            row.BeamWidthBonus = ReadOptionalFloat(record, "beam_width_bonus");
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
            row.HasMaxHealthBonus = TryReadFloat(record, "max_health_bonus", out var maxHealthBonus);
            row.MaxHealthBonus = maxHealthBonus;
            row.HitTargetCountBonus = ReadOptionalInt(record, "hit_target_count_bonus");
            row.CritChanceBonus = ReadOptionalFloat(record, "crit_chance_bonus");
            row.CritDamageBonus = ReadOptionalFloat(record, "crit_damage_bonus");
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
            row.CountStatusId = record.ReadString("count_status_id");
            row.CountTargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("count_target_side");
            row.DamageMultiplierPerCount = ReadOptionalFloat(record, "damage_multiplier_per_count");
            row.CountMax = ReadOptionalInt(record, "count_max");
            row.HasStatusConditionalDamageTakenBonus = TryReadFloat(record, "status_conditional_damage_taken_bonus", out var statusConditionalDamageTakenBonus);
            row.StatusConditionalDamageTakenBonus = statusConditionalDamageTakenBonus;
            row.StatusConditionalSourceStatusId = record.ReadString("status_conditional_source_status_id");
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
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                DamageMultiplier = ReadOptionalFloat(record, "damage_multiplier"),
                Radius = record.ReadFloat("radius"),
                CoverAll = record.ReadBool("cover_all"),
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
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
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
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                RuntimeSupportState = record.ReadString("runtime_support_state"),
                RuntimeSupportNotes = record.ReadString("runtime_support_notes")
            };

            if (row.DamageMultiplier <= 0f)
            {
                row.DamageMultiplier = 1f;
            }

            if (row.RepeatCount <= 0)
            {
                row.RepeatCount = 1;
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
