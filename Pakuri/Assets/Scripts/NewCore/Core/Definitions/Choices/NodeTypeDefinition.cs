/* NodeTypeDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Choices
{
    public class NodeTypeDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 NodeTypeDefinition 불변 정의를 구성한다. */
        internal NodeTypeDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(node_type_id), nameof(handler_id), nameof(node_kind));
        }

        public string node_type_id => RequiredString(nameof(node_type_id));

        public string handler_id => RequiredString(nameof(handler_id));

        public string node_kind => RequiredString(nameof(node_kind));

        public string runtime_support_state => OptionalString(nameof(runtime_support_state));

        public string runtime_support_notes => OptionalString(nameof(runtime_support_notes));
    }
}
