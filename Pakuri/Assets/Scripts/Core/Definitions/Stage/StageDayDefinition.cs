/* StageDayDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Stage
{
    public class StageDayDefinition : StageDefinition
    {
        /* CSV 레코드의 열 값을 읽어 StageDayDefinition 불변 정의를 구성한다. */
        internal StageDayDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public int? stage => OptionalInt(nameof(stage));

        public int? day => OptionalInt(nameof(day));

        public string day_key => OptionalString(nameof(day_key));

        public string combat_type => OptionalString(nameof(combat_type));

        public string encounter_id => OptionalString(nameof(encounter_id));

        public string reward_rule_id => OptionalString(nameof(reward_rule_id));

        public float? elite_option_chance => OptionalFloat(nameof(elite_option_chance));

        public bool? shop_option_enabled => OptionalBool(nameof(shop_option_enabled));

        public bool? event_roll_enabled => OptionalBool(nameof(event_roll_enabled));

        public string notes => OptionalString(nameof(notes));
    }
}
