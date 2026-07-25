/* ChoiceNodeDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Choices
{
    public class ChoiceNodeDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 ChoiceNodeDefinition 불변 정의를 구성한다. */
        internal ChoiceNodeDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(monster_id),
                nameof(owner_kind),
                nameof(owner_id),
                nameof(graph_kind),
                nameof(node_type_id));
        }

        public string monster_id => RequiredString(nameof(monster_id));

        public string owner_kind => RequiredString(nameof(owner_kind));

        public string owner_id => RequiredString(nameof(owner_id));

        public string graph_kind => RequiredString(nameof(graph_kind));

        public int? graph_index => OptionalInt(nameof(graph_index));

        public string target_skill_id => OptionalString(nameof(target_skill_id));

        public int? node_order => OptionalInt(nameof(node_order));

        public string node_type_id => RequiredString(nameof(node_type_id));

        public string arg_1 => OptionalString(nameof(arg_1));

        public string arg_2 => OptionalString(nameof(arg_2));

        public string arg_3 => OptionalString(nameof(arg_3));

        public string arg_4 => OptionalString(nameof(arg_4));

        public string arg_5 => OptionalString(nameof(arg_5));

        public string arg_6 => OptionalString(nameof(arg_6));

        public string arg_7 => OptionalString(nameof(arg_7));

        public string arg_8 => OptionalString(nameof(arg_8));

        public string arg_9 => OptionalString(nameof(arg_9));

        public string arg_10 => OptionalString(nameof(arg_10));

        public string arg_11 => OptionalString(nameof(arg_11));

        public string arg_12 => OptionalString(nameof(arg_12));

        public string excludes_active_choice_id =>
            OptionalString(nameof(excludes_active_choice_id));
    }
}
