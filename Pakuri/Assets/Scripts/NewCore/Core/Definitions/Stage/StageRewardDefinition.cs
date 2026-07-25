/* StageRewardDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Stage
{
    public sealed class StageRewardDefinition : StageDefinition
    {
        /* CSV 레코드의 열 값을 읽어 StageRewardDefinition 불변 정의를 구성한다. */
        internal StageRewardDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(reward_rule_id),
                nameof(combat_type),
                nameof(guaranteed_prisoner_source));
        }

        public string reward_rule_id => RequiredString(nameof(reward_rule_id));

        public string combat_type => RequiredString(nameof(combat_type));

        public int? stage => OptionalInt(nameof(stage));

        public int? gold => OptionalInt(nameof(gold));

        public int? dark_trace => OptionalInt(nameof(dark_trace));

        public float? prisoner_count_1_chance =>
            OptionalFloat(nameof(prisoner_count_1_chance));

        public float? prisoner_count_2_chance =>
            OptionalFloat(nameof(prisoner_count_2_chance));

        public float? prisoner_count_3_chance =>
            OptionalFloat(nameof(prisoner_count_3_chance));

        public float? manifest_success_chance =>
            OptionalFloat(nameof(manifest_success_chance));

        public int? elite_bonus_prisoners => OptionalInt(nameof(elite_bonus_prisoners));

        public int? artifact_choice_count => OptionalInt(nameof(artifact_choice_count));

        public string guaranteed_prisoner_source =>
            RequiredString(nameof(guaranteed_prisoner_source));

        public string notes => OptionalString(nameof(notes));
    }
}
