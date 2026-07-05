using Pakuri.Combat;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class StatusPayloadRow
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
            public string StatusConditionalTargetStatusId;
            public float StatusConditionalStatusChanceBonus;
            public string StatusConditionalIncomingSkillRuntimeKinds;
            public string StatusConditionalOutgoingSkillRuntimeKinds;
            public string StatusAppliedStatusDurationBonusStatusId;
            public float StatusAppliedStatusDurationBonus;
            public float StatusOutgoingAdditionalDamageMultiplier;
            public DamageAttribute StatusOutgoingAdditionalDamageTriggerAttribute;
            public DamageAttribute StatusOutgoingAdditionalDamageAttribute;
        }

        private static StatusPayloadRow ReadStatusPayload(
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
                payload.StatusConditionalTargetStatusId = ReadStatusString(record, "status_conditional_target_status_id", allowMissingColumns);
                payload.StatusConditionalStatusChanceBonus = ReadStatusFloat(record, "status_conditional_status_chance_bonus", allowMissingColumns);
                payload.StatusConditionalIncomingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_incoming_skill_runtime_kinds");
                payload.StatusConditionalOutgoingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_outgoing_skill_runtime_kinds");
                payload.StatusAppliedStatusDurationBonusStatusId = ReadStatusString(record, "status_applied_status_duration_bonus_status_id", allowMissingColumns);
                payload.StatusAppliedStatusDurationBonus = ReadStatusFloat(record, "status_applied_status_duration_bonus", allowMissingColumns);
                payload.StatusOutgoingAdditionalDamageMultiplier = ReadStatusFloat(record, "status_outgoing_additional_damage_multiplier", allowMissingColumns);
                payload.StatusOutgoingAdditionalDamageTriggerAttribute = ReadOptionalEnumIfColumnExists(record, "status_outgoing_additional_damage_trigger_attribute", DamageAttribute.Physical);
                payload.StatusOutgoingAdditionalDamageAttribute = ReadOptionalEnumIfColumnExists(record, "status_outgoing_additional_damage_attribute", DamageAttribute.Physical);
            }

            return payload;
        }

        private static string ReadStatusString(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            return allowMissingColumns
                ? ReadOptionalStringIfColumnExists(record, columnName)
                : record.ReadString(columnName);
        }

        private static float ReadStatusFloat(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            return allowMissingColumns
                ? ReadOptionalFloatIfColumnExists(record, columnName)
                : record.ReadFloat(columnName);
        }

        private static int ReadStatusInt(CsvRecord record, string columnName, bool allowMissingColumns)
        {
            return allowMissingColumns
                ? ReadOptionalIntIfColumnExists(record, columnName)
                : record.ReadInt(columnName);
        }
    }
}
