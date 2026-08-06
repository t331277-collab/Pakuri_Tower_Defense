/*
 * 역할: 타입 지정 CSV 행 파싱.
 * 책임: 이름 기반 Column을 읽어 유닛·스킬·상태·Trigger·카탈로그 행으로 변환한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// CsvRowParser 원본 값을 런타임 모델로 파싱한다.
    internal static class CsvRowParser
    {

        /// MonsterRow에 해당하는 CSV 한 행을 표현한다.
        internal class MonsterRow
        {
            public string Name;
            public string DisplayName;
            public string RoleSummary;
            public string ElementLabel;
            public DamageAttribute PrimaryAttribute;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public string MonsterIconImagePath;
            public string ImagePath;
            public float MaxHealth;
            public float PowerStat;
            public float BaseAttackPower;
            public float BaseSpellPower;
            public float BaseMoveSpeed;
            public float BaseCriticalChance;
            public float BaseCriticalDamage;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
        }

        internal class ArtifactRow
        {
            public string Name;
            public string DisplayName;
            public string SynergyName;
            public string DescriptionText;
            public string IconPath;
        }

        internal class ArtifactSynergyLevelRow
        {
            public string Name;
            public int RequiredCount;
            public string DescriptionText;
        }

        internal class ArtifactSynergyRow
        {
            public string Name;
            public string DisplayName;
            public string Summary;
            public string DescriptionText;
            public string IconPath;
            public ArtifactSynergyLevelRow[] Levels = Array.Empty<ArtifactSynergyLevelRow>();
        }

        internal class ArtifactEffectRow
        {
            public string Name;
            public string ArtifactName;
            public ArtifactEffectApplicationMode ApplicationMode;
            public ArtifactEffectRecipient Recipient;
            public ArtifactEffectRepeatRule RepeatRule;
            public ArtifactEffectSelectionRule SelectionRule;
            public string RecipientMonsterName;
            public string TargetSkillName;
            public string OutcomeSkillName;
        }

        internal class ArtifactSynergyEffectRow
        {
            public string Name;
            public string SynergyLevelName;
            public ArtifactEffectApplicationMode ApplicationMode;
            public ArtifactEffectRecipient Recipient;
            public string RecipientMonsterName;
            public string TargetSkillName;
            public string OutcomeSkillName;
            public string SpawnSummonName;
        }

        /// RewardChoiceRow에 해당하는 CSV 한 행을 표현한다.
        internal class RewardChoiceRow
        {
            public string Name;
            public string MonsterName;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public int SortOrder;
        }

        /// SkillRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillRow
        {
            public string Name;
            public string MonsterName;
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
            public string RuntimeVisualSpritePath;
            public string RuntimeVisualAnimatorControllerPath;
            public float RuntimeVisualScale = 1f;
            public float RuntimeVisualScaleX;
            public float RuntimeVisualScaleY;
            public float RuntimeVisualScaleZ;
            public int RuntimeVisualSortingOrder;
            public string RuntimeVisualAnchor;
            public float RuntimeHitboxSizeX;
            public float RuntimeHitboxSizeY;
            public string RuntimeImpactVisualSpritePath;
            public string RuntimeImpactVisualAnimatorControllerPath;
            public float RuntimeImpactVisualScale = 1f;
            public int RuntimeImpactVisualSortingOrder;
            public string DescriptionText;
            public string Summary;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public float LineLength;
            public int CastRepeatCount = 1;
            public float CastRepeatIntervalSeconds;
            public float KnockbackDistance;
            public float DamageDelaySeconds;
            public float ExecuteHealthRatioThreshold;
            public bool RequireExecuteThresholdToCast;
            public float ExecuteDamageMultiplier = 1f;
            public float KillCooldownRefundRatio;
            public float BossDamageMultiplier = 1f;
            public string HitTargetCount;
            public bool UsePrefabHitbox;
            public string TargetSelection;
            public string TargetSelectionStatusName;
            public int TargetSelectionStatusMinStacks;
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
            public string DeploymentRequiredTargetStatusName;
            public int DeploymentRequiredTargetStatusMinStacks;
            public string TargetStatusStackStatusName;
            public int TargetStatusStackMaxStacks;
            public float TargetStatusStackBaseDamage;
            public float TargetStatusStackAttackPowerCoefficient;
            public float TargetStatusStackSpellPowerCoefficient;
            public string ConsumeTargetStatusName;
            public float ConsumeTargetStatusRatio;
            public int ConsumeTargetStatusStacks;
            public StatusPayloadRow Status = new StatusPayloadRow();
        }

        /// SkillChoiceRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillChoiceRow
        {
            public string Name;
            public string MonsterName;
            public string SkillName;
            public string TargetSkillName;
            public SkillChoiceGroup ChoiceGroup;
            public int SortOrder;
            public string Title;
            public string DescriptionText;
            public string SkillIconPath;
        }

        /// SkillTriggerRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillTriggerRow
        {
            public string Name;
            public string MonsterName;
            public string SourceSkillName;
            public SkillTriggerEvent TriggerEvent;
            public string RequiresActiveChoiceName;
            public string ExcludesActiveChoiceName;
            public string RequiredSourceStatusName;
            public int RequiredSourceStatusMinStacks;
            public string ConditionStatusName;
            public string ConditionStatusSourceSkillName;
            public string TriggerAttribute;
            public string EventSkillName;
            public string EventSkillRuntimeKinds;
            public float ProcChance = 1f;
            public float InternalCooldownSeconds;
            public float TriggerDelaySeconds;
            public int TriggerEveryCount;
            public string EventSourceScope;
            public int SortOrder;
            public int RepeatCount = 1;
            public float RepeatIntervalSeconds;
            public bool RequireEventExecute;
        }

        internal static MonsterRow ParseMonsterRow(CsvRecord record)
        {
            return new MonsterRow
            {
                Name = record.ReadRequiredString("Name"),
                DisplayName = record.ReadRequiredString("display_name"),
                RoleSummary = record.ReadString("role_summary"),
                ElementLabel = record.ReadString("element_label"),
                PrimaryAttribute = record.ReadEnum<DamageAttribute>("primary_attribute"),
                ActiveSkillName = ReadOptionalStringIfColumnExists(record, "active_skill_name"),
                PassiveSkillName = ReadOptionalStringIfColumnExists(record, "passive_skill_name"),
                MonsterIconImagePath = ReadOptionalStringIfColumnExists(record, "MonsterIconImage"),
                ImagePath = ReadOptionalStringIfColumnExists(record, "Image"),
                MaxHealth = record.ReadFloat("max_health"),
                PowerStat = record.ReadFloat("power_stat"),
                BaseAttackPower = record.ReadFloat("base_attack_power"),
                BaseSpellPower = record.ReadFloat("base_spell_power"),
                BaseMoveSpeed = record.ReadFloat("base_move_speed"),
                BaseCriticalChance = record.ReadFloat("base_crit_chance"),
                BaseCriticalDamage = record.ReadFloat("base_crit_damage"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy")
            };
        }

        internal static ArtifactRow ParseArtifactRow(CsvRecord record)
        {
            return new ArtifactRow
            {
                Name = record.ReadRequiredString("artifact_name"),
                DisplayName = record.ReadRequiredString("artifact_display_name"),
                SynergyName = record.ReadRequiredString("synergy_name"),
                DescriptionText = record.ReadString("description_text"),
                IconPath = record.ReadString("artifact_icon")
            };
        }

        internal static ArtifactSynergyRow ParseArtifactSynergyRow(CsvRecord record)
        {
            var levels = new ArtifactSynergyLevelRow[4];
            for (var i = 0; i < levels.Length; i++)
            {
                var prefix = "level_" + (i + 1) + "_";
                levels[i] = new ArtifactSynergyLevelRow
                {
                    Name = record.ReadRequiredString(prefix + "Name"),
                    RequiredCount = record.ReadInt(prefix + "required_count"),
                    DescriptionText = record.ReadString(prefix + "description_text")
                };
            }

            return new ArtifactSynergyRow
            {
                Name = record.ReadRequiredString("synergy_name"),
                DisplayName = record.ReadRequiredString("synergy_display_name"),
                Summary = record.ReadString("summary"),
                DescriptionText = record.ReadString("description_text"),
                IconPath = record.ReadString("Icon_Image"),
                Levels = levels
            };
        }

        internal static ArtifactEffectRow ParseArtifactEffectRow(CsvRecord record)
        {
            return new ArtifactEffectRow
            {
                Name = record.ReadRequiredString("effect_name"),
                ArtifactName = record.ReadRequiredString("artifact_name"),
                ApplicationMode = record.ReadEnum<ArtifactEffectApplicationMode>("application_mode"),
                Recipient = record.ReadEnum<ArtifactEffectRecipient>("recipient_scope"),
                RepeatRule = record.ReadEnum<ArtifactEffectRepeatRule>("repeat_rule"),
                SelectionRule = record.ReadEnum<ArtifactEffectSelectionRule>("selection_rule"),
                RecipientMonsterName = record.ReadString("recipient_monster_name"),
                TargetSkillName = record.ReadString("target_skill_name"),
                OutcomeSkillName = record.ReadString("outcome_skill_name")
            };
        }

        internal static ArtifactSynergyEffectRow ParseArtifactSynergyEffectRow(CsvRecord record)
        {
            return new ArtifactSynergyEffectRow
            {
                Name = record.ReadRequiredString("effect_name"),
                SynergyLevelName = record.ReadRequiredString("synergy_level_name"),
                ApplicationMode = record.ReadEnum<ArtifactEffectApplicationMode>("application_mode"),
                Recipient = record.ReadEnum<ArtifactEffectRecipient>("recipient_scope"),
                RecipientMonsterName = record.ReadString("recipient_monster_name"),
                TargetSkillName = record.ReadString("target_skill_name"),
                OutcomeSkillName = record.ReadString("outcome_skill_name"),
                SpawnSummonName = record.ReadString("spawn_monster_name")
            };
        }

        internal static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record)
        {
            return new RewardChoiceRow
            {
                Name = record.ReadRequiredString("choice_name"),
                MonsterName = record.ReadRequiredString("monster_name"),
                ActiveSkillName = record.ReadString("active_skill_name"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        internal static SkillRow ParseSkillRow(
            CsvRecord record,
            PakuriCsvSkillKind skillKind,
            string ownerIdOverride = null)
        {
            var slot = record.ReadEnum<SkillSlot>("slot");
            var monsterName = ownerIdOverride;
            if (string.IsNullOrWhiteSpace(monsterName))
            {
                monsterName = record.ReadRequiredString("monster_name");
            }

            var runtimeKind = SkillRuntimeKind.Passive;
            if (skillKind == PakuriCsvSkillKind.Active)
            {
                runtimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind");
            }

            return new SkillRow
            {
                Name = record.ReadRequiredString("skill_name"),
                MonsterName = monsterName,
                SkillKind = skillKind,
                Slot = slot,
                DisplayName = record.ReadRequiredString("display_name"),
                RuntimeKind = runtimeKind,
                ImplementationState = ReadOptionalEnum(record, "implementation_state", SkillImplementationState.RuntimeImplemented),
                IsDefaultLearned = ReadOptionalBool(record, "is_default_learned", slot == SkillSlot.A),
                IsAvailableWithoutActiveRequirement = ReadOptionalBool(record, "is_available_without_active_requirement", slot == SkillSlot.F),
                RequiredActiveSlot = ReadOptionalEnum(record, "required_active_slot", GetRequiredActiveSlot(slot)),
                SkillIconPath = ReadSkillIconPath(record),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                RuntimeVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_visual_sprite_path"),
                RuntimeVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_visual_animator_controller_path"),
                RuntimeVisualScale = ReadOptionalFloat(record, "runtime_visual_scale", 1f),
                RuntimeVisualScaleX = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_x"),
                RuntimeVisualScaleY = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_y"),
                RuntimeVisualScaleZ = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_z"),
                RuntimeVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_visual_sorting_order"),
                RuntimeVisualAnchor = ReadOptionalStringIfColumnExists(record, "runtime_visual_anchor"),
                RuntimeHitboxSizeX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_x"),
                RuntimeHitboxSizeY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_y"),
                RuntimeImpactVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_impact_visual_sprite_path"),
                RuntimeImpactVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_impact_visual_animator_controller_path"),
                RuntimeImpactVisualScale = ReadOptionalFloat(record, "runtime_impact_visual_scale", 1f),
                RuntimeImpactVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_impact_visual_sorting_order"),
                DescriptionText = ReadOptionalStringIfColumnExists(record, "description_text"),
                Summary = ReadOptionalStringIfColumnExists(record, "summary"),
                Attribute = ReadOptionalEnum(record, "attribute", DamageAttribute.Physical),
                BaseDamage = ReadOptionalFloatIfColumnExists(record, "base_damage"),
                AttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "attack_power_coefficient"),
                SpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "spell_power_coefficient"),
                Radius = ReadOptionalFloatIfColumnExists(record, "radius"),
                LineLength = ReadOptionalFloatIfColumnExists(record, "line_length"),
                CastRepeatCount = Math.Max(1, ReadOptionalIntIfColumnExists(record, "cast_repeat_count")),
                CastRepeatIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "cast_repeat_interval_seconds"),
                KnockbackDistance = ReadOptionalFloatIfColumnExists(record, "knockback_distance"),
                DamageDelaySeconds = ReadOptionalFloatIfColumnExists(record, "damage_delay_seconds"),
                ExecuteHealthRatioThreshold = ReadOptionalFloatIfColumnExists(record, "execute_health_ratio_threshold"),
                RequireExecuteThresholdToCast = ReadOptionalBoolIfColumnExists(record, "require_execute_threshold_to_cast"),
                ExecuteDamageMultiplier = ReadOptionalFloat(record, "execute_damage_multiplier", 1f),
                KillCooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "kill_cooldown_refund_ratio"),
                BossDamageMultiplier = ReadOptionalFloat(record, "boss_damage_multiplier", 1f),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                UsePrefabHitbox = ReadOptionalBoolIfColumnExists(record, "use_prefab_hitbox"),
                TargetSelection = ReadOptionalStringIfColumnExists(record, "target_selection"),
                TargetSelectionStatusName = ReadOptionalStringIfColumnExists(record, "target_selection_status_name"),
                TargetSelectionStatusMinStacks = ReadOptionalIntIfColumnExists(record, "target_selection_status_min_stacks"),
                CooldownSeconds = ReadOptionalFloatIfColumnExists(record, "cooldown_seconds"),
                ActiveDurationSeconds = ReadOptionalFloatIfColumnExists(record, "active_duration_seconds"),
                MagazineCapacity = ReadOptionalIntIfColumnExists(record, "magazine_capacity"),
                ReloadSeconds = ReadOptionalFloatIfColumnExists(record, "reload_seconds"),
                ShotIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "shot_interval_seconds"),
                BurstIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "burst_interval_seconds"),
                ProjectileBurstCount = ReadOptionalIntIfColumnExists(record, "projectile_burst_count"),
                BurstDamageProjectileIndex = ReadOptionalIntIfColumnExists(record, "burst_damage_projectile_index"),
                BurstDamageMultiplier = ReadOptionalFloat(record, "burst_damage_multiplier", 1f),
                ProjectileSpeed = ReadOptionalFloatIfColumnExists(record, "projectile_speed"),
                PierceCount = ReadOptionalIntIfColumnExists(record, "pierce_count"),
                CriticalAllowed = ReadOptionalBoolIfColumnExists(record, "critical_allowed"),
                DeploymentRequiredTargetStatusName = ReadOptionalStringIfColumnExists(record, "deployment_required_target_status_name"),
                DeploymentRequiredTargetStatusMinStacks = ReadOptionalIntIfColumnExists(record, "deployment_required_target_status_min_stacks"),
                TargetStatusStackStatusName = ReadOptionalStringIfColumnExists(record, "target_status_stack_status_name"),
                TargetStatusStackMaxStacks = ReadOptionalIntIfColumnExists(record, "target_status_stack_max_stacks"),
                TargetStatusStackBaseDamage = ReadOptionalFloatIfColumnExists(record, "target_status_stack_base_damage"),
                TargetStatusStackAttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_attack_power_coefficient"),
                TargetStatusStackSpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_spell_power_coefficient"),
                ConsumeTargetStatusName = ReadOptionalStringIfColumnExists(record, "consume_target_status_name"),
                ConsumeTargetStatusRatio = ReadOptionalFloatIfColumnExists(record, "consume_target_status_ratio"),
                ConsumeTargetStatusStacks = ReadOptionalIntIfColumnExists(record, "consume_target_status_stacks"),
                Status = ReadStatusPayload(record, false, true)
            };
        }

        internal static SkillChoiceRow ParseSkillChoiceRow(
            CsvRecord record,
            SkillChoiceGroup? implicitChoiceGroup = null)
        {
            return new SkillChoiceRow
            {
                Name = record.ReadRequiredString("choice_name"),
                MonsterName = record.ReadRequiredString("monster_name"),
                SkillName = record.ReadRequiredString("skill_name"),
                TargetSkillName = ReadOptionalStringIfColumnExists(record, "target_skill_name"),
                ChoiceGroup = implicitChoiceGroup
                    ?? record.ReadEnum<SkillChoiceGroup>("choice_group"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                DescriptionText = ReadOptionalStringIfColumnExists(record, "description_text"),
                SkillIconPath = ReadOptionalStringIfColumnExists(record, "skill_icon_path")
            };
        }

        internal static SkillTriggerRow ParseSkillTriggerRow(CsvRecord record)
        {
            var row = new SkillTriggerRow
            {
                Name = record.ReadRequiredString("trigger_name"),
                MonsterName = record.ReadRequiredString("monster_name"),
                SourceSkillName = record.ReadRequiredString("source_skill_name"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                RequiresActiveChoiceName = ReadOptionalStringIfColumnExists(record, "requires_active_choice_name"),
                ExcludesActiveChoiceName = ReadOptionalStringIfColumnExists(record, "excludes_active_choice_name"),
                RequiredSourceStatusName = ReadOptionalStringIfColumnExists(record, "required_source_status_name"),
                RequiredSourceStatusMinStacks = ReadOptionalIntIfColumnExists(record, "required_source_status_min_stacks"),
                ConditionStatusName = ReadOptionalStringIfColumnExists(record, "condition_status_name"),
                ConditionStatusSourceSkillName = ReadOptionalStringIfColumnExists(record, "condition_status_source_skill_name"),
                TriggerAttribute = ReadOptionalStringIfColumnExists(record, "trigger_attribute"),
                EventSkillName = ReadOptionalStringIfColumnExists(record, "event_skill_name"),
                EventSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "event_skill_runtime_kinds"),
                SortOrder = record.ReadInt("sort_order"),
                RepeatCount = ReadOptionalIntIfColumnExists(record, "repeat_count"),
                RepeatIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "repeat_interval_seconds"),
                TriggerDelaySeconds = ReadOptionalFloatIfColumnExists(record, "trigger_delay_seconds"),
                TriggerEveryCount = ReadOptionalIntIfColumnExists(record, "trigger_every_count"),
                EventSourceScope = ReadOptionalStringIfColumnExists(record, "event_source_scope"),
                RequireEventExecute = ReadOptionalBoolIfColumnExists(record, "require_event_execute")
            };

            if (TryReadFloatIfColumnExists(record, "proc_chance", out var procChance))
            {
                row.ProcChance = procChance;
            }

            if (TryReadFloatIfColumnExists(record, "internal_cooldown_seconds", out var internalCooldownSeconds))
            {
                row.InternalCooldownSeconds = internalCooldownSeconds;
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

        internal static SkillSlot GetRequiredActiveSlot(SkillSlot passiveSlot)
        {
            switch (passiveSlot)
            {
                case SkillSlot.G:
                    return SkillSlot.B;
                case SkillSlot.H:
                    return SkillSlot.C;
                case SkillSlot.I:
                    return SkillSlot.D;
                case SkillSlot.J:
                    return SkillSlot.E;
                default:
                    return SkillSlot.A;
            }
        }

        internal static int ReadOptionalIntIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadInt(columnName) : 0;
        }

        internal static float ReadOptionalFloatIfColumnExists(CsvRecord record, string columnName)
        {
            return ReadOptionalFloat(record, columnName, 0f);
        }

        internal static float ReadOptionalFloat(CsvRecord record, string columnName, float defaultValue)
        {
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out var value)
                ? value
                : defaultValue;
        }

        internal static string ReadOptionalStringIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadString(columnName) : string.Empty;
        }

        internal static string ReadSkillIconPath(CsvRecord record)
        {
            var path = ReadOptionalStringIfColumnExists(record, "SkillIconImage");
            return string.IsNullOrWhiteSpace(path)
                ? ReadOptionalStringIfColumnExists(record, "skill_icon_path")
                : path;
        }

        internal static bool ReadOptionalBoolIfColumnExists(CsvRecord record, string columnName)
        {
            return ReadOptionalBool(record, columnName, false);
        }

        internal static bool ReadOptionalBool(CsvRecord record, string columnName, bool defaultValue)
        {
            return !record.HasColumn(columnName) || string.IsNullOrWhiteSpace(record.ReadString(columnName))
                ? defaultValue
                : record.ReadBool(columnName);
        }

        internal static T ReadOptionalEnum<T>(CsvRecord record, string columnName, T defaultValue) where T : struct
        {
            return !record.HasColumn(columnName) || string.IsNullOrWhiteSpace(record.ReadString(columnName))
                ? defaultValue
                : record.ReadEnum<T>(columnName);
        }

        internal static bool TryReadFloatIfColumnExists(CsvRecord record, string columnName, out float value)
        {
            value = 0f;
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out value);
        }

        internal static bool TryReadFloat(CsvRecord record, string columnName, out float value)
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

        /// EnemyRow에 해당하는 CSV 한 행을 표현한다.
        internal class EnemyRow
        {
            public string Name;
            public string StageName;
            public int SortOrder;
            public string DisplayName;
            public DamageAttribute Attribute;
            public float MaxHealth;
            public float AttackPower;
            public float SpellPower;
            public float MoveSpeed;
            public float CriticalChance;
            public float CriticalDamage;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
            public string SkillSlotAName;
            public string SkillSlotBName;
            public string PassiveName;
            public float NexusDamage;
            public string ImagePath;
        }

        /// EnemyBaseSkillRow에 해당하는 CSV 한 행을 표현한다.
        internal class EnemyBaseSkillRow
        {
            public SkillRow Skill;
            public string ExecutionProfile;
            public string TargetScope;
            public string TargetSelection;
            public float CastRange;
            public float EffectRadius;
            public float ProjectileLifetime;
            public float FlatValue;
            public float IncomingDamageMultiplier = 1f;
            public float MoveSpeedMultiplier = 1f;
            public float OutgoingDamageMultiplier = 1f;
            public float ChainDamageMultiplier;
            public float ChainDelaySeconds;
            public float ChainRadius;
            public bool ExcludePrimaryTarget;
            public float StatusActionSpeedBonus;
            public float StatusDurationSeconds;
            public float TargetMaxHealthRatio;
            public string HitTargetCount;
            public float ChargeRampSeconds = 3f;
            public float ChargeMoveSpeedMultiplier = 2.5f;
            public PassiveModifierKind PassiveModifierKind;
            public bool PassiveHasAttribute;
            public DamageAttribute PassiveAttribute;
            public float PassiveModifierValue;
        }

        /// EnemyTriggerRow에 해당하는 CSV 한 행을 표현한다.
        internal class EnemyTriggerRow
        {
            public string Name;
            public string SourceSkillName;
            public SkillTriggerEvent TriggerEvent;
            public string TriggeredSkillName;
            public int SortOrder;
            public bool Enabled;
        }

        internal static EnemyRow ParseEnemyRow(CsvRecord record)
        {
            return new EnemyRow
            {
                Name = record.ReadRequiredString("enemy_name"),
                StageName = record.ReadRequiredString("stage_name"),
                SortOrder = record.ReadInt("sort_order"),
                DisplayName = record.ReadRequiredString("display_name"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                MaxHealth = record.ReadFloat("max_health"),
                AttackPower = record.ReadFloat("attack_power"),
                SpellPower = record.ReadFloat("spell_power"),
                MoveSpeed = record.ReadFloat("move_speed"),
                CriticalChance = record.ReadFloat("crit_chance"),
                CriticalDamage = record.ReadFloat("crit_damage"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy"),
                SkillSlotAName = record.ReadRequiredString("skill_slot_a_name"),
                SkillSlotBName = record.ReadRequiredString("skill_slot_b_name"),
                PassiveName = record.ReadRequiredString("passive_name"),
                NexusDamage = record.ReadFloat("nexus_damage"),
                ImagePath = ReadOptionalStringIfColumnExists(record, "Image")
            };
        }

        internal static EnemyBaseSkillRow ParseEnemyBaseSkillRow(CsvRecord record, string tableName)
        {
            if (string.Equals(tableName, "skills_passive.csv", StringComparison.OrdinalIgnoreCase))
            {
                return ParseEnemyPassiveSkillRow(record);
            }

            var row = new EnemyBaseSkillRow
            {
                Skill = ParseSkillRow(
                    record,
                    record.ReadEnum<PakuriCsvSkillKind>("skill_kind"),
                    "enemy-shared"),
                ExecutionProfile = ReadOptionalStringIfColumnExists(record, "execution_profile"),
                TargetScope = ReadOptionalStringIfColumnExists(record, "target_scope"),
                TargetSelection = ReadOptionalStringIfColumnExists(record, "target_selection"),
                CastRange = ReadOptionalFloatIfColumnExists(record, "cast_range"),
                EffectRadius = ReadOptionalFloatIfColumnExists(record, "effect_radius"),
                ProjectileLifetime = ReadOptionalFloatIfColumnExists(record, "projectile_lifetime"),
                FlatValue = ReadOptionalFloatIfColumnExists(record, "flat_value"),
                IncomingDamageMultiplier = ReadOptionalFloat(record, "incoming_damage_multiplier", 1f),
                MoveSpeedMultiplier = ReadOptionalFloat(record, "move_speed_multiplier", 1f),
                OutgoingDamageMultiplier = ReadOptionalFloat(record, "outgoing_damage_multiplier", 1f),
                ChainDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "chain_damage_multiplier"),
                ChainDelaySeconds = ReadOptionalFloatIfColumnExists(record, "chain_delay_seconds"),
                ChainRadius = ReadOptionalFloatIfColumnExists(record, "chain_radius"),
                ExcludePrimaryTarget = ReadOptionalBoolIfColumnExists(record, "exclude_primary_target"),
                StatusActionSpeedBonus = ReadOptionalFloatIfColumnExists(record, "status_action_speed_bonus"),
                StatusDurationSeconds = ReadOptionalFloatIfColumnExists(record, "status_duration_seconds"),
                TargetMaxHealthRatio = ReadOptionalFloatIfColumnExists(record, "target_max_health_ratio"),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                ChargeRampSeconds = ReadOptionalFloat(record, "charge_ramp_seconds", 3f),
                ChargeMoveSpeedMultiplier = ReadOptionalFloat(record, "charge_move_speed_multiplier", 2.5f)
            };

            if (row.Skill.SkillKind == PakuriCsvSkillKind.Passive
                || row.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' contains passive skill '{row.Skill.Name}'. Enemy passive rows must be authored in 'skills_passive.csv'.");
            }

            return row;
        }

        internal static EnemyBaseSkillRow ParseEnemyPassiveSkillRow(CsvRecord record)
        {
            var modifierKind = record.ReadEnum<PassiveModifierKind>("modifier_kind");
            var hasAttribute = TryReadDamageAttribute(record, "attribute", out var attribute);
            return new EnemyBaseSkillRow
            {
                Skill = new SkillRow
                {
                    Name = record.ReadRequiredString("skill_name"),
                    MonsterName = "enemy-shared",
                    SkillKind = PakuriCsvSkillKind.Passive,
                    Slot = SkillSlot.F,
                    DisplayName = record.ReadRequiredString("display_name"),
                    RuntimeKind = SkillRuntimeKind.Passive,
                    ImplementationState = SkillImplementationState.RuntimeImplemented,
                    IsAvailableWithoutActiveRequirement = true
                },
                PassiveModifierKind = modifierKind,
                PassiveHasAttribute = hasAttribute,
                PassiveAttribute = attribute,
                PassiveModifierValue = record.ReadFloat("modifier_value")
            };
        }

        internal static EnemyTriggerRow ParseEnemyTriggerRow(CsvRecord record)
        {
            return new EnemyTriggerRow
            {
                Name = record.ReadRequiredString("trigger_name"),
                SourceSkillName = record.ReadRequiredString("source_skill_name"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                TriggeredSkillName = record.ReadRequiredString("triggered_skill_name"),
                SortOrder = record.ReadInt("sort_order"),
                Enabled = record.ReadBool("enabled")
            };
        }

    }
}
