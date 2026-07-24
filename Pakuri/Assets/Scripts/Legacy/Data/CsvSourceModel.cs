using System;
using System.Collections.Generic;
using Pakuri.Combat;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.SkillGraphParser;


/*
 * 파싱된 CSV 행 형식과 카탈로그 생성 전 원본 데이터 모음을 정의한다.
 */
namespace Pakuri.Data
{
    internal enum PakuriCsvSkillKind
    {
        Active,
        Passive
    }

    /*
     * CSV 원본 행과 로딩 중간 모델을 정의한다.
     */
    internal static class CsvSourceModel
    {
        /*
         * 각 CSV에서 읽은 행을 종류별 조회 표로 모으는 중간 모델이다.
         */
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

        /*
         * 카탈로그 표시 순서와 실제 정의 ID의 연결을 보관한다.
         */
        internal class CatalogEntryRow
        {
            public string Id;
            public string RefId;
            public int SortOrder;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static CatalogEntryRow ParseCatalogEntry(CsvRecord record /* 읽을 CSV 행 */, string refColumnName /* 참조 열 이름 */)
        {
            return new CatalogEntryRow
            {
                Id = record.ReadRequiredString("id"),
                RefId = record.ReadRequiredString(refColumnName),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateCatalogEntries<T>(
            Dictionary<string, CatalogEntryRow> entries /* 등록 정보 목록 */,
            Dictionary<string, T> targetLookup /* 대상 조회표 */,
            string tableName /* CSV 표 이름 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            if (entries.Count == 0)
            {
                errors.Add($"{tableName} has no rows.");
                return;
            }

            foreach (var entry in entries.Values)
            {
                if (!targetLookup.ContainsKey(entry.RefId))
                {
                    errors.Add($"{tableName} entry '{entry.Id}' references unknown id '{entry.RefId}'.");
                }
            }
        }

        /*
         * 상태 효과 CSV 한 행의 동작과 능력치 변경값을 보관한다.
         */
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static StatusEffectRow ParseStatusEffectRow(CsvRecord record /* 읽을 CSV 행 */)
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

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadDamageAttribute(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, out DamageAttribute attribute /* 피해 속성 */)
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

        /*
         * 스킬이 적용할 상태 효과 값과 적용 규칙을 보관한다.
         */
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
            public string StatusConditionalIncomingSkillRuntimeKinds;
            public string StatusConditionalOutgoingSkillRuntimeKinds;
            public string StatusAppliedStatusDurationBonusStatusId;
            public float StatusAppliedStatusDurationBonus;
            public float StatusOutgoingAdditionalDamageMultiplier;
            public DamageAttribute StatusOutgoingAdditionalDamageTriggerAttribute;
            public DamageAttribute StatusOutgoingAdditionalDamageAttribute;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static StatusPayloadRow ReadStatusPayload(
            CsvRecord record /* 읽을 CSV 행 */,
            bool includeEffectOnlyModifiers /* 포함 효과 한정 보정 목록 여부 */,
            bool allowMissingColumns = false /* 허용 누락된 열 목록 여부 */)
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
                payload.StatusConditionalIncomingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_incoming_skill_runtime_kinds");
                payload.StatusConditionalOutgoingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_outgoing_skill_runtime_kinds");
                payload.StatusAppliedStatusDurationBonusStatusId = ReadStatusString(record, "status_applied_status_duration_bonus_status_id", allowMissingColumns);
                payload.StatusAppliedStatusDurationBonus = ReadStatusFloat(record, "status_applied_status_duration_bonus", allowMissingColumns);
                payload.StatusOutgoingAdditionalDamageMultiplier = ReadStatusFloat(record, "status_outgoing_additional_damage_multiplier", allowMissingColumns);
                payload.StatusOutgoingAdditionalDamageTriggerAttribute = ReadOptionalEnum(record, "status_outgoing_additional_damage_trigger_attribute", DamageAttribute.Physical);
                payload.StatusOutgoingAdditionalDamageAttribute = ReadOptionalEnum(record, "status_outgoing_additional_damage_attribute", DamageAttribute.Physical);
            }

            return payload;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static string ReadStatusString(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, bool allowMissingColumns /* 허용 누락된 열 목록 여부 */)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalStringIfColumnExists(record, columnName);
            }

            return record.ReadString(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadStatusFloat(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, bool allowMissingColumns /* 허용 누락된 열 목록 여부 */)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalFloatIfColumnExists(record, columnName);
            }

            return record.ReadFloat(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static int ReadStatusInt(CsvRecord record /* 읽을 CSV 행 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, bool allowMissingColumns /* 허용 누락된 열 목록 여부 */)
        {
            if (allowMissingColumns)
            {
                return ReadOptionalIntIfColumnExists(record, columnName);
            }

            return record.ReadInt(columnName);
        }
    }
}
