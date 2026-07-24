namespace Pakuri.NewCore.Definitions.Status
{
    public sealed class StatusDefinition : CsvDefinition
    {
        internal StatusDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(status_effect_id),
                nameof(status_effect_label),
                nameof(effect_type),
                nameof(attribute));
        }

        public string status_effect_id => RequiredString(nameof(status_effect_id));

        public string status_effect_label => RequiredString(nameof(status_effect_label));

        public string effect_type => RequiredString(nameof(effect_type));

        public string attribute => RequiredString(nameof(attribute));

        public float? default_duration_seconds => OptionalFloat(nameof(default_duration_seconds));

        public bool? is_permanent => OptionalBool(nameof(is_permanent));

        public int? max_stacks => OptionalInt(nameof(max_stacks));

        public int? base_stack_amount => OptionalInt(nameof(base_stack_amount));

        public bool? can_move => OptionalBool(nameof(can_move));

        public bool? can_act => OptionalBool(nameof(can_act));

        public bool? can_use_special_skill => OptionalBool(nameof(can_use_special_skill));

        public float? action_speed_bonus_per_stack =>
            OptionalFloat(nameof(action_speed_bonus_per_stack));

        public float? move_speed_bonus_per_stack =>
            OptionalFloat(nameof(move_speed_bonus_per_stack));

        public float? attack_power_bonus_per_stack =>
            OptionalFloat(nameof(attack_power_bonus_per_stack));

        public float? damage_taken_bonus_per_stack =>
            OptionalFloat(nameof(damage_taken_bonus_per_stack));

        public float? critical_damage_taken_bonus_per_stack =>
            OptionalFloat(nameof(critical_damage_taken_bonus_per_stack));

        public float? critical_resistance_bonus_per_stack =>
            OptionalFloat(nameof(critical_resistance_bonus_per_stack));

        public float? element_resist_reduction_per_stack =>
            OptionalFloat(nameof(element_resist_reduction_per_stack));

        public float? element_damage_taken_bonus_per_stack =>
            OptionalFloat(nameof(element_damage_taken_bonus_per_stack));

        public string status_effect_prefab_path =>
            OptionalString(nameof(status_effect_prefab_path));
    }
}
