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

        /// RewardChoiceRow에 해당하는 CSV 한 행을 표현한다.
        internal class RewardChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public int SortOrder;
        }

        /// SkillRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillRow
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
            public string TargetSelectionStatusId;
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
            public string DeploymentRequiredTargetStatusId;
            public int DeploymentRequiredTargetStatusMinStacks;
            public string TargetStatusStackStatusId;
            public int TargetStatusStackMaxStacks;
            public float TargetStatusStackBaseDamage;
            public float TargetStatusStackAttackPowerCoefficient;
            public float TargetStatusStackSpellPowerCoefficient;
            public string ConsumeTargetStatusId;
            public float ConsumeTargetStatusRatio;
            public int ConsumeTargetStatusStacks;
            public StatusPayloadRow Status = new StatusPayloadRow();
        }

        /// SkillChoiceRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string SkillId;
            public string TargetSkillId;
            public SkillChoiceGroup ChoiceGroup;
            public int SortOrder;
            public string Title;
            public string DescriptionText;
            public string SkillIconPath;
        }

        /// SkillTriggerRow에 해당하는 CSV 한 행을 표현한다.
        internal class SkillTriggerRow
        {
            public string Id;
            public string MonsterId;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string RequiresActiveChoiceId;
            public string ExcludesActiveChoiceId;
            public string RequiredSourceStatusId;
            public int RequiredSourceStatusMinStacks;
            public string ConditionStatusId;
            public string ConditionStatusSourceSkillId;
            public string TriggerAttribute;
            public string EventSkillId;
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

        /// 전달된 record 값을 사용해 MonsterRow 값을 런타임 표현으로 파싱한다.
        internal static MonsterRow ParseMonsterRow(CsvRecord record)
        {
            return new MonsterRow
            {
                Id = record.ReadRequiredString("id"),
                DisplayName = record.ReadRequiredString("display_name"),
                RoleSummary = record.ReadString("role_summary"),
                ElementLabel = record.ReadString("element_label"),
                PrimaryAttribute = record.ReadEnum<DamageAttribute>("primary_attribute"),
                ActiveSkillName = ReadOptionalStringIfColumnExists(record, "active_skill_name"),
                PassiveSkillName = ReadOptionalStringIfColumnExists(record, "passive_skill_name"),
                MonsterIconImagePath = ReadOptionalStringIfColumnExists(record, "MonsterIconImage"),
                MaxHealth = record.ReadFloat("max_health"),
                PowerStat = record.ReadFloat("power_stat"),
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

        /// 전달된 record 값을 사용해 RewardChoiceRow 값을 런타임 표현으로 파싱한다.
        internal static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record)
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

        /// 전달된 런타임 입력값을 사용해 SkillRow 값을 런타임 표현으로 파싱한다.
        internal static SkillRow ParseSkillRow(
            CsvRecord record,
            PakuriCsvSkillKind skillKind,
            string ownerIdOverride = null)
        {
            var slot = record.ReadEnum<SkillSlot>("slot");
            var monsterId = ownerIdOverride;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                monsterId = record.ReadRequiredString("monster_id");
            }

            var runtimeKind = SkillRuntimeKind.Passive;
            if (skillKind == PakuriCsvSkillKind.Active)
            {
                runtimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind");
            }

            return new SkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                MonsterId = monsterId,
                SkillKind = skillKind,
                Slot = slot,
                DisplayName = record.ReadRequiredString("display_name"),
                RuntimeKind = runtimeKind,
                ImplementationState = ReadOptionalEnum(record, "implementation_state", SkillImplementationState.RuntimeImplemented),
                IsDefaultLearned = ReadOptionalBool(record, "is_default_learned", slot == SkillSlot.A),
                IsAvailableWithoutActiveRequirement = ReadOptionalBool(record, "is_available_without_active_requirement", slot == SkillSlot.F),
                RequiredActiveSlot = ReadOptionalEnum(record, "required_active_slot", GetRequiredActiveSlot(slot)),
                SkillIconPath = ReadOptionalStringIfColumnExists(record, "skill_icon_path"),
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
                TargetSelectionStatusId = ReadOptionalStringIfColumnExists(record, "target_selection_status_id"),
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
                DeploymentRequiredTargetStatusId = ReadOptionalStringIfColumnExists(record, "deployment_required_target_status_id"),
                DeploymentRequiredTargetStatusMinStacks = ReadOptionalIntIfColumnExists(record, "deployment_required_target_status_min_stacks"),
                TargetStatusStackStatusId = ReadOptionalStringIfColumnExists(record, "target_status_stack_status_id"),
                TargetStatusStackMaxStacks = ReadOptionalIntIfColumnExists(record, "target_status_stack_max_stacks"),
                TargetStatusStackBaseDamage = ReadOptionalFloatIfColumnExists(record, "target_status_stack_base_damage"),
                TargetStatusStackAttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_attack_power_coefficient"),
                TargetStatusStackSpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_spell_power_coefficient"),
                ConsumeTargetStatusId = ReadOptionalStringIfColumnExists(record, "consume_target_status_id"),
                ConsumeTargetStatusRatio = ReadOptionalFloatIfColumnExists(record, "consume_target_status_ratio"),
                ConsumeTargetStatusStacks = ReadOptionalIntIfColumnExists(record, "consume_target_status_stacks"),
                Status = ReadStatusPayload(record, false, true)
            };
        }

        /// 전달된 record 값을 사용해 SkillChoiceRow 값을 런타임 표현으로 파싱한다.
        internal static SkillChoiceRow ParseSkillChoiceRow(CsvRecord record)
        {
            return new SkillChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SkillId = record.ReadRequiredString("skill_id"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                ChoiceGroup = record.ReadEnum<SkillChoiceGroup>("choice_group"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                DescriptionText = ReadOptionalStringIfColumnExists(record, "description_text"),
                SkillIconPath = ReadOptionalStringIfColumnExists(record, "skill_icon_path")
            };
        }

        /// 전달된 record 값을 사용해 SkillTriggerRow 값을 런타임 표현으로 파싱한다.
        internal static SkillTriggerRow ParseSkillTriggerRow(CsvRecord record)
        {
            var row = new SkillTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                RequiresActiveChoiceId = ReadOptionalStringIfColumnExists(record, "requires_active_choice_id"),
                ExcludesActiveChoiceId = ReadOptionalStringIfColumnExists(record, "excludes_active_choice_id"),
                RequiredSourceStatusId = ReadOptionalStringIfColumnExists(record, "required_source_status_id"),
                RequiredSourceStatusMinStacks = ReadOptionalIntIfColumnExists(record, "required_source_status_min_stacks"),
                ConditionStatusId = ReadOptionalStringIfColumnExists(record, "condition_status_id"),
                ConditionStatusSourceSkillId = ReadOptionalStringIfColumnExists(record, "condition_status_source_skill_id"),
                TriggerAttribute = ReadOptionalStringIfColumnExists(record, "trigger_attribute"),
                EventSkillId = ReadOptionalStringIfColumnExists(record, "event_skill_id"),
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

        /// 전달된 passiveSlot 값을 사용해 RequiredActiveSlot를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 OptionalIntIfColumnExists를 읽는다.
        internal static int ReadOptionalIntIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadInt(columnName) : 0;
        }

        /// 전달된 런타임 입력값을 사용해 OptionalFloatIfColumnExists를 읽는다.
        internal static float ReadOptionalFloatIfColumnExists(CsvRecord record, string columnName)
        {
            return ReadOptionalFloat(record, columnName, 0f);
        }

        /// 전달된 런타임 입력값을 사용해 OptionalFloat를 읽는다.
        internal static float ReadOptionalFloat(CsvRecord record, string columnName, float defaultValue)
        {
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out var value)
                ? value
                : defaultValue;
        }

        /// 전달된 런타임 입력값을 사용해 OptionalStringIfColumnExists를 읽는다.
        internal static string ReadOptionalStringIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadString(columnName) : string.Empty;
        }

        /// 전달된 런타임 입력값을 사용해 OptionalBoolIfColumnExists를 읽는다.
        internal static bool ReadOptionalBoolIfColumnExists(CsvRecord record, string columnName)
        {
            return ReadOptionalBool(record, columnName, false);
        }

        /// 전달된 런타임 입력값을 사용해 OptionalBool를 읽는다.
        internal static bool ReadOptionalBool(CsvRecord record, string columnName, bool defaultValue)
        {
            return !record.HasColumn(columnName) || string.IsNullOrWhiteSpace(record.ReadString(columnName))
                ? defaultValue
                : record.ReadBool(columnName);
        }

        /// 전달된 런타임 입력값을 사용해 OptionalEnum를 읽는다.
        internal static T ReadOptionalEnum<T>(CsvRecord record, string columnName, T defaultValue) where T : struct
        {
            return !record.HasColumn(columnName) || string.IsNullOrWhiteSpace(record.ReadString(columnName))
                ? defaultValue
                : record.ReadEnum<T>(columnName);
        }

        /// 전달된 런타임 입력값을 사용해 ReadFloatIfColumnExists 작업을 시도하고 성공 여부를 반환한다.
        internal static bool TryReadFloatIfColumnExists(CsvRecord record, string columnName, out float value)
        {
            value = 0f;
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out value);
        }

        /// 전달된 런타임 입력값을 사용해 ReadFloat 작업을 시도하고 성공 여부를 반환한다.
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
            public string Id;
            public string StageId;
            public int SortOrder;
            public string DisplayName;
            public DamageAttribute Attribute;
            public float MaxHealth;
            public float AttackPower;
            public float SpellPower;
            public float MoveSpeed;
            public float CriticalChance;
            public float CriticalDamage;
            public float CriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
            public string SkillSlotAId;
            public string SkillSlotBId;
            public string PassiveId;
            public float NexusDamage;
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
            public string Id;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string TriggeredSkillId;
            public int SortOrder;
            public bool Enabled;
        }

        /// 전달된 record 값을 사용해 EnemyRow 값을 런타임 표현으로 파싱한다.
        internal static EnemyRow ParseEnemyRow(CsvRecord record)
        {
            return new EnemyRow
            {
                Id = record.ReadRequiredString("enemy_id"),
                StageId = record.ReadRequiredString("stage_id"),
                SortOrder = record.ReadInt("sort_order"),
                DisplayName = record.ReadRequiredString("display_name"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                MaxHealth = record.ReadFloat("max_health"),
                AttackPower = record.ReadFloat("attack_power"),
                SpellPower = record.ReadFloat("spell_power"),
                MoveSpeed = record.ReadFloat("move_speed"),
                CriticalChance = record.ReadFloat("crit_chance"),
                CriticalDamage = record.ReadFloat("crit_damage"),
                CriticalResistance = record.ReadFloat("crit_resistance"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy"),
                SkillSlotAId = record.ReadRequiredString("skill_slot_a_id"),
                SkillSlotBId = record.ReadRequiredString("skill_slot_b_id"),
                PassiveId = record.ReadRequiredString("passive_id"),
                NexusDamage = record.ReadFloat("nexus_damage")
            };
        }

        /// 전달된 런타임 입력값을 사용해 EnemyBaseSkillRow 값을 런타임 표현으로 파싱한다.
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
                    $"CSV table '{tableName}' contains passive skill '{row.Skill.Id}'. Enemy passive rows must be authored in 'skills_passive.csv'.");
            }

            return row;
        }

        /// 전달된 record 값을 사용해 EnemyPassiveSkillRow 값을 런타임 표현으로 파싱한다.
        internal static EnemyBaseSkillRow ParseEnemyPassiveSkillRow(CsvRecord record)
        {
            var modifierKind = record.ReadEnum<PassiveModifierKind>("modifier_kind");
            var hasAttribute = TryReadDamageAttribute(record, "attribute", out var attribute);
            return new EnemyBaseSkillRow
            {
                Skill = new SkillRow
                {
                    Id = record.ReadRequiredString("skill_id"),
                    MonsterId = "enemy-shared",
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

        /// 전달된 record 값을 사용해 EnemyTriggerRow 값을 런타임 표현으로 파싱한다.
        internal static EnemyTriggerRow ParseEnemyTriggerRow(CsvRecord record)
        {
            return new EnemyTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
                SortOrder = record.ReadInt("sort_order"),
                Enabled = record.ReadBool("enabled")
            };
        }

    }
}
