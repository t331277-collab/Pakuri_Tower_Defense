/* NodeParamDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Choices
{
    public class NodeParamDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 NodeParamDefinition 불변 정의를 구성한다. */
        internal NodeParamDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string node_type_id => OptionalString(nameof(node_type_id));

        public int? param_order => OptionalInt(nameof(param_order));

        public string param_key => OptionalString(nameof(param_key));

        public string value_type => OptionalString(nameof(value_type));

        public bool? required => OptionalBool(nameof(required));

        public string allowed_values => OptionalString(nameof(allowed_values));
    }
}
