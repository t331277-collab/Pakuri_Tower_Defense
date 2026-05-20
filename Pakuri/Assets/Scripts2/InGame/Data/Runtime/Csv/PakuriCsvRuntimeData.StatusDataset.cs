using Pakuri.Combat;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private sealed class StatusEffectRow
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
        }

        private static StatusEffectRow ParseStatusEffectRow(CsvRecord record)
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
                ElementDamageTakenBonusPerStack = record.ReadFloat("element_damage_taken_bonus_per_stack")
            };
        }

        private static bool TryReadDamageAttribute(CsvRecord record, string columnName, out DamageAttribute attribute)
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
    }
}
