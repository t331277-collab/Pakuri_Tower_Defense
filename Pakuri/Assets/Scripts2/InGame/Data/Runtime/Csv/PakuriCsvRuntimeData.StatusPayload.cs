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

        private static StatusPayloadRow ReadStatusPayload(CsvRecord record, bool includeEffectOnlyModifiers)
        {
            var payload = new StatusPayloadRow
            {
                StatusEffectId = record.ReadString("status_effect_id"),
                StatusChance = record.ReadFloat("status_chance"),
                StatusEffectLabel = record.ReadString("status_effect_label"),
                StatusEffectPrefabPath = record.ReadString("status_effect_prefab_path"),
                StatusDurationSeconds = record.ReadFloat("status_duration_seconds"),
                StatusMaxStacks = record.ReadInt("status_max_stacks"),
                StatusStackAmount = record.ReadInt("status_stack_amount"),
                StatusTargetScope = record.ReadString("status_target_scope"),
                StatusMergePolicy = record.ReadString("status_merge_policy"),
                ShieldAmountRefreshPolicy = record.ReadString("shield_amount_refresh_policy"),
                StatusActionSpeedBonus = record.ReadFloat("status_action_speed_bonus"),
                StatusMoveSpeedBonus = record.ReadFloat("status_move_speed_bonus"),
                StatusAttackPowerBonus = record.ReadFloat("status_attack_power_bonus"),
                StatusDamageTakenBonus = record.ReadFloat("status_damage_taken_bonus"),
                StatusCriticalDamageTakenBonus = record.ReadFloat("status_critical_damage_taken_bonus"),
                StatusAilmentResistanceBonus = record.ReadFloat("status_ailment_resistance_bonus"),
                StatusCriticalResistanceBonus = record.ReadFloat("status_critical_resistance_bonus"),
                StatusElementResistReduction = record.ReadFloat("status_element_resist_reduction"),
                StatusFlatElementResistReduction = record.ReadFloat("status_flat_element_resist_reduction"),
                StatusElementDamageTakenBonus = record.ReadFloat("status_element_damage_taken_bonus")
            };

            if (includeEffectOnlyModifiers)
            {
                payload.StatusSpellPowerBonus = record.ReadFloat("status_spell_power_bonus");
                payload.StatusDamageBonusRate = record.ReadFloat("status_damage_bonus_rate");
                payload.StatusShieldReceivedBonus = record.ReadFloat("status_shield_received_bonus");
                payload.StatusCriticalChanceBonus = record.ReadFloat("status_critical_chance_bonus");
                payload.StatusCriticalDamageBonus = ReadOptionalFloatIfColumnExists(record, "status_critical_damage_bonus");
                payload.StatusConditionalTargetStatusId = record.ReadString("status_conditional_target_status_id");
                payload.StatusConditionalStatusChanceBonus = record.ReadFloat("status_conditional_status_chance_bonus");
                payload.StatusConditionalIncomingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_incoming_skill_runtime_kinds");
                payload.StatusConditionalOutgoingSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "status_conditional_outgoing_skill_runtime_kinds");
                payload.StatusAppliedStatusDurationBonusStatusId = record.ReadString("status_applied_status_duration_bonus_status_id");
                payload.StatusAppliedStatusDurationBonus = record.ReadFloat("status_applied_status_duration_bonus");
                payload.StatusOutgoingAdditionalDamageMultiplier = record.ReadFloat("status_outgoing_additional_damage_multiplier");
                payload.StatusOutgoingAdditionalDamageTriggerAttribute = record.ReadEnum<DamageAttribute>("status_outgoing_additional_damage_trigger_attribute");
                payload.StatusOutgoingAdditionalDamageAttribute = record.ReadEnum<DamageAttribute>("status_outgoing_additional_damage_attribute");
            }

            return payload;
        }
    }
}
