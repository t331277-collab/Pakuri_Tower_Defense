/*
 * 역할: 파싱된 CSV 원본 계약.
 * 책임: 카탈로그 생성 전의 원본 행·조회 컬렉션·그래프 행·원본 모델 소유권을 정의한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// PakuriCsvSkillKind에서 지원하는 값의 종류를 정의한다.
    internal enum PakuriCsvSkillKind
    {
        Active,
        Passive
    }

    /// CsvSourceModel가 소유하는 데이터와 동작을 캡슐화한다.
    internal static class CsvSourceModel
    {

        /// SourceModel가 소유하는 데이터와 동작을 캡슐화한다.
        internal class SourceModel
        {
            public readonly Dictionary<string, CatalogEntryRow> CatalogMonsters = new Dictionary<string, CatalogEntryRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, MonsterRow> Monsters = new Dictionary<string, MonsterRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, RewardChoiceRow> RewardChoices = new Dictionary<string, RewardChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillRow> Skills = new Dictionary<string, SkillRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillTriggerRow> SkillTriggers = new Dictionary<string, SkillTriggerRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillChoiceRow> SkillChoices = new Dictionary<string, SkillChoiceRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, SkillNodeRow> SkillNodes = new Dictionary<string, SkillNodeRow>(StringComparer.OrdinalIgnoreCase);
            public readonly List<SkillNodeParamRow> SkillNodeParams = new List<SkillNodeParamRow>();
            public readonly Dictionary<string, SkillNodeTypeRow> SkillNodeTypes = new Dictionary<string, SkillNodeTypeRow>(StringComparer.OrdinalIgnoreCase);
            public readonly List<SkillNodeTypeParamRow> SkillNodeTypeParams = new List<SkillNodeTypeParamRow>();
            public readonly List<SkillGraphNodeRow> SkillGraphNodes = new List<SkillGraphNodeRow>();
            public readonly Dictionary<string, StatusEffectRow> StatusEffects = new Dictionary<string, StatusEffectRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyRow> Enemies = new Dictionary<string, EnemyRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyBaseSkillRow> EnemyBaseSkills = new Dictionary<string, EnemyBaseSkillRow>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, EnemyTriggerRow> EnemyTriggers = new Dictionary<string, EnemyTriggerRow>(StringComparer.OrdinalIgnoreCase);
        }

        /// CatalogEntryRow에 해당하는 CSV 한 행을 표현한다.
        internal class CatalogEntryRow
        {
            public string Id;
            public string RefId;
            public int SortOrder;
        }

        /// 전달된 런타임 입력값을 사용해 CatalogEntry 값을 런타임 표현으로 파싱한다.
        internal static CatalogEntryRow ParseCatalogEntry(CsvRecord record, string refColumnName)
        {
            return new CatalogEntryRow
            {
                Id = record.ReadRequiredString("id"),
                RefId = record.ReadRequiredString(refColumnName),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        /// StatusEffectRow에 해당하는 CSV 한 행을 표현한다.
        internal class StatusEffectRow
        {
            public string Id;
            public string Label;
            public StatusEffectClassification Classification;
            public bool HasAttribute;
            public DamageAttribute Attribute;
            public float DefaultDurationSeconds;
            public bool IsPermanent;
            public int MaxStacks;
            public int BaseStackAmount;
            public bool CanMove;
            public bool CanAct;
            public bool CanUseSpecialSkill;
            public float ActionSpeedBonusPerStack;
            public float MoveSpeedBonusPerStack;
            public float AttackPowerBonusPerStack;
            public float DamageTakenBonusPerStack;
            public float CriticalDamageTakenBonusPerStack;
            public float CriticalResistanceBonusPerStack;
            public float ElementResistReductionPerStack;
            public float ElementDamageTakenBonusPerStack;
            public string StatusEffectPrefabPath;
        }

        /// 전달된 record 값을 사용해 StatusEffectRow 값을 런타임 표현으로 파싱한다.
        internal static StatusEffectRow ParseStatusEffectRow(CsvRecord record)
        {
            return new StatusEffectRow
            {
                Id = record.ReadRequiredString("status_effect_id"),
                Label = record.ReadRequiredString("status_effect_label"),
                Classification = record.ReadEnum<StatusEffectClassification>("effect_type"),
                HasAttribute = TryReadDamageAttribute(record, "attribute", out var attribute),
                Attribute = attribute,
                DefaultDurationSeconds = record.ReadFloat("default_duration_seconds"),
                IsPermanent = record.ReadBool("is_permanent"),
                MaxStacks = record.ReadInt("max_stacks"),
                BaseStackAmount = record.ReadInt("base_stack_amount"),
                CanMove = record.ReadBool("can_move"),
                CanAct = record.ReadBool("can_act"),
                CanUseSpecialSkill = record.ReadBool("can_use_special_skill"),
                ActionSpeedBonusPerStack = record.ReadFloat("action_speed_bonus_per_stack"),
                MoveSpeedBonusPerStack = record.ReadFloat("move_speed_bonus_per_stack"),
                AttackPowerBonusPerStack = record.ReadFloat("attack_power_bonus_per_stack"),
                DamageTakenBonusPerStack = record.ReadFloat("damage_taken_bonus_per_stack"),
                CriticalDamageTakenBonusPerStack = record.ReadFloat("critical_damage_taken_bonus_per_stack"),
                CriticalResistanceBonusPerStack = record.ReadFloat("critical_resistance_bonus_per_stack"),
                ElementResistReductionPerStack = record.ReadFloat("element_resist_reduction_per_stack"),
                ElementDamageTakenBonusPerStack = record.ReadFloat("element_damage_taken_bonus_per_stack"),
                StatusEffectPrefabPath = record.ReadString("status_effect_prefab_path")
            };
        }

        /// 전달된 런타임 입력값을 사용해 ReadDamageAttribute 작업을 시도하고 성공 여부를 반환한다.
        internal static bool TryReadDamageAttribute(CsvRecord record, string columnName, out DamageAttribute attribute)
        {
            attribute = default;
            var value = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "None", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            attribute = record.ReadEnum<DamageAttribute>(columnName);
            return true;
        }

        /// StatusPayloadRow에 해당하는 CSV 한 행을 표현한다.
        internal class StatusPayloadRow
        {
            public string StatusEffectId;
            public float StatusChance;
            public string StatusEffectLabel;
            public string StatusEffectPrefabPath;
            public float StatusDurationSeconds;
            public int StatusMaxStacks;
            public int StatusStackAmount;
            public string StatusTargetScope;
            public string StatusMergePolicy;
            public string ShieldAmountRefreshPolicy;
            public float StatusActionSpeedBonus;
            public float StatusMoveSpeedBonus;
            public float StatusAttackPowerBonus;
            public float StatusSpellPowerBonus;
            public float StatusDamageBonusRate;
            public float StatusShieldReceivedBonus;
            public float StatusDamageTakenBonus;
            public float StatusCriticalDamageTakenBonus;
            public float StatusCriticalDamageBonus;
            public float StatusAilmentResistanceBonus;
            public float StatusCriticalResistanceBonus;
            public float StatusElementResistReduction;
            public float StatusFlatElementResistReduction;
            public float StatusElementDamageTakenBonus;
            public float StatusCriticalChanceBonus;
            public float StatusConditionalStatusChanceBonus;
        }

        /// 전달된 런타임 입력값을 사용해 StatusPayload를 읽는다.
        internal static StatusPayloadRow ReadStatusPayload(
            CsvRecord record,
            bool includeEffectOnlyModifiers,
            bool allowMissingColumns = false)
        {
            var payload = new StatusPayloadRow
            {
                StatusEffectId = ReadStatusString(record, "status_effect_id", allowMissingColumns),
                StatusChance = ReadStatusFloat(record, "status_chance", allowMissingColumns),
                StatusEffectLabel = ReadStatusString(record, "status_effect_label", allowMissingColumns),
                StatusEffectPrefabPath = ReadStatusString(record, "status_effect_prefab_path", allowMissingColumns),
                StatusDurationSeconds = ReadStatusFloat(record, "status_duration_seconds", allowMissingColumns),
                StatusMaxStacks = ReadStatusInt(record, "status_max_stacks", allowMissingColumns),
                StatusStackAmount = ReadStatusInt(record, "status_stack_amount", allowMissingColumns),
                StatusTargetScope = ReadStatusString(record, "status_target_scope", allowMissingColumns),
                StatusMergePolicy = ReadStatusString(record, "status_merge_policy", allowMissingColumns),
                ShieldAmountRefreshPolicy = ReadStatusString(record, "shield_amount_refresh_policy", allowMissingColumns),
                StatusActionSpeedBonus = ReadStatusFloat(record, "status_action_speed_bonus", allowMissingColumns),
                StatusMoveSpeedBonus = ReadStatusFloat(record, "status_move_speed_bonus", allowMissingColumns),
                StatusAttackPowerBonus = ReadStatusFloat(record, "status_attack_power_bonus", allowMissingColumns),
                StatusDamageTakenBonus = ReadStatusFloat(record, "status_damage_taken_bonus", allowMissingColumns),
                StatusCriticalDamageTakenBonus = ReadStatusFloat(record, "status_critical_damage_taken_bonus", allowMissingColumns),
                StatusAilmentResistanceBonus = ReadStatusFloat(record, "status_ailment_resistance_bonus", allowMissingColumns),
                StatusCriticalResistanceBonus = ReadStatusFloat(record, "status_critical_resistance_bonus", allowMissingColumns),
                StatusElementResistReduction = ReadStatusFloat(record, "status_element_resist_reduction", allowMissingColumns),
                StatusFlatElementResistReduction = ReadStatusFloat(record, "status_flat_element_resist_reduction", allowMissingColumns),
                StatusElementDamageTakenBonus = ReadStatusFloat(record, "status_element_damage_taken_bonus", allowMissingColumns)
            };

            if (includeEffectOnlyModifiers)
            {
                payload.StatusSpellPowerBonus = ReadStatusFloat(record, "status_spell_power_bonus", allowMissingColumns);
                payload.StatusDamageBonusRate = ReadStatusFloat(record, "status_damage_bonus_rate", allowMissingColumns);
                payload.StatusShieldReceivedBonus = ReadStatusFloat(record, "status_shield_received_bonus", allowMissingColumns);
                payload.StatusCriticalChanceBonus = ReadStatusFloat(record, "status_critical_chance_bonus", allowMissingColumns);
                payload.StatusCriticalDamageBonus = ReadOptionalFloatIfColumnExists(record, "status_critical_damage_bonus");
                payload.StatusConditionalStatusChanceBonus = ReadStatusFloat(record, "status_conditional_status_chance_bonus", allowMissingColumns);
            }

            return payload;
        }

        /// 전달된 런타임 입력값을 사용해 StatusString를 읽는다.
        internal static string ReadStatusString(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalStringIfColumnExists(record, columnName);
            }

            return record.ReadString(columnName);
        }

        /// 전달된 런타임 입력값을 사용해 StatusFloat를 읽는다.
        internal static float ReadStatusFloat(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalFloatIfColumnExists(record, columnName);
            }

            return record.ReadFloat(columnName);
        }

        /// 전달된 런타임 입력값을 사용해 StatusInt를 읽는다.
        internal static int ReadStatusInt(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalIntIfColumnExists(record, columnName);
            }

            return record.ReadInt(columnName);
        }
    }
}
