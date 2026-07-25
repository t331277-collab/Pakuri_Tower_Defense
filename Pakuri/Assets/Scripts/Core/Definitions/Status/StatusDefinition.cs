/* StatusDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Status
{
    public class StatusDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 StatusDefinition 불변 정의를 구성한다. */
        internal StatusDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string status_effect_id => OptionalString(nameof(status_effect_id));

        public string status_effect_label => OptionalString(nameof(status_effect_label));

        public string effect_type => OptionalString(nameof(effect_type));

        public string attribute => OptionalString(nameof(attribute));

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
