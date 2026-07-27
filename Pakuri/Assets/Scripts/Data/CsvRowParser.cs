using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;


/*
 * authoring CSV 행을 런타임 카탈로그 생성 전의 SourceModel 행 데이터로 변환한다.
 * 몬스터, 보상, 스킬, Choice, Trigger, 적 스킬 행을 명시된 CSV 종류에 맞춰 읽는다.
 */
namespace Pakuri.Data
{
    internal static class CsvRowParser
    {
        /*
         * 플레이어 몬스터 CSV 한 행의 능력치와 표시 정보를 보관한다.
         */
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

        /*
         * 몬스터 초기 보상 선택지와 연결 스킬 ID를 보관한다.
         */
        internal class RewardChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public int SortOrder;
        }

        /*
         * 액티브·패시브 스킬 CSV 한 행의 실행 값을 보관한다.
         */
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

        /*
         * 스킬 성장 선택지 CSV 한 행의 변경값과 조건을 보관한다.
         */
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

        /*
         * 전투 사건에 연결된 스킬 Trigger 한 행을 보관한다.
         */
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
            public SkillTriggerActionKind TriggerAction;
            public string EventSkillId;
            public string EventSkillRuntimeKinds;
            public float ProcChance = 1f;
            public float InternalCooldownSeconds;
            public float TriggerDelaySeconds;
            public int TriggerEveryCount;
            public string EventSourceScope;
            public string TriggeredSkillId;
            public string TargetSkillId;
            public string TriggeredEffectId;
            public SkillNodeOwnerKind TriggeredGraphOwnerKind;
            public string TriggeredGraphOwnerId;
            public SkillGraphKind TriggeredGraphKind;
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
            public string RuntimeVisualSpritePath;
            public string RuntimeVisualAnimatorControllerPath;
            public float RuntimeVisualScale = 1f;
            public int RuntimeVisualSortingOrder;
            public string RuntimeVisualAnchor;
            public float RuntimeHitboxSizeX;
            public float RuntimeHitboxSizeY;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static MonsterRow ParseMonsterRow(CsvRecord record /* 읽을 CSV 행 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record /* 읽을 CSV 행 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillRow ParseSkillRow(
            CsvRecord record /* 읽을 CSV 행 */,
            PakuriCsvSkillKind skillKind /* 스킬 종류 */,
            string ownerIdOverride = null /* 소유자 식별자 덮어쓸 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillChoiceRow ParseSkillChoiceRow(CsvRecord record /* 읽을 CSV 행 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillTriggerRow ParseSkillTriggerRow(CsvRecord record /* 읽을 CSV 행 */)
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
                TriggerAction = ReadOptionalEnum(record, "trigger_action", SkillTriggerActionKind.Auto),
                EventSkillId = ReadOptionalStringIfColumnExists(record, "event_skill_id"),
                EventSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "event_skill_runtime_kinds"),
                TriggeredSkillId = ReadOptionalStringIfColumnExists(record, "triggered_skill_id"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                TriggeredEffectId = ReadOptionalStringIfColumnExists(record, "triggered_effect_id"),
                TriggeredGraphOwnerKind = ReadOptionalEnum(
                    record,
                    "triggered_graph_owner_kind",
                    SkillNodeOwnerKind.Skill),
                TriggeredGraphOwnerId = ReadOptionalStringIfColumnExists(record, "triggered_graph_owner_id"),
                TriggeredGraphKind = ReadOptionalEnum(
                    record,
                    "triggered_graph_kind",
                    SkillGraphKind.Effect),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                TargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("target_side"),
                TargetSelection = record.ReadEnum<SkillMultiEffectTargetSelection>("target_selection"),
                TargetShape = record.ReadEnum<SkillMultiEffectTargetShape>("target_shape"),
                CenterMode = record.ReadEnum<SkillMultiEffectCenterMode>("center_mode"),
                Attribute = ReadOptionalEnum(record, "attribute", DamageAttribute.Physical),
                BaseDamage = ReadOptionalFloatIfColumnExists(record, "base_damage"),
                AttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "attack_power_coefficient"),
                SpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "spell_power_coefficient"),
                DamageMultiplier = ReadOptionalFloatIfColumnExists(record, "damage_multiplier"),
                DamageSource = record.ReadEnum<SkillTriggerDamageSource>("damage_source"),
                DamageSourceMultiplier = ReadOptionalFloatIfColumnExists(record, "damage_source_multiplier"),
                TrackedAttribute = record.ReadEnum<DamageAttribute>("tracked_attribute"),
                Radius = ReadOptionalFloatIfColumnExists(record, "radius"),
                CoverAll = ReadOptionalBoolIfColumnExists(record, "cover_all"),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                RepeatCount = ReadOptionalIntIfColumnExists(record, "repeat_count"),
                RepeatIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "repeat_interval_seconds"),
                TriggerDelaySeconds = ReadOptionalFloatIfColumnExists(record, "trigger_delay_seconds"),
                TriggerEveryCount = ReadOptionalIntIfColumnExists(record, "trigger_every_count"),
                EventSourceScope = ReadOptionalStringIfColumnExists(record, "event_source_scope"),
                RequireEventExecute = ReadOptionalBoolIfColumnExists(record, "require_event_execute"),
                CooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "cooldown_refund_ratio"),
                ReloadReduceRatio = ReadOptionalFloatIfColumnExists(record, "reload_reduce_ratio"),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                RuntimeVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_visual_sprite_path"),
                RuntimeVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_visual_animator_controller_path"),
                RuntimeVisualScale = ReadOptionalFloat(record, "runtime_visual_scale", 1f),
                RuntimeVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_visual_sorting_order"),
                RuntimeVisualAnchor = ReadOptionalStringIfColumnExists(record, "runtime_visual_anchor"),
                RuntimeHitboxSizeX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_x"),
                RuntimeHitboxSizeY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_y"),
                RuntimeSupportState = ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
                RuntimeSupportNotes = ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
            };

            if (TryReadFloatIfColumnExists(record, "proc_chance", out var procChance))
            {
                row.ProcChance = procChance;
            }

            if (TryReadFloatIfColumnExists(record, "internal_cooldown_seconds", out var internalCooldownSeconds))
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

        /*
         * 패시브 슬롯과 연결되는 액티브 슬롯을 반환한다.
         */
        internal static SkillSlot GetRequiredActiveSlot(SkillSlot passiveSlot /* 패시브 슬롯 */)
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

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloat(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (TryReadFloat(record, columnName, out var value))
            {
                return value;
            }

            return 0f;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static int ReadOptionalInt(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (TryReadInt(record, columnName, out var value))
            {
                return value;
            }

            return 0;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static int ReadOptionalIntIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (record.HasColumn(columnName))
            {
                return ReadOptionalInt(record, columnName);
            }

            return 0;
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadIntIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, out int value /* 처리할 값 */)
        {
            value = 0;
            return record.HasColumn(columnName) && TryReadInt(record, columnName, out value);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloatIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (record.HasColumn(columnName))
            {
                return ReadOptionalFloat(record, columnName);
            }

            return 0f;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloat(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, float defaultValue /* 값이 없을 때 사용할 기본값 */)
        {
            if (record.HasColumn(columnName) && TryReadFloat(record, columnName, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static string ReadOptionalStringIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (record.HasColumn(columnName))
            {
                return record.ReadString(columnName);
            }

            return string.Empty;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static bool ReadOptionalBoolIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */)
        {
            if (!record.HasColumn(columnName))
            {
                return false;
            }

            var raw = record.ReadString(columnName);
            return !string.IsNullOrWhiteSpace(raw) && record.ReadBool(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static bool ReadOptionalBool(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, bool defaultValue /* 값이 없을 때 사용할 기본값 */)
        {
            if (!record.HasColumn(columnName))
            {
                return defaultValue;
            }

            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            return record.ReadBool(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static T ReadOptionalEnum<T>(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, T defaultValue /* 값이 없을 때 사용할 기본값 */) where T : struct
        {
            if (!record.HasColumn(columnName))
            {
                return defaultValue;
            }

            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            return record.ReadEnum<T>(columnName);
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadFloatIfColumnExists(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, out float value /* 처리할 값 */)
        {
            value = 0f;
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out value);
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadFloat(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, out float value /* 처리할 값 */)
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

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadInt(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, out int value /* 처리할 값 */)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateExpectedSlots(
            string monsterId /* 몬스터 식별자 */,
            HashSet<SkillSlot> slots /* 슬롯 목록 */,
            SkillSlot first /* 첫 번째 */,
            SkillSlot last /* 마지막 */,
            string kindLabel /* 종류 표시 문구 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            for (var slot = first; slot <= last; slot++)
            {
                if (!slots.Contains(slot))
                {
                    errors.Add($"Monster '{monsterId}' is missing {kindLabel} slot '{slot}'.");
                }
            }
        }

        /*
         * 적 CSV 한 행의 기본 능력치와 장착 스킬 ID를 보관한다.
         */
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

        /*
         * 적 스킬 CSV 한 행의 실행 방식과 전투 값을 보관한다.
         */
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
            public EnemyPassiveModifierKind PassiveModifierKind;
            public bool PassiveHasAttribute;
            public DamageAttribute PassiveAttribute;
            public float PassiveModifierValue;
        }

        /*
         * 적 스킬 Trigger 한 행의 실행 대상과 순서를 보관한다.
         */
        internal class EnemyTriggerRow
        {
            public string Id;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string TriggeredSkillId;
            public SkillRuntimeKind RuntimeKind;
            public int SortOrder;
            public bool Enabled;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyRow ParseEnemyRow(CsvRecord record /* 읽을 CSV 행 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyBaseSkillRow ParseEnemyBaseSkillRow(CsvRecord record /* 읽을 CSV 행 */, string tableName /* CSV 표 이름 */)
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyBaseSkillRow ParseEnemyPassiveSkillRow(CsvRecord record /* 읽을 CSV 행 */)
        {
            var modifierKind = record.ReadEnum<EnemyPassiveModifierKind>("modifier_kind");
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyTriggerRow ParseEnemyTriggerRow(CsvRecord record /* 읽을 CSV 행 */)
        {
            return new EnemyTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                Enabled = record.ReadBool("enabled")
            };
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyRows(SourceModel model /* CSV에서 읽은 원본 데이터 */, List<string> errors /* 검증 오류를 모을 목록 */)
        {
            var referencedActiveSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencedPassiveIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stageSortKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var enemy in model.Enemies.Values)
            {
                if (!string.Equals(enemy.StageId, "stage_one", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(enemy.StageId, "stage_two", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Enemy '{enemy.Id}' has unsupported stage_id '{enemy.StageId}'.");
                }

                if (enemy.SortOrder < 0)
                {
                    errors.Add($"Enemy '{enemy.Id}' has negative sort_order '{enemy.SortOrder}'.");
                }
                else if (!stageSortKeys.Add(enemy.StageId + ":" + enemy.SortOrder))
                {
                    errors.Add($"Enemy stage '{enemy.StageId}' has duplicate sort_order '{enemy.SortOrder}'.");
                }

                if (enemy.NexusDamage <= 0f)
                {
                    errors.Add($"Enemy '{enemy.Id}' requires positive nexus_damage.");
                }

                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotAId, SkillSlot.A, referencedActiveSkillIds, errors);
                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotBId, SkillSlot.B, referencedActiveSkillIds, errors);
                ValidateEnemyPassive(model, enemy, referencedPassiveIds, errors);
            }

            foreach (var baseSkill in model.EnemyBaseSkills.Values)
            {
                if (baseSkill == null || baseSkill.Skill == null)
                {
                    continue;
                }

                if (baseSkill.Skill.SkillKind == PakuriCsvSkillKind.Passive)
                {
                    if (!referencedPassiveIds.Contains(baseSkill.Skill.Id))
                    {
                        errors.Add($"Enemy passive skill '{baseSkill.Skill.Id}' is not referenced by enemies.csv passive_id.");
                    }
                }
                else if (!referencedActiveSkillIds.Contains(baseSkill.Skill.Id))
                {
                    errors.Add($"Enemy base skill '{baseSkill.Skill.Id}' is not referenced by an Enemy A/B skill slot.");
                }
            }

            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(trigger.SourceSkillId, out var sourceSkill)
                    || sourceSkill == null
                    || sourceSkill.Skill == null)
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
                }
                else if (trigger.RuntimeKind != sourceSkill.Skill.RuntimeKind)
                {
                    errors.Add(
                        $"Enemy trigger '{trigger.Id}' runtime_kind '{trigger.RuntimeKind}' does not match source skill kind '{sourceSkill.Skill.RuntimeKind}'.");
                }

                if (!model.EnemyBaseSkills.ContainsKey(trigger.TriggeredSkillId))
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown triggered skill '{trigger.TriggeredSkillId}'.");
                }
            }

            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.Buff, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemySkillSlot(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            EnemyRow enemy /* 적 */,
            string skillId /* 스킬 식별자 */,
            SkillSlot slot /* 스킬이나 유닛이 배치될 슬롯 */,
            HashSet<string> referencedSkillIds /* 참조된 스킬 식별자 목록 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            if (!model.EnemyBaseSkills.TryGetValue(skillId, out var skill)
                || skill == null
                || skill.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' references unknown base skill '{skillId}'.");
                return;
            }

            if (skill.Skill.SkillKind != PakuriCsvSkillKind.Active
                || skill.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' must reference an active skill, but '{skillId}' is passive.");
                return;
            }

            referencedSkillIds.Add(skillId);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyPassive(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            EnemyRow enemy /* 적 */,
            HashSet<string> referencedPassiveIds /* 참조된 패시브 식별자 목록 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            var passiveId = string.Empty;
            if (enemy.PassiveId != null)
            {
                passiveId = enemy.PassiveId.Trim();
            }
            if (!model.EnemyBaseSkills.TryGetValue(passiveId, out var passive)
                || passive == null
                || passive.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' references unknown passive_id '{passiveId}'.");
                return;
            }

            if (passive.Skill.SkillKind != PakuriCsvSkillKind.Passive
                || passive.Skill.RuntimeKind != SkillRuntimeKind.Passive
                || passive.Skill.Slot != SkillSlot.F)
            {
                errors.Add($"Enemy '{enemy.Id}' passive_id '{passiveId}' must reference an Enemy passive definition.");
            }

            if (passive.PassiveModifierKind == EnemyPassiveModifierKind.None)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a supported modifier_kind.");
            }

            if (passive.PassiveModifierKind == EnemyPassiveModifierKind.DamageUp
                && !passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveId}' requires attribute for DamageUp.");
            }

            if (passive.PassiveModifierKind != EnemyPassiveModifierKind.DamageUp
                && passive.PassiveModifierKind != EnemyPassiveModifierKind.DefenseUp
                && passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveId}' cannot use attribute with '{passive.PassiveModifierKind}'.");
            }

            if (passive.PassiveModifierValue <= 0f)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a positive modifier_value.");
            }

            referencedPassiveIds.Add(passiveId);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyCombatStartTrigger(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string skillId /* 스킬 식별자 */,
            SkillRuntimeKind runtimeKind /* 런타임 종류 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            var count = 0;
            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (trigger.Enabled
                    && trigger.TriggerEvent == SkillTriggerEvent.CombatStart
                    && string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(trigger.TriggeredSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && trigger.RuntimeKind == runtimeKind)
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add($"Enemy skill '{skillId}' requires exactly one enabled CombatStart trigger; found '{count}'.");
            }
        }
        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static SourceModel LoadSourceModel(CsvRuntimeCatalog sourceCatalog /* 발생 원본 데이터 목록 */)
        {
            var model = new SourceModel();

            var catalogMonsterTable = CsvTable.Load(sourceCatalog.CatalogMonsters, CatalogMonstersFileName);
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var projectileSkillAssets = sourceCatalog.MonsterSkillsProjectileFiles;
            var lineAttackSkillAssets = sourceCatalog.MonsterSkillsLineAttackFiles;
            var areaAttackSkillAssets = sourceCatalog.MonsterSkillsAreaAttackFiles;
            var singleAttackSkillAssets = sourceCatalog.MonsterSkillsSingleAttackFiles;
            var buffSkillAssets = sourceCatalog.MonsterSkillsBuffFiles;
            var passiveSkillAssets = sourceCatalog.MonsterSkillsPassiveFiles;
            var skillTriggerAssets = sourceCatalog.MonsterSkillTriggerFiles;
            var skillGraphNodeAssets = sourceCatalog.MonsterSkillGraphNodeFiles;
            var skillNodeDefinitionTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitions,
                MonsterSkillNodeDefinitionsFileName);
            var skillNodeDefinitionParamTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitionParams,
                MonsterSkillNodeDefinitionParamsFileName);
            var projectileChoiceAssets = sourceCatalog.MonsterSkillChoicesProjectileFiles;
            var lineAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesLineAttackFiles;
            var areaAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesAreaAttackFiles;
            var singleAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesSingleAttackFiles;
            var buffChoiceAssets = sourceCatalog.MonsterSkillChoicesBuffFiles;
            var passiveChoiceAssets = sourceCatalog.MonsterSkillChoicesPassiveFiles;
            var statusEffectTable = CsvTable.Load(sourceCatalog.StatusEffects, StatusEffectsFileName);
            var enemyTable = CsvTable.Load(sourceCatalog.Enemies, EnemiesFileName);

            foreach (var record in catalogMonsterTable.Records)
            {
                var row = ParseCatalogEntry(record, "monster_id");
                AddUnique(model.CatalogMonsters, row.Id, row, record);
            }

            foreach (var record in monsterTable.Records)
            {
                var row = ParseMonsterRow(record);
                AddUnique(model.Monsters, row.Id, row, record);
            }

            foreach (var record in rewardChoiceTable.Records)
            {
                var row = ParseRewardChoiceRow(record);
                AddUnique(model.RewardChoices, row.Id, row, record);
            }

            LoadSkillRows(
                model,
                projectileSkillAssets,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillRows(
                model,
                lineAttackSkillAssets,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.LineAttack);
            LoadSkillRows(
                model,
                areaAttackSkillAssets,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillRows(
                model,
                singleAttackSkillAssets,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.SingleAttack);
            LoadSkillRows(
                model,
                buffSkillAssets,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillRows(
                model,
                passiveSkillAssets,
                PakuriCsvSkillKind.Passive,
                SkillRuntimeKind.Passive);

            foreach (var record in statusEffectTable.Records)
            {
                var row = ParseStatusEffectRow(record);
                AddUnique(model.StatusEffects, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionTable.Records)
            {
                var row = ParseSkillNodeTypeRow(record);
                AddUnique(model.SkillNodeTypes, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionParamTable.Records)
            {
                model.SkillNodeTypeParams.Add(ParseSkillNodeTypeParamRow(record));
            }

            for (var assetIndex = 0; assetIndex < skillTriggerAssets.Length; assetIndex++)
            {
                var skillTriggerTable = CsvTable.Load(
                    skillTriggerAssets[assetIndex],
                    GetTextAssetCsvTableName(skillTriggerAssets[assetIndex]));
                foreach (var record in skillTriggerTable.Records)
                {
                    var row = ParseSkillTriggerRow(record);
                    AddUnique(model.SkillTriggers, row.Id, row, record);
                }
            }

            LoadSkillChoiceRows(
                model,
                projectileChoiceAssets,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillChoiceRows(
                model,
                lineAttackChoiceAssets,
                SkillRuntimeKind.LineAttack);
            LoadSkillChoiceRows(
                model,
                areaAttackChoiceAssets,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillChoiceRows(
                model,
                singleAttackChoiceAssets,
                SkillRuntimeKind.SingleAttack);
            LoadSkillChoiceRows(
                model,
                buffChoiceAssets,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillChoiceRows(
                model,
                passiveChoiceAssets,
                SkillRuntimeKind.Passive);

            for (var assetIndex = 0; assetIndex < skillGraphNodeAssets.Length; assetIndex++)
            {
                var graphNodeTable = CsvTable.Load(
                    skillGraphNodeAssets[assetIndex],
                    GetTextAssetCsvTableName(skillGraphNodeAssets[assetIndex]));
                foreach (var record in graphNodeTable.Records)
                {
                    model.SkillGraphNodes.Add(ParseSkillGraphNodeRow(record));
                }
            }

            MaterializeSkillGraphRows(model);

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyRow(record);
                AddUnique(model.Enemies, row.Id, row, record);
            }

            var enemyBaseAssets = sourceCatalog.EnemySkillBaseFiles;
            for (var assetIndex = 0; assetIndex < enemyBaseAssets.Length; assetIndex++)
            {
                var asset = enemyBaseAssets[assetIndex];
                var tableName = GetTextAssetCsvTableName(asset);
                var table = CsvTable.Load(asset, tableName);
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyBaseSkillRow(record, tableName);
                    AddUnique(model.EnemyBaseSkills, row.Skill.Id, row, record);
                }
            }

            var enemyTriggerAssets = sourceCatalog.EnemySkillTriggerFiles;
            for (var assetIndex = 0; assetIndex < enemyTriggerAssets.Length; assetIndex++)
            {
                var asset = enemyTriggerAssets[assetIndex];
                var table = CsvTable.Load(asset, GetTextAssetCsvTableName(asset));
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyTriggerRow(record);
                    AddUnique(model.EnemyTriggers, row.Id, row, record);
                }
            }

            return model;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillRows(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            TextAsset[] skillAssets /* 스킬 에셋 목록 */,
            PakuriCsvSkillKind skillKind /* 스킬 종류 */,
            params SkillRuntimeKind[] allowedRuntimeKinds /* 허용된 런타임 종류 목록 여부 */)
        {
            for (var assetIndex = 0; assetIndex < skillAssets.Length; assetIndex++)
            {
                var skillAsset = skillAssets[assetIndex];
                LoadSkillRows(
                    model,
                    skillAsset,
                    GetTextAssetCsvTableName(skillAsset),
                    skillKind,
                    allowedRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillRows(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            TextAsset skillAsset /* 스킬 에셋 */,
            string tableName /* CSV 표 이름 */,
            PakuriCsvSkillKind skillKind /* 스킬 종류 */,
            params SkillRuntimeKind[] allowedRuntimeKinds /* 허용된 런타임 종류 목록 여부 */)
        {
            var skillTable = CsvTable.Load(skillAsset, tableName);
            foreach (var record in skillTable.Records)
            {
                var row = ParseSkillRow(record, skillKind);
                if (!IsAllowedSkillRuntimeKind(row.RuntimeKind, allowedRuntimeKinds))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains skill '{row.Id}' with unsupported runtime_kind '{row.RuntimeKind}'.",
                        new List<string>
                        {
                            $"Move skill '{row.Id}' to the split monster skill CSV that owns runtime_kind '{row.RuntimeKind}'."
                        });
                }

                AddUnique(model.Skills, row.Id, row, record);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillChoiceRows(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            TextAsset[] choiceAssets /* 선택지 에셋 목록 */,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds /* 허용된 소유자 런타임 종류 목록 여부 */)
        {
            for (var assetIndex = 0; assetIndex < choiceAssets.Length; assetIndex++)
            {
                var choiceAsset = choiceAssets[assetIndex];
                LoadSkillChoiceRows(
                    model,
                    choiceAsset,
                    GetTextAssetCsvTableName(choiceAsset),
                    allowedOwnerRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillChoiceRows(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            TextAsset choiceAsset /* 선택지 에셋 */,
            string tableName /* CSV 표 이름 */,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds /* 허용된 소유자 런타임 종류 목록 여부 */)
        {
            var choiceTable = CsvTable.Load(choiceAsset, tableName);
            foreach (var record in choiceTable.Records)
            {
                var row = ParseSkillChoiceRow(record);
                if (!model.Skills.TryGetValue(row.SkillId, out var ownerSkill))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains choice '{row.Id}' for unknown owner skill '{row.SkillId}'.",
                        new List<string>
                        {
                            $"Define skill '{row.SkillId}' in the split monster skill CSV before adding its choices."
                        });
                }

                if (!IsAllowedSkillRuntimeKind(ownerSkill.RuntimeKind, allowedOwnerRuntimeKinds))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains choice '{row.Id}' for skill '{row.SkillId}' with unsupported owner runtime_kind '{ownerSkill.RuntimeKind}'.",
                        new List<string>
                        {
                            $"Move choice '{row.Id}' to the split monster skill choice CSV that owns runtime_kind '{ownerSkill.RuntimeKind}'."
                        });
                }

                AddUnique(model.SkillChoices, row.Id, row, record);
            }
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool IsAllowedSkillRuntimeKind(
            SkillRuntimeKind runtimeKind /* 런타임 종류 */,
            SkillRuntimeKind[] allowedRuntimeKinds /* 허용된 런타임 종류 목록 여부 */)
        {
            for (var i = 0; i < allowedRuntimeKinds.Length; i++)
            {
                if (allowedRuntimeKinds[i] == runtimeKind)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * TextAsset 이름으로 CSV 테이블 이름을 만든다.
         */
        internal static string GetTextAssetCsvTableName(TextAsset asset /* 읽을 텍스트 에셋 */)
        {
            if (asset == null)
            {
                throw new CsvFatalException("CSV runtime catalog contains a null TextAsset reference.");
            }

            if (string.IsNullOrWhiteSpace(asset.name))
            {
                throw new CsvFatalException("CSV runtime catalog contains a TextAsset without a name.");
            }

            if (asset.name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return asset.name;
            }

            return asset.name + ".csv";
        }

        /*
         * 중복 ID를 거부하고 원본 행을 사전에 추가한다.
         */
        internal static void AddUnique<T>(Dictionary<string, T> dictionary /* 사전 */, string id /* 대상을 구분하는 식별자 */, T value /* 처리할 값 */, CsvRecord record /* 읽을 CSV 행 */)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new CsvFatalException(
                    $"CSV row {record.RowNumber} in '{record.TableName}' is missing a required id value.");
            }

            if (dictionary.ContainsKey(id))
            {
                throw new CsvFatalException(
                    $"CSV row {record.RowNumber} in '{record.TableName}' uses duplicate id '{id}'.");
            }

            dictionary.Add(id, value);
        }
    }
}
